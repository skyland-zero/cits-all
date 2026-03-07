using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyCSharp.HttpUserAgentParser.Providers;
using Polly;
using Timer = System.Timers.Timer;


namespace Cits.LoginLogs;

public class LoginLogService : BackgroundService
{
    private readonly IFreeSql _freeSql;
    private readonly IHttpUserAgentParserProvider _parser;
    private readonly ILoginLogWriter _logWriter;
    private readonly ILogger<LoginLogService> _logger;

    private const int DefaultBatchSize = 100;
    private readonly int _batchSize;
    private readonly int _flushInterval;
    private readonly IAsyncPolicy _retryPolicy;
    private readonly List<LoginLog> _batch;
    private readonly Timer _timer;


    public LoginLogService(IFreeSql freeSql, ILoginLogWriter logWriter, ILogger<LoginLogService> logger,
        IHttpUserAgentParserProvider parserProvider)
    {
        _freeSql = freeSql;
        _logWriter = logWriter;
        _logger = logger;
        _parser = parserProvider;

        _batchSize = DefaultBatchSize;
        _flushInterval = 10 * 1000;
        _batch = new List<LoginLog>(_batchSize);
        _timer = new Timer(_flushInterval);
        _timer.Elapsed += async (_, _) => await HandleTimer();
        _timer.Start();

        // 重试策略
        _retryPolicy = Policy
            .Handle<Exception>() // 异常
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 指数退避
                onRetry: (exception, delay, retryCount, context) =>
                {
                    logger.LogWarning(exception,
                        "登录日志保存失败，第 {RetryCount} 次重试，延迟 {DelayTotalMilliseconds}ms", retryCount, delay
                            .TotalMilliseconds);
                });
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await foreach (var logEntry in _logWriter.GetChannel().Reader.ReadAllAsync(stoppingToken))
                {
                    if (logEntry.UserAgent != null)
                    {
                        var info = _parser.Parse(logEntry.UserAgent!);
                        logEntry.Browser = info.Name;
                        logEntry.Os = info.Platform?.PlatformType.ToString();
                        logEntry.Device = info.MobileDeviceType;
                        logEntry.BrowserInfo = info.Version;
                    }

                    _batch.Add(logEntry);
                    // 达到批次大小立即保存
                    if (_batch.Count >= _batchSize)
                    {
                        // _logger.LogInformation("队列满了保存一批");
                        _timer.Stop();
                        await FlushLogs(_batch, stoppingToken);
                        _timer.Interval = _flushInterval;
                        _timer.Start();
                    }
                }
            }
            finally
            {
                _timer.Dispose();
                // 应用关闭前强制保存剩余日志
                if (_batch.Count > 0)
                {
                    await FlushLogs(_batch, CancellationToken.None);
                }
            }
        }
    }

    private async ValueTask HandleTimer()
    {
        // _logger.LogInformation("时间到了保存一批");
        if (_batch.Count > 0)
        {
            await FlushLogs(_batch, CancellationToken.None);
        }
    }

    /// <summary>
    /// 处理所有日志
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="ct"></param>
    private async Task FlushLogs(List<LoginLog> batch, CancellationToken ct)
    {
        await SaveAsync(batch, ct);
        batch.Clear();
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="cancellationToken"></param>
    private async Task SaveAsync(List<LoginLog> batch, CancellationToken cancellationToken)
    {
        await _retryPolicy.ExecuteAsync(async ct =>
            {
                var insert = _freeSql.Insert(batch.ToList());
                if (_freeSql.Ado.DataType == FreeSql.DataType.PostgreSQL)
                {
                    await insert.ExecutePgCopyAsync(ct);
                }
                else
                {
                    await insert.ExecuteAffrowsAsync(ct);
                }
            },
            cancellationToken);
    }
}