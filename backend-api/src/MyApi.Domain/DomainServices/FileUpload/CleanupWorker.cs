using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyApi.Domain.FileUpload;

namespace MyApi.Domain.DomainServices.FileUpload;

public class CleanupWorker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _sp;
    private readonly ILogger<CleanupWorker> _logger;

    public CleanupWorker(IServiceProvider sp, ILogger<CleanupWorker> logger, IConfiguration configuration)
    {
        _sp = sp;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _sp.CreateScope())
            {
                var fsql = scope.ServiceProvider.GetRequiredService<IFreeSql>();
                var storage = scope.ServiceProvider.GetRequiredService<IStorageProvider>();

                // 查找 24小时前未转正的文件
                var expired = await fsql.Select<FileAttachment>()
                    .Where(f => !f.IsPermanent && f.CreateTime < DateTime.Now.AddHours(-24))
                    .ToListAsync(stoppingToken);

                foreach (var file in expired)
                {
                    try
                    {
                        await storage.DeleteAsync(file.RootIdentifier, file.RelativePath);
                        await fsql.Delete<FileAttachment>(file.Id).ExecuteAffrowsAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"清理文件 {file.Id} 失败");
                    }
                }

                CleanupDanglingUploadingFiles();
            }

            await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
        }
    }

    private void CleanupDanglingUploadingFiles()
    {
        try
        {
            var paths = _configuration.GetSection("StorageConfig:Local:Paths").Get<string[]>();
            var businessFolder = _configuration["StorageConfig:BusinessSubFolder"];
            if (paths == null || paths.Length == 0 || string.IsNullOrWhiteSpace(businessFolder))
            {
                return;
            }

            var cutoff = DateTime.Now.AddHours(-24);
            foreach (var rootPath in paths)
            {
                if (string.IsNullOrWhiteSpace(rootPath))
                {
                    continue;
                }

                var businessPath = Path.Combine(rootPath, businessFolder);
                if (!Directory.Exists(businessPath))
                {
                    continue;
                }

                foreach (var file in Directory.GetFiles(businessPath, $"*{LocalStorageProvider.GetUploadingSuffix()}", SearchOption.AllDirectories))
                {
                    try
                    {
                        if (File.GetCreationTime(file) < cutoff)
                        {
                            File.Delete(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "清理临时上传文件 {FilePath} 失败", file);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "扫描临时上传文件失败");
        }
    }
}
