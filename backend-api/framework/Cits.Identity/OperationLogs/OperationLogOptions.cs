using System.ComponentModel.DataAnnotations;

namespace Cits.OperationLogs;

public class OperationLogOptions
{
    [Range(1, int.MaxValue)]
    public int QueueCapacity { get; set; } = 10000;

    [Range(1, int.MaxValue)]
    public int BatchSize { get; set; } = 200;

    [Range(100, int.MaxValue)]
    public int FlushIntervalMs { get; set; } = 5000;

    [Range(128, int.MaxValue)]
    public int MaxRequestLength { get; set; } = 4096;

    [Range(128, int.MaxValue)]
    public int MaxResponseLength { get; set; } = 8192;

    [Range(128, int.MaxValue)]
    public int MaxErrorLength { get; set; } = 2048;

    [Range(1, int.MaxValue)]
    public int RetentionDays { get; set; } = 180;

    [Range(1, int.MaxValue)]
    public int CleanupBatchSize { get; set; } = 1000;

    [Range(1, int.MaxValue)]
    public int CleanupIntervalMinutes { get; set; } = 60;

    [Range(0, int.MaxValue)]
    public int CleanupBatchDelayMs { get; set; } = 100;
}