using MyApi.Domain.FileUpload;

namespace MyApi.Domain.DomainServices.FileUpload;

public interface IStorageProvider
{
    /// <summary>
    /// 保存完整文件（普通上传）
    /// </summary>
    /// <returns>返回存储的根标识（本地为磁盘根路径，MinIO 为 Bucket 名）</returns>
    Task<string> SaveAsync(Stream stream, string fileName, string contentType, Action<int> onProgress);

    /// <summary>
    /// 保存单个分片（断点续传）
    /// </summary>
    Task SaveChunkAsync(Stream chunkStream, string fileHash, int chunkIndex);

    // 合并分片 (返回: rootIdentifier, relativePath)
    Task<(string Root, string Path)> MergeChunksAsync(string fileHash, string fileName);

    /// <summary>
    /// 获取文件流（用于 API 下载或预览）
    /// </summary>
    Task<Stream> GetStreamAsync(string root, string relativePath);

    /// <summary>
    /// 删除文件
    /// </summary>
    Task DeleteAsync(string root, string relativePath);
}