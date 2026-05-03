using System.Text.Json;
using Cits;
using Cits.Domain.SystemSettings;
using Cits.Dtos;
using Cits.Extensions;
using Cits.IdGenerator;
using Cits.SystemSettings;
using MyApi.Application.Announcements.Dto;
using MyApi.Application.FileManagement;
using MyApi.Domain.Announcements;
using MyApi.Domain.Identities;

namespace MyApi.Application.Announcements;

public class AnnouncementAppService : IAnnouncementAppService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ICurrentUser _currentUser;
    private readonly IFreeSql _freeSql;
    private readonly IIdGenerator _idGenerator;
    private readonly RichTextImageReferenceService _richTextImageReferenceService;
    private readonly ISettingProvider _settingProvider;

    public AnnouncementAppService(
        IFreeSql freeSql,
        IIdGenerator idGenerator,
        ICurrentUser currentUser,
        ISettingProvider settingProvider,
        RichTextImageReferenceService richTextImageReferenceService)
    {
        _freeSql = freeSql;
        _idGenerator = idGenerator;
        _currentUser = currentUser;
        _settingProvider = settingProvider;
        _richTextImageReferenceService = richTextImageReferenceService;
    }

    public async Task<PagedResultDto<AnnouncementDto>> GetListAsync(QueryAnnouncementDto input)
    {
        var keyword = input.Keyword ?? string.Empty;
        var query = _freeSql.Select<SystemAnnouncement>()
            .Where(x => !x.IsDeleted)
            .WhereIf(!keyword.IsNullOrWhiteSpace(), x => x.Title.Contains(keyword) ||
                (x.Summary != null && x.Summary.Contains(keyword)))
            .WhereIf(input.IsPublished.HasValue, x => x.IsPublished == input.IsPublished)
            .WhereIf(input.Priority.HasValue, x => x.Priority == input.Priority);

        var count = await query.CountAsync();
        if (count == 0) return new PagedResultDto<AnnouncementDto>(count, []);

        var list = await query.OrderByDescending(x => x.CreationTime)
            .PageBy(input)
            .ToListAsync();
        return new PagedResultDto<AnnouncementDto>(count, list.ConvertAll(x => ToDto(x)));
    }

    public async Task<AnnouncementDto> GetAsync(Guid id)
    {
        var entity = await _freeSql.Select<SystemAnnouncement>()
            .Where(x => x.Id == id && !x.IsDeleted)
            .FirstAsync() ?? throw new UserFriendlyException("公告不存在");
        return ToDto(entity);
    }

    public async Task CreateAsync(AnnouncementCreateUpdateDto input)
    {
        ValidateInput(input);
        var entity = new SystemAnnouncement
        {
            Id = _idGenerator.Create(),
            CreatorUserId = _currentUser.Id,
            CreatorUserName = _currentUser.Surname,
            LastModifierUserId = _currentUser.Id,
            LastModifierUserName = _currentUser.Surname
        };
        ApplyInput(entity, input);

        await _freeSql.Insert(entity).ExecuteAffrowsAsync();
        await _richTextImageReferenceService.SyncAnnouncementImagesAsync(entity.Id, null, entity.ContentHtml);
    }

    public async Task UpdateAsync(Guid id, AnnouncementCreateUpdateDto input)
    {
        ValidateInput(input);
        var entity = await _freeSql.Select<SystemAnnouncement>()
            .Where(x => x.Id == id && !x.IsDeleted)
            .FirstAsync() ?? throw new UserFriendlyException("公告不存在");

        var oldHtml = entity.ContentHtml;
        ApplyInput(entity, input);
        entity.LastModifierUserId = _currentUser.Id;
        entity.LastModifierUserName = _currentUser.Surname;

        await _freeSql.Update<SystemAnnouncement>().SetSource(entity).ExecuteAffrowsAsync();
        await _richTextImageReferenceService.SyncAnnouncementImagesAsync(entity.Id, oldHtml, entity.ContentHtml);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _freeSql.Select<SystemAnnouncement>()
            .Where(x => x.Id == id && !x.IsDeleted)
            .FirstAsync();
        if (entity == null) return;

        await _freeSql.Update<SystemAnnouncement>()
            .Set(x => x.IsDeleted, true)
            .Where(x => x.Id == id)
            .ExecuteAffrowsAsync();
        await _richTextImageReferenceService.ReleaseAnnouncementImagesAsync(id, entity.ContentHtml);
    }

    public async Task PublishAsync(Guid id, AnnouncementPublishDto input)
    {
        var entity = await _freeSql.Select<SystemAnnouncement>()
            .Where(x => x.Id == id && !x.IsDeleted)
            .FirstAsync() ?? throw new UserFriendlyException("公告不存在");
        entity.IsPublished = input.IsPublished;
        entity.PublishTime = input.IsPublished && !entity.PublishTime.HasValue ? DateTime.Now : entity.PublishTime;
        entity.LastModifierUserId = _currentUser.Id;
        entity.LastModifierUserName = _currentUser.Surname;
        await _freeSql.Update<SystemAnnouncement>().SetSource(entity).ExecuteAffrowsAsync();
    }

    public async Task<List<AnnouncementDto>> GetUnreadLoginPopupsAsync()
    {
        if (!await _settingProvider.GetBoolAsync("announcement.loginPopupEnabled")) return [];

        var announcements = await GetVisiblePublishedAnnouncementsAsync(popupOnly: true);
        var readIds = await GetReadAnnouncementIdsAsync(announcements.Select(x => x.Id).ToList());
        return announcements.Where(x => !readIds.Contains(x.Id)).Select(x => ToDto(x, false)).ToList();
    }

    public async Task<List<AnnouncementDto>> GetCurrentUnreadAsync()
    {
        var announcements = await GetVisiblePublishedAnnouncementsAsync(popupOnly: false);
        var readIds = await GetReadAnnouncementIdsAsync(announcements.Select(x => x.Id).ToList());
        return announcements.Where(x => !readIds.Contains(x.Id)).Select(x => ToDto(x, false)).ToList();
    }

    public async Task MarkReadAsync(Guid id)
    {
        if (_currentUser.Id == Guid.Empty) throw new UserFriendlyException("请先登录");
        if (await _freeSql.Select<SystemAnnouncementReadRecord>()
            .AnyAsync(x => x.AnnouncementId == id && x.UserId == _currentUser.Id)) return;

        await _freeSql.Insert(new SystemAnnouncementReadRecord
        {
            Id = _idGenerator.Create(),
            AnnouncementId = id,
            UserId = _currentUser.Id,
            ReadTime = DateTime.Now,
            CreatorUserId = _currentUser.Id,
            CreatorUserName = _currentUser.Surname,
            LastModifierUserId = _currentUser.Id,
            LastModifierUserName = _currentUser.Surname
        }).ExecuteAffrowsAsync();
    }

    private async Task<List<SystemAnnouncement>> GetVisiblePublishedAnnouncementsAsync(bool popupOnly)
    {
        if (_currentUser.Id == Guid.Empty) return [];
        var user = await _freeSql.Select<IdentityUser>().WhereDynamic(_currentUser.Id).FirstAsync();
        if (user == null) return [];

        var roleIds = await _freeSql.Select<IdentityUserRole>()
            .Where(x => x.UserId == user.Id)
            .ToListAsync(x => x.RoleId);
        if (user.MainRoleId != Guid.Empty && !roleIds.Contains(user.MainRoleId)) roleIds.Add(user.MainRoleId);

        var now = DateTime.Now;
        var list = await _freeSql.Select<SystemAnnouncement>()
            .Where(x => !x.IsDeleted && x.IsPublished)
            .Where(x => !x.PublishTime.HasValue || x.PublishTime <= now)
            .Where(x => !x.ExpireTime.HasValue || x.ExpireTime > now)
            .WhereIf(popupOnly, x => x.PopupOnLogin)
            .OrderByDescending(x => x.Priority)
            .OrderByDescending(x => x.PublishTime)
            .ToListAsync();

        var organizationUnit = user.OrganizationUnitId == Guid.Empty
            ? null
            : await _freeSql.Select<IdentityOrganizationUnit>().WhereDynamic(user.OrganizationUnitId).FirstAsync();

        return list.Where(x => IsVisibleToUser(x, roleIds, user.OrganizationUnitId, organizationUnit?.Path)).ToList();
    }

    private async Task<HashSet<Guid>> GetReadAnnouncementIdsAsync(List<Guid> announcementIds)
    {
        if (announcementIds.Count == 0) return [];
        var ids = await _freeSql.Select<SystemAnnouncementReadRecord>()
            .Where(x => x.UserId == _currentUser.Id && announcementIds.Contains(x.AnnouncementId))
            .ToListAsync(x => x.AnnouncementId);
        return ids.ToHashSet();
    }

    private static bool IsVisibleToUser(
        SystemAnnouncement announcement,
        List<Guid> roleIds,
        Guid organizationUnitId,
        string? organizationUnitPath)
    {
        if (announcement.VisibleToAll) return true;
        var announcementRoleIds = ParseIds(announcement.RoleIdsJson);
        if (announcementRoleIds.Count > 0 && announcementRoleIds.Intersect(roleIds).Any()) return true;

        var organizationUnitIds = ParseIds(announcement.OrganizationUnitIdsJson);
        if (organizationUnitId == Guid.Empty || organizationUnitIds.Count == 0) return false;
        if (organizationUnitIds.Contains(organizationUnitId)) return true;

        return organizationUnitIds.Any(id => (organizationUnitPath ?? string.Empty).Contains($"{id},"));
    }

    private static void ValidateInput(AnnouncementCreateUpdateDto input)
    {
        if (input.Title.IsNullOrWhiteSpace()) throw new UserFriendlyException("请输入公告标题");
        if (input.ContentHtml.IsNullOrWhiteSpace()) throw new UserFriendlyException("请输入公告正文");
        if (!input.VisibleToAll && input.RoleIds.Count == 0 && input.OrganizationUnitIds.Count == 0)
        {
            throw new UserFriendlyException("非全员可见时，请至少选择一个角色或部门");
        }
    }

    private static void ApplyInput(SystemAnnouncement entity, AnnouncementCreateUpdateDto input)
    {
        entity.Title = input.Title.Trim();
        entity.Summary = input.Summary?.Trim();
        entity.ContentHtml = input.ContentHtml;
        entity.Priority = input.Priority;
        entity.IsPublished = input.IsPublished;
        entity.PopupOnLogin = input.PopupOnLogin;
        entity.VisibleToAll = input.VisibleToAll;
        entity.PublishTime = input.PublishTime ?? (input.IsPublished ? DateTime.Now : null);
        entity.ExpireTime = input.ExpireTime;
        entity.RoleIdsJson = SerializeIds(input.VisibleToAll ? [] : input.RoleIds);
        entity.OrganizationUnitIdsJson = SerializeIds(input.VisibleToAll ? [] : input.OrganizationUnitIds);
    }

    private static AnnouncementDto ToDto(SystemAnnouncement entity, bool isRead = false)
    {
        return new AnnouncementDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Summary = entity.Summary,
            ContentHtml = entity.ContentHtml,
            Priority = entity.Priority,
            IsPublished = entity.IsPublished,
            PopupOnLogin = entity.PopupOnLogin,
            VisibleToAll = entity.VisibleToAll,
            PublishTime = entity.PublishTime,
            ExpireTime = entity.ExpireTime,
            RoleIds = ParseIds(entity.RoleIdsJson),
            OrganizationUnitIds = ParseIds(entity.OrganizationUnitIdsJson),
            IsRead = isRead,
            CreationTime = entity.CreationTime,
            CreatorUserName = entity.CreatorUserName
        };
    }

    private static List<Guid> ParseIds(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string SerializeIds(List<Guid> ids)
    {
        return JsonSerializer.Serialize(ids.Distinct().ToList(), JsonOptions);
    }
}
