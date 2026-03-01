using Cits.Dtos;

namespace MyApi.Application.Identities.Dto;

public class OrganizationUnitTreeDto : EntityDto<Guid>
{
    /// <summary>
    ///     名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     编码
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     父级ID
    /// </summary>
    public Guid? ParentId { get; set; }

    public List<OrganizationUnitTreeDto>? Children { get; set; }
}