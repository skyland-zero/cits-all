using System.ComponentModel.DataAnnotations;
using Cits.Dtos;

namespace Cits.SystemSettings;

public class SystemSettingDto : EntityDto<Guid>
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Value { get; set; }

    public string ValueType { get; set; } = string.Empty;

    public string Group { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsEncrypted { get; set; }

    public bool IsReadonly { get; set; }

    public int Sort { get; set; }
}

public class CreateSystemSettingDto
{
    [Required]
    [MaxLength(150)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Value { get; set; }

    [Required]
    [MaxLength(30)]
    public string ValueType { get; set; } = "String";

    [Required]
    [MaxLength(80)]
    public string Group { get; set; } = "Basic";

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsEncrypted { get; set; }

    public bool IsReadonly { get; set; }

    public int Sort { get; set; }
}

public class UpdateSystemSettingDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Value { get; set; }

    [Required]
    [MaxLength(30)]
    public string ValueType { get; set; } = "String";

    [Required]
    [MaxLength(80)]
    public string Group { get; set; } = "Basic";

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsEncrypted { get; set; }

    public bool IsReadonly { get; set; }

    public int Sort { get; set; }
}

public class UpdateSystemSettingValueDto
{
    public string? Value { get; set; }
}

public class QuerySystemSettingDto : PagedRequestDto
{
    public string? Keyword { get; set; }

    public string? Group { get; set; }
}

public class SystemSettingGroupDto
{
    public string Value { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}
