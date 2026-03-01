using Cits.Dtos;

namespace MyApi.Application.Identities.Dto;

public class GetOrganizationUnitInput : PagedRequestDto
{
    /// <summary>
    ///     名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     编码
    /// </summary>
    public string? Code { get; set; }
}