using System;
using System.ComponentModel.DataAnnotations;
using Cits.Dtos;

namespace Cits.Dictionaries;

public class DataDictTypeDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateDataDictTypeDto
{
    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
}

public class UpdateDataDictTypeDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
}

public class QueryDataDictTypeDto : PagedRequestDto
{
    public string? Keyword { get; set; }
}
