using System.ComponentModel.DataAnnotations;
using Cits.Dtos;

namespace MyApi.Application.Identities.Dto;

public class GetUsersInput : PagedRequestDto
{
    /// <summary>
    ///     账号
    /// </summary>
    [MaxLength(20)]
    public string? UserName { get; set; }

    /// <summary>
    ///     姓名
    /// </summary>
    [MaxLength(20)]
    public string? Surname { get; set; }

    /// <summary>
    ///     组织架构Id
    /// </summary>
    public Guid? OrganizationUnitId { get; set; }
}