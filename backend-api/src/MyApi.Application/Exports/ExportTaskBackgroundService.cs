using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyApi.Domain.DomainServices.FileUpload;
using MyApi.Domain.Exports;
using MyApi.Domain.Shared.Exports;

namespace MyApi.Application.Exports;

public class ExportTaskBackgroundService : BackgroundService
{
    private const int IdleDelayMilliseconds = 2000;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExportTaskBackgroundService> _logger;

    public ExportTaskBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExportTaskBackgroundService> logger)
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
                _logger.LogError(ex, "导出任务后台处理异常");
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
        var providers = scope.ServiceProvider.GetServices<IExportProvider>()
            .ToDictionary(x => x.ModuleKey, StringComparer.OrdinalIgnoreCase);

        var task = await freeSql.Select<ExportTask>()
            .Where(x => x.Status == ExportTaskStatus.Pending)
            .OrderBy(x => x.CreationTime)
            .FirstAsync(cancellationToken);

        if (task is null)
        {
            return false;
        }

        if (!providers.TryGetValue(task.ModuleKey, out var provider))
        {
            await MarkFailedAsync(freeSql, task, "导出模块未注册", cancellationToken);
            return true;
        }

        task.Status = ExportTaskStatus.Processing;
        task.StartedTime = DateTime.Now;
        task.LastModificationTime = DateTime.Now;
        await freeSql.Update<ExportTask>().SetSource(task).ExecuteAffrowsAsync(cancellationToken);

        try
        {
            var selectedFields = JsonSerializer.Deserialize<List<string>>(task.FieldsJson) ?? [];
            var rows = await provider.GetRowsAsync(task.QueryJson, cancellationToken);
            var bytes = CsvExportFileBuilder.Build(provider.Fields, selectedFields, rows);
            await using var stream = new MemoryStream(bytes);
            var storageName = $"{task.Id:N}.csv";
            var root = await storage.SaveAsync(
                stream,
                storageName,
                "text/csv",
                null,
                cancellationToken);

            var businessSubFolder = configuration["StorageConfig:BusinessSubFolder"] ?? "business";
            task.Status = ExportTaskStatus.Succeeded;
            task.TotalCount = rows.Count;
            task.RootIdentifier = root;
            task.RelativePath = Path.Combine(businessSubFolder, storageName).Replace("\\", "/");
            task.StorageName = storageName;
            task.ContentType = "text/csv; charset=utf-8";
            task.FileSize = bytes.LongLength;
            task.FinishedTime = DateTime.Now;
            task.LastModificationTime = DateTime.Now;
            task.ErrorMessage = null;
            await freeSql.Update<ExportTask>().SetSource(task).ExecuteAffrowsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出任务处理失败: {TaskId}", task.Id);
            await MarkFailedAsync(freeSql, task, ex.Message, cancellationToken);
        }

        return true;
    }

    private static async Task MarkFailedAsync(
        IFreeSql freeSql,
        ExportTask task,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        task.Status = ExportTaskStatus.Failed;
        task.ErrorMessage = errorMessage.Length > 500 ? errorMessage[..500] : errorMessage;
        task.FinishedTime = DateTime.Now;
        task.LastModificationTime = DateTime.Now;
        await freeSql.Update<ExportTask>().SetSource(task).ExecuteAffrowsAsync(cancellationToken);
    }
}
