using Cits.Entities;

namespace MyApi.Domain.FileUpload;

/// <summary>
/// 附件信息持久化实体
/// </summary>
public class FileAttachment : EntityBase
{
    /// <summary>
    /// 原始文件名
    /// </summary>
    public string OriginalName { get; set; }

    /// <summary>
    /// 文件哈希值（用于秒传和断点续传标识）
    /// </summary>
    public string FileHash { get; set; }

    /// <summary>
    /// 存储在磁盘的文件名（Guid格式）
    /// </summary>
    public string StorageName { get; set; }

    /// <summary>
    /// 文件扩展名
    /// </summary>
    public string Extension { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 文件内容类型（MIME类型）
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// 文件相对路径
    /// </summary>
    public string RelativePath { get; set; }

    /// <summary>
    /// 根标识（本地路径或MinIO桶名）
    /// </summary>
    public string RootIdentifier { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 是否正式转正（已关联业务数据）
    /// </summary>
    public bool IsPermanent { get; set; }

    /// <summary>
    /// 文件访问签名版本号，用于撤销历史签名链接。
    /// </summary>
    public int AccessVersion { get; set; } = 1;
}
