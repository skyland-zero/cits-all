using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyApi.Domain.DomainServices.FileUpload;
using MyApi.Domain.Imports;
using MyApi.Domain.Shared.Imports;

namespace MyApi.Application.Imports;

public class ImportTaskBackgroundService : BackgroundService
{
    private const int IdleDelayMilliseconds = 2000;
    private readonly ILogger<ImportTaskBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ImportTaskBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ImportTaskBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var handled = await TryHandleOneAsync(stoppingToken);
                if (!handled)
                {
                    await Task.Delay(IdleDelayMilliseconds, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导入任务后台处理异常");
                await Task.Delay(IdleDelayMilliseconds, stoppingToken);
            }
        }
    }

    private async Task<bool> TryHandleOneAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var freeSql = scope.ServiceProvider.GetRequiredService<IFreeSql>();
        var storage = scope.ServiceProvider.GetRequiredService<IStorageProvider>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var providers = scope.ServiceProvider.GetServices<IImportProvider>()
            .ToDictionary(x => x.ModuleKey, StringComparer.OrdinalIgnoreCase);

        var task = await freeSql.Select<ImportTask>()
            .Where(x => x.Status == ImportTaskStatus.Pending)
            .OrderBy(x => x.CreationTime)
            .FirstAsync(cancellationToken);

        if (task is null) return false;

        if (!providers.TryGetValue(task.ModuleKey, out var provider))
        {
            await MarkFailedAsync(freeSql, task, "导入模块未注册", cancellationToken);
            return true;
        }

        task.Status = ImportTaskStatus.Processing;
        task.StartedTime = DateTime.Now;
        task.LastModificationTime = DateTime.Now;
        await freeSql.Update<ImportTask>().SetSource(task).ExecuteAffrowsAsync(cancellationToken);

        try
        {
            await using var inputStream = await storage.GetStreamAsync(task.RootIdentifier, task.RelativePath)
                                          ?? throw new FileNotFoundException("导入文件不存在");
            var result = await provider.ImportAsync(inputStream, cancellationToken);
            task.TotalCount = result.TotalCount;
            task.SuccessCount = result.SuccessCount;
            task.FailedCount = result.Errors.Count;
            task.Status = result.Errors.Count == 0
                ? ImportTaskStatus.Succeeded
                : result.SuccessCount > 0
                    ? ImportTaskStatus.PartiallySucceeded
                    : ImportTaskStatus.Failed;
            task.ErrorMessage = result.Errors.Count > 0 ? $"存在 {result.Errors.Count} 行导入失败" : null;

            if (result.Errors.Count > 0)
            {
                var bytes = XlsxImportErrorReportBuilder.Build(provider.Columns, result.Errors);
                await using var reportStream = new MemoryStream(bytes);
                var reportStorageName = $"import_error_{task.Id:N}.xlsx";
                var reportRoot = await storage.SaveAsync(
                    reportStream,
                    reportStorageName,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    null,
                    cancellationToken);
                var businessSubFolder = configuration["StorageConfig:BusinessSubFolder"] ?? "business";
                task.ErrorReportRootIdentifier = reportRoot;
                task.ErrorReportRelativePath = Path.Combine(businessSubFolder, reportStorageName).Replace("\\", "/");
                task.ErrorReportStorageName = reportStorageName;
                task.ErrorReportFileSize = bytes.LongLength;
            }

            task.FinishedTime = DateTime.Now;
            task.LastModificationTime = DateTime.Now;
            await freeSql.Update<ImportTask>().SetSource(task).ExecuteAffrowsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导入任务处理失败: {TaskId}", task.Id);
            await MarkFailedAsync(freeSql, task, ex.Message, cancellationToken);
        }

        return true;
    }

    private static async Task MarkFailedAsync(IFreeSql freeSql, ImportTask task, string errorMessage, CancellationToken cancellationToken)
    {
        task.Status = ImportTaskStatus.Failed;
        task.ErrorMessage = errorMessage.Length > 500 ? errorMessage[..500] : errorMessage;
        task.FinishedTime = DateTime.Now;
        task.LastModificationTime = DateTime.Now;
        await freeSql.Update<ImportTask>().SetSource(task).ExecuteAffrowsAsync(cancellationToken);
    }
}
