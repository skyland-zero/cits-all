using Cits.Dtos;

namespace MyApi.Application.Identities.Dto;

public class GetLoginLogsInput : PagedRequestDto
{
    public string? UserName { get; set; }
    public string? IP { get; set; }
    public bool? Status { get; set; }
}