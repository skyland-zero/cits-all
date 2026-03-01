using System.ComponentModel.DataAnnotations;

namespace MyApi.Domain.Models;

public class UserCacheModel
{
    public Guid Id { get; set; }

    /// <summary>
    ///     账号
    /// </summary>
    [MaxLength(20)]
    public required string UserName { get; set; }

    /// <summary>
    ///     姓名
    /// </summary>
    [MaxLength(20)]
    public required string Surname { get; set; }

    /// <summary>
    ///     组织架构Id
    /// </summary>
    public Guid OrganizationUnitId { get; set; }

    /// <summary>
    ///     是否已激活
    /// </summary>
    public bool IsActive { get; set; }
}