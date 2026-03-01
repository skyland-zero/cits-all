using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyApi.Domain.FileUpload;

namespace MyApi.Domain.DomainServices.FileUpload;

public class CleanupWorker : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<CleanupWorker> _logger;

    public CleanupWorker(IServiceProvider sp, ILogger<CleanupWorker> logger)
    {
        _sp = sp;
        _logger = logger;
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
            }

            await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
        }
    }
}