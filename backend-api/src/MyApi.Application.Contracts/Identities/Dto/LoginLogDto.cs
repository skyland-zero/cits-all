using Cits.Dtos;

namespace MyApi.Application.Identities.Dto;

public class LoginLogDto : EntityDto<Guid>
{
    public string? IP { get; set; }
    public string? Browser { get; set; }
    public string? Os { get; set; }
    public string? Device { get; set; }
    public string? BrowserInfo { get; set; }
    public bool Status { get; set; }
    public string? Location { get; set; }
    public string? Message { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? RealName { get; set; }
    public DateTime LoginTime { get; set; }
    public string? UserAgent { get; set; }
}