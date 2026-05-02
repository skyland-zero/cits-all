using System;
using System.ComponentModel.DataAnnotations;
using Cits.Dtos;

namespace Cits.Dictionaries;

public class DataDictItemDto : EntityDto<Guid>
{
    public Guid DictTypeId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Sort { get; set; }
    public bool IsEnabled { get; set; }
}

public class CreateDataDictItemDto
{
    [Required]
    public Guid DictTypeId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Label { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Value { get; set; } = string.Empty;
    
    public int Sort { get; set; }
    
    public bool IsEnabled { get; set; } = true;
}

public class UpdateDataDictItemDto
{
    [Required]
    [MaxLength(200)]
    public string Label { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Value { get; set; } = string.Empty;
    
    public int Sort { get; set; }
    
    public bool IsEnabled { get; set; }
}

public class QueryDataDictItemDto : PagedRequestDto
{
    public Guid? DictTypeId { get; set; }
    public string? DictCode { get; set; }
}
