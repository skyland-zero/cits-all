using Cits.Dtos;

namespace MyApi.Application.Exports.Dto;

public class GetExportTasksInput : PagedRequestDto
{
    public string? ModuleKey { get; set; }
}
