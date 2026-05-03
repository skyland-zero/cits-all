using Cits;
using Cits.DI;
using Cits.Dtos;
using MyApi.Application.Announcements.Dto;

namespace MyApi.Application.Announcements;

public interface IAnnouncementAppService : IApplicationService
{
    Task<PagedResultDto<AnnouncementDto>> GetListAsync(QueryAnnouncementDto input);

    Task<AnnouncementDto> GetAsync(Guid id);

    Task CreateAsync(AnnouncementCreateUpdateDto input);

    Task UpdateAsync(Guid id, AnnouncementCreateUpdateDto input);

    Task DeleteAsync(Guid id);

    Task PublishAsync(Guid id, AnnouncementPublishDto input);

    Task<List<AnnouncementDto>> GetUnreadLoginPopupsAsync();

    Task<List<AnnouncementDto>> GetCurrentUnreadAsync();

    Task MarkReadAsync(Guid id);
}
