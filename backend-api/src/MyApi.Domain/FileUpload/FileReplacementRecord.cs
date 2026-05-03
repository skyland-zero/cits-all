using Cits.Entities;
using FreeSql.DataAnnotations;

namespace MyApi.Domain.FileUpload;

/// <summary>
/// 文件替换记录。替换时源文件保留，新文件作为独立附件保存。
/// </summary>
[Table(Name = "sys_file_replacement_records")]
[Index("idx_file_replace_source", nameof(SourceFileId), false)]
[Index("idx_file_replace_replacement", nameof(ReplacementFileId), false)]
public class FileReplacementRecord : EntityBase
{
    public Guid SourceFileId { get; set; }

    public Guid ReplacementFileId { get; set; }

    [Column(StringLength = 300)]
    public string SourceOriginalName { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string SourceRelativePath { get; set; } = string.Empty;

    public long SourceFileSize { get; set; }

    [Column(StringLength = 50)]
    public string SourceExtension { get; set; } = string.Empty;

    public int SourceAccessVersion { get; set; }

    [Column(StringLength = 300)]
    public string ReplacementOriginalName { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string ReplacementRelativePath { get; set; } = string.Empty;

    public long ReplacementFileSize { get; set; }

    [Column(StringLength = 50)]
    public string ReplacementExtension { get; set; } = string.Empty;

    public int ReplacementAccessVersion { get; set; }

    public DateTime ReplacedTime { get; set; }

    public Guid ReplacedByUserId { get; set; }

    [Column(StringLength = 100)]
    public string ReplacedByUserName { get; set; } = string.Empty;
}
