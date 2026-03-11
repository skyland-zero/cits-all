using Cits.Dtos;

namespace MyApi.Application.Contracts.WorkOrders.Dtos;

/// <summary>
/// 附件 DTO
/// </summary>
public class FileAttachmentDto : EntityDto<Guid>
{
    /// <summary>
    /// 原始文件名
    /// </summary>
    public string OriginalName { get; set; } = string.Empty;

    /// <summary>
    /// 存储在磁盘的文件名
    /// </summary>
    public string StorageName { get; set; } = string.Empty;

    /// <summary>
    /// 文件扩展名
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 相对路径
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// 拼接后的完整访问 URL (可选，根据 StorageProvider 决定)
    /// </summary>
    public string? Url { get; set; }
}
