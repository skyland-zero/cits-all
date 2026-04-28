using System.Diagnostics;
using Cits.LoginLogs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace Cits.OperationLogs;

public class OperationLogService : BackgroundService
{
    private readonly IFreeSql _freeSql;
    private readonly IOperationLogWriter _logWriter;
    private readonly IIpLocationService _ipLocationService;
    private readonly ILogger<OperationLogService> _logger;
    private readonly OperationLogOptions _options;
    private readonly IAsyncPolicy _retryPolicy;
    private readonly List<OperationLog> _batch;
    private readonly Lock _batchLock = new();

    public OperationLogService(
        IFreeSql freeSql,
        IOperationLogWriter logWriter,
        IIpLocationService ipLocationService,
        IOptions<OperationLogOptions> options,
        ILogger<OperationLogService> logger)
    {
        _freeSql = freeSql;
        _logWriter = logWriter;
        _ipLocationService = ipLocationService;
        _logger = logger;
        _options = options.Value;
        _batch = new List<OperationLog>(_options.BatchSize);
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, delay, retryCount, _) =>
                {
                    _logger.LogWarning(exception,
                        "操作日志保存失败，第 {RetryCount} 次重试，延迟 {DelayTotalMilliseconds}ms",
                        retryCount,
                        delay.TotalMilliseconds);
                });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_options.FlushIntervalMs));
        var flushTask = FlushLoopAsync(timer, stoppingToken);

        try
        {
            await foreach (var logEntry in _logWriter.GetChannel().Reader.ReadAllAsync(stoppingToken))
            {
                if (!string.IsNullOrWhiteSpace(logEntry.OperationIp))
                {
                    try
                    {
                        logEntry.OperationLocation = _ipLocationService.Search(logEntry.OperationIp);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "操作日志 IP 归属地解析失败: {OperationIp}", logEntry.OperationIp);
                    }
                }

                List<OperationLog>? flushBatch = null;
                lock (_batchLock)
                {
                    _batch.Add(logEntry);
                    if (_batch.Count >= _options.BatchSize)
                    {
                        flushBatch = DrainBatchUnsafe();
                    }
                }

                if (flushBatch is not null)
                {
                    await SaveBatchAsync(flushBatch, stoppingToken);
                }
            }
        }
        finally
        {
            await FlushRemainingAsync(CancellationToken.None);
            await flushTask;
        }
    }

    private async Task FlushLoopAsync(PeriodicTimer timer, CancellationToken cancellationToken)
    {
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await FlushRemainingAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task FlushRemainingAsync(CancellationToken cancellationToken)
    {
        List<OperationLog>? flushBatch = null;
        lock (_batchLock)
        {
            if (_batch.Count > 0)
            {
                flushBatch = DrainBatchUnsafe();
            }
        }

        if (flushBatch is not null)
        {
            await SaveBatchAsync(flushBatch, cancellationToken);
        }
    }

    private List<OperationLog> DrainBatchUnsafe()
    {
        var flushBatch = _batch.ToList();
        _batch.Clear();
        return flushBatch;
    }

    private async Task SaveBatchAsync(List<OperationLog> batch, CancellationToken cancellationToken)
    {
        while (true)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _retryPolicy.ExecuteAsync(async ct =>
                {
                    await _freeSql.Insert(batch).ExecuteAffrowsAsync(ct);
                }, cancellationToken);

                stopwatch.Stop();
                _logger.LogDebug("操作日志批量保存完成，数量 {Count}，耗时 {ElapsedMilliseconds}ms", batch.Count, stopwatch.ElapsedMilliseconds);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "操作日志批量保存重试耗尽，数量 {Count}，耗时 {ElapsedMilliseconds}ms，将延迟后继续重试",
                    batch.Count,
                    stopwatch.ElapsedMilliseconds);
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }
    }
}