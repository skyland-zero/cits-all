using System.Text.RegularExpressions;
using Cits.DI;
using MyApi.Domain.Announcements;
using MyApi.Domain.FileUpload;

namespace MyApi.Application.FileManagement;

public class RichTextImageReferenceService : ISelfScopedService
{
    private static readonly Regex FileIdRegex = new(
        "data-file-id=[\"'](?<id>[0-9a-fA-F-]{36})[\"']",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private readonly IFreeSql _freeSql;

    public RichTextImageReferenceService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public async Task<List<Guid>> MarkReferencedImagesAsPermanentAsync(
        string? html,
        CancellationToken cancellationToken = default)
    {
        var fileIds = ExtractFileIds(html);
        if (fileIds.Count == 0) return [];

        await _freeSql.Update<FileAttachment>()
            .Set(x => x.IsPermanent, true)
            .Where(x => fileIds.Contains(x.Id))
            .ExecuteAffrowsAsync(cancellationToken);

        return fileIds;
    }

    public async Task SyncAnnouncementImagesAsync(
        Guid announcementId,
        string? oldHtml,
        string? newHtml,
        CancellationToken cancellationToken = default)
    {
        var newIds = await MarkReferencedImagesAsPermanentAsync(newHtml, cancellationToken);
        var removedIds = ExtractFileIds(oldHtml).Except(newIds).ToList();
        if (removedIds.Count == 0) return;

        var otherAnnouncementHtmlList = await _freeSql.Select<SystemAnnouncement>()
            .Where(x => !x.IsDeleted && x.Id != announcementId)
            .ToListAsync(x => x.ContentHtml, cancellationToken);

        var stillUsedIds = otherAnnouncementHtmlList
            .SelectMany(ExtractFileIds)
            .ToHashSet();

        var releasableIds = removedIds.Where(id => !stillUsedIds.Contains(id)).ToList();
        if (releasableIds.Count == 0) return;

        await _freeSql.Update<FileAttachment>()
            .Set(x => x.IsPermanent, false)
            .Where(x => releasableIds.Contains(x.Id))
            .ExecuteAffrowsAsync(cancellationToken);
    }

    public Task ReleaseAnnouncementImagesAsync(
        Guid announcementId,
        string? html,
        CancellationToken cancellationToken = default)
    {
        return SyncAnnouncementImagesAsync(announcementId, html, null, cancellationToken);
    }

    public static List<Guid> ExtractFileIds(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return [];

        return FileIdRegex.Matches(html)
            .Select(match => match.Groups["id"].Value)
            .Where(value => Guid.TryParse(value, out _))
            .Select(Guid.Parse)
            .Distinct()
            .ToList();
    }
}
