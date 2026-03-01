using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyApi.Domain.DomainServices.CorpWx;

public class AccessTokenScheduledService : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30); // 轮询间隔
    private readonly ILogger<AccessTokenScheduledService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1); // 异步锁

    public AccessTokenScheduledService(ILogger<AccessTokenScheduledService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("定时任务已启动");

        // 使用 PeriodicTimer（.NET 6+ 特性）
        using var timer = new PeriodicTimer(_interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken)) await TryExecuteTaskAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("定时任务已停止");
        }
    }

    private async Task TryExecuteTaskAsync(CancellationToken ct)
    {
        // 尝试获取锁（超时时间防止死锁）
        if (!await _semaphore.WaitAsync(TimeSpan.Zero, ct))
        {
            _logger.LogWarning("上一个任务仍在执行，跳过本次触发");
            return;
        }

        try
        {
            _logger.LogInformation("任务开始执行");
            await DoWorkAsync(ct); // 执行实际任务
            _logger.LogInformation("任务执行完成");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task DoWorkAsync(CancellationToken ct)
    {
        // 模拟耗时操作（替换为实际业务逻辑）
        await Task.Delay(TimeSpan.FromSeconds(10), ct);
        _logger.LogInformation("业务逻辑已执行");
    }
}