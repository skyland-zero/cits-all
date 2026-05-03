using Cits;
using Cits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Announcements;
using MyApi.Application.Announcements.Dto;

namespace MyApi.HttpApi.Controllers;

[Authorize]
[Route("api/system/announcements")]
public class SystemAnnouncementsController : BaseApiController
{
    private readonly IAnnouncementAppService _announcementAppService;

    public SystemAnnouncementsController(IAnnouncementAppService announcementAppService)
    {
        _announcementAppService = announcementAppService;
    }

    [HttpGet]
    public Task<PagedResultDto<AnnouncementDto>> GetListAsync([FromQuery] QueryAnnouncementDto input)
    {
        return _announcementAppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public Task<AnnouncementDto> GetAsync(Guid id)
    {
        return _announcementAppService.GetAsync(id);
    }

    [HttpPost]
    public Task CreateAsync(AnnouncementCreateUpdateDto input)
    {
        return _announcementAppService.CreateAsync(input);
    }

    [HttpPost("{id}/update")]
    public Task UpdateAsync(Guid id, AnnouncementCreateUpdateDto input)
    {
        return _announcementAppService.UpdateAsync(id, input);
    }

    [HttpPost("{id}/delete")]
    public Task DeleteAsync(Guid id)
    {
        return _announcementAppService.DeleteAsync(id);
    }

    [HttpPost("{id}/publish")]
    public Task PublishAsync(Guid id, AnnouncementPublishDto input)
    {
        return _announcementAppService.PublishAsync(id, input);
    }

    [HttpGet("current/unread")]
    public Task<List<AnnouncementDto>> GetCurrentUnreadAsync()
    {
        return _announcementAppService.GetCurrentUnreadAsync();
    }

    [HttpGet("current/login-popups")]
    public Task<List<AnnouncementDto>> GetUnreadLoginPopupsAsync()
    {
        return _announcementAppService.GetUnreadLoginPopupsAsync();
    }

    [HttpPost("{id}/read")]
    public Task MarkReadAsync(Guid id)
    {
        return _announcementAppService.MarkReadAsync(id);
    }
}
