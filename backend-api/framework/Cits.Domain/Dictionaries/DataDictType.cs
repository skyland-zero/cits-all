using System;
using System.ComponentModel.DataAnnotations;
using Cits.Entities;
using FreeSql.DataAnnotations;

namespace Cits.Domain.Dictionaries;

/// <summary>
/// 数据字典分类
/// </summary>
[Table(Name = "sys_data_dict_types")]
[Index("idx_dict_type_code", nameof(Code), true)]
public class DataDictType : EntityBaseWithSoftDelete
{
    /// <summary>
    /// 字典编码 (唯一)
    /// </summary>
    [Column(StringLength = 100)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 字典名称
    /// </summary>
    [Column(StringLength = 100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [Column(StringLength = 500)]
    public string? Description { get; set; }
}
