using System.ComponentModel.DataAnnotations;
using Cits.Entities;
using FreeSql.DataAnnotations;

namespace Cits.Domain.SystemSettings;

/// <summary>
/// 系统参数配置。
/// </summary>
[Table(Name = "sys_system_settings")]
[Index("idx_system_setting_key", nameof(Key), true)]
[Index("idx_system_setting_group", nameof(Group), false)]
public class SystemSetting : EntityBaseWithSoftDelete
{
    /// <summary>
    /// 参数键名。
    /// </summary>
    [MaxLength(150)]
    [Column(StringLength = 150)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 参数名称。
    /// </summary>
    [MaxLength(100)]
    [Column(StringLength = 100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 参数值。
    /// </summary>
    [Column(StringLength = -1)]
    public string? Value { get; set; }

    /// <summary>
    /// 值类型：String、Number、Boolean、Json。
    /// </summary>
    [MaxLength(30)]
    [Column(StringLength = 30)]
    public string ValueType { get; set; } = SystemSettingValueTypes.String;

    /// <summary>
    /// 参数分组。
    /// </summary>
    [MaxLength(80)]
    [Column(StringLength = 80)]
    public string Group { get; set; } = SystemSettingGroups.Basic;

    /// <summary>
    /// 参数说明。
    /// </summary>
    [MaxLength(500)]
    [Column(StringLength = 500)]
    public string? Description { get; set; }

    /// <summary>
    /// 是否敏感配置。
    /// </summary>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// 是否只读。
    /// </summary>
    public bool IsReadonly { get; set; }

    /// <summary>
    /// 排序。
    /// </summary>
    public int Sort { get; set; }
}

public static class SystemSettingValueTypes
{
    public const string String = "String";
    public const string Number = "Number";
    public const string Boolean = "Boolean";
    public const string Json = "Json";

    public static readonly string[] All = [String, Number, Boolean, Json];
}

public static class SystemSettingGroups
{
    public const string Basic = "Basic";
    public const string Security = "Security";
    public const string Upload = "Upload";
    public const string Import = "Import";
    public const string Announcement = "Announcement";
}
