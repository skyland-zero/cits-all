using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MyApi.Domain.DomainServices.FileUpload;

public class LocalStorageProvider : IStorageProvider
{
    private readonly IConfiguration _config;
    private readonly string _tempSub;
    private readonly string _bizSub;

    public LocalStorageProvider(IConfiguration config)
    {
        _config = config;
        _tempSub = _config["StorageConfig:TempSubFolder"];
        _bizSub = _config["StorageConfig:BusinessSubFolder"];
    }

    private string GetAvailableRoot()
    {
        var paths = _config.GetSection("StorageConfig:Local:Paths").Get<string[]>();
        var minFree = _config.GetValue<long>("StorageConfig:Local:MinFreeGB") * 1024 * 1024 * 1024;

        foreach (var path in paths)
        {
            try
            {
                // 自动识别 Linux/Windows 挂载点剩余空间
                var drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(path)));
                if (drive.IsReady && drive.AvailableFreeSpace > minFree) return path;
            }
            catch
            {
                continue;
            }
        }

        throw new Exception("没有可用的磁盘存储空间");
    }

    public async Task<string> SaveAsync(Stream stream, string fileName, string contentType, Action<int> onProgress)
    {
        var root = GetAvailableRoot();
        var relPath = Path.Combine(_bizSub, fileName);
        var fullPath = Path.Combine(root, relPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? string.Empty);

        // 手动 buffer 实现进度
        byte[] buffer = new byte[81920];
        long total = stream.Length, written = 0;
        using var fs = new FileStream(fullPath, FileMode.Create);
        int read, lastPct = 0;

        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fs.WriteAsync(buffer, 0, read);
            written += read;
            int pct = (int)((float)written / total * 100);
            if (pct > lastPct)
            {
                lastPct = pct;
                onProgress?.Invoke(pct);
            }
        }

        return root;
    }

    public async Task SaveChunkAsync(Stream chunkStream, string fileHash, int chunkIndex)
    {
        var root = GetAvailableRoot();
        var path = Path.Combine(root, _tempSub, fileHash, chunkIndex.ToString());
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using var fs = new FileStream(path, FileMode.Create);
        await chunkStream.CopyToAsync(fs);
    }

    public async Task<(string Root, string Path)> MergeChunksAsync(string fileHash, string fileName)
    {
        var root = GetAvailableRoot();
        var chunkDir = Path.Combine(root, _tempSub, fileHash);
        if (!Directory.Exists(chunkDir)) throw new FileNotFoundException("分片不存在");

        var ext = Path.GetExtension(fileName);
        var saveName = $"{Guid.NewGuid()}{ext}";
        var relPath = Path.Combine(_bizSub, saveName);
        var finalPath = Path.Combine(root, relPath);
        Directory.CreateDirectory(Path.GetDirectoryName(finalPath));

        // 排序合并
        var files = Directory.GetFiles(chunkDir).OrderBy(f => int.Parse(Path.GetFileName(f)));
        using (var dest = new FileStream(finalPath, FileMode.Create))
        {
            foreach (var f in files)
            {
                using var src = new FileStream(f, FileMode.Open);
                await src.CopyToAsync(dest);
            }
        }

        try
        {
            Directory.Delete(chunkDir, true);
        }
        catch
        {
        }

        return (root, relPath);
    }

    public Task<Stream> GetStreamAsync(string root, string relativePath)
    {
        var path = Path.Combine(root, relativePath);
        return Task.FromResult<Stream>(File.Exists(path) ? new FileStream(path, FileMode.Open, FileAccess.Read) : null);
    }

    public Task DeleteAsync(string root, string relativePath)
    {
        var path = Path.Combine(root, relativePath);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }
}