using System;
using Cits.Entities;
using FreeSql.DataAnnotations;

namespace Cits.Domain.Dictionaries;

/// <summary>
/// 数据字典明细项
/// </summary>
[Table(Name = "sys_data_dict_items")]
[Index("idx_dict_item_type_id", nameof(DictTypeId), false)]
public class DataDictItem : EntityBaseWithSoftDelete
{
    /// <summary>
    /// 字典分类Id
    /// </summary>
    public Guid DictTypeId { get; set; }

    /// <summary>
    /// 显示标签
    /// </summary>
    [Column(StringLength = 200)]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 数据值
    /// </summary>
    [Column(StringLength = 200)]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; } = 0;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 导航属性
    /// </summary>
    [Navigate(nameof(DictTypeId))]
    public virtual DataDictType DictType { get; set; } = default!;
}
