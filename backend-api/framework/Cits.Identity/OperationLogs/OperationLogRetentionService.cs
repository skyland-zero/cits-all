using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cits.OperationLogs;

public class OperationLogRetentionService : BackgroundService
{
    private readonly IFreeSql _freeSql;
    private readonly OperationLogOptions _options;
    private readonly ILogger<OperationLogRetentionService> _logger;

    public OperationLogRetentionService(
        IFreeSql freeSql,
        IOptions<OperationLogOptions> options,
        ILogger<OperationLogRetentionService> logger)
    {
        _freeSql = freeSql;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.CleanupIntervalMinutes));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await CleanupAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "操作日志清理失败，将在下个周期继续执行");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        var expiredBefore = DateTime.Now.AddDays(-_options.RetentionDays);
        var totalDeleted = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var ids = await _freeSql.Select<OperationLog>()
                .Where(x => x.OperationTime < expiredBefore)
                .OrderBy(x => x.OperationTime)
                .Take(_options.CleanupBatchSize)
                .ToListAsync(x => x.Id, cancellationToken);

            if (ids.Count == 0)
            {
                break;
            }

            var deleted = await _freeSql.Delete<OperationLog>()
                .Where(x => ids.Contains(x.Id))
                .ExecuteAffrowsAsync(cancellationToken);

            totalDeleted += deleted;

            if (ids.Count < _options.CleanupBatchSize)
            {
                break;
            }

            if (_options.CleanupBatchDelayMs > 0)
            {
                await Task.Delay(_options.CleanupBatchDelayMs, cancellationToken);
            }
        }

        if (totalDeleted > 0)
        {
            _logger.LogInformation("操作日志清理完成，删除 {DeletedCount} 条过期日志", totalDeleted);
        }
    }
}