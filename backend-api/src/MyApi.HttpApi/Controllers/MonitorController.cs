using System.Diagnostics;
using System.Runtime.InteropServices;
using Cits;
using Cits.OperationLogs;
using FreeRedis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MyApi.Domain.Identities;

namespace MyApi.HttpApi.Controllers;

/// <summary>
/// 服务监控接口
/// </summary>
[AllowAnonymous] // 允许匿名访问监控信息，或者根据需要添加特定权限 [Authorize]
[SkipOperationLog]
public class MonitorController : BaseApiController
{
    private readonly IFreeSql _fsql;
    private readonly RedisClient _redis;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "ServerMonitorInfo";

    public MonitorController(IFreeSql fsql, RedisClient redis, IMemoryCache cache)
    {
        _fsql = fsql;
        _redis = redis;
        _cache = cache;
    }

    /// <summary>
    /// 获取服务器及服务监控信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("server-info")]
    public async Task<ServerInfoResult> GetServerInfo()
    {
        if (_cache.TryGetValue(CacheKey, out ServerInfoResult? cachedInfo) && cachedInfo != null)
        {
            return cachedInfo;
        }

        var info = new ServerInfoResult
        {
            System = GetSystemInfo(),
            Cpu = GetCpuInfo(),
            Memory = GetMemoryInfo(),
            Disk = GetDiskInfo(),
            Services = await GetServicesStatus(),
            OnlineUsers = GetOnlineUsers()
        };

        // 缓存 3 秒，防止前端高频轮询对系统造成压力
        _cache.Set(CacheKey, info, TimeSpan.FromSeconds(3));

        return info;
    }

    private long GetOnlineUsers()
    {
        try
        {
            var now = DateTime.UtcNow;
            var onlineAfter = now.AddMinutes(-5);
            return _fsql.Select<IdentityOnlineUserSession>()
                .Where(x => !x.IsRevoked && x.ExpireTime > now && x.LastActiveTime >= onlineAfter)
                .Count();
        }
        catch
        {
            return -1; // 异常时返回 -1 标识不可用
        }
    }

    private SystemInfo GetSystemInfo()
    {
        var process = Process.GetCurrentProcess();
        return new SystemInfo
        {
            Os = RuntimeInformation.OSDescription,
            Architecture = RuntimeInformation.OSArchitecture.ToString(),
            Framework = RuntimeInformation.FrameworkDescription,
            StartTime = process.StartTime,
            UpTime = DateTime.Now - process.StartTime
        };
    }

    private CpuInfo GetCpuInfo()
    {
        return new CpuInfo
        {
            Name = RuntimeInformation.ProcessArchitecture.ToString(),
            Cores = Environment.ProcessorCount,
            // 简单演示：获取当前进程的 CPU 使用大致情况（需要连续两次采样才准确，这里仅返回核数信息）
            Usage = 0 
        };
    }

    private MemoryInfo GetMemoryInfo()
    {
        var process = Process.GetCurrentProcess();
        // 获取系统总内存（Windows 示例）
        long totalMemory = 0;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                // 注意：在没有额外库的情况下获取系统总内存较为复杂，这里主要展示进程占用
                totalMemory = 0; 
            }
            catch { }
        }

        return new MemoryInfo
        {
            Total = totalMemory,
            Used = process.WorkingSet64 / 1024 / 1024, // 进程占用物理内存 MB
            FrameworkUsed = GC.GetTotalMemory(false) / 1024 / 1024 // .NET 托管堆内存 MB
        };
    }

    private List<DiskInfo> GetDiskInfo()
    {
        try
        {
            return DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d => new DiskInfo
                {
                    Name = d.Name,
                    Total = d.TotalSize / 1024 / 1024 / 1024,
                    Free = d.AvailableFreeSpace / 1024 / 1024 / 1024
                }).ToList();
        }
        catch
        {
            return new List<DiskInfo>();
        }
    }

    private async Task<List<ServiceStatus>> GetServicesStatus()
    {
        var statuses = new List<ServiceStatus>();

        // 数据库状态
        var dbWatch = Stopwatch.StartNew();
        var dbOnline = false;
        var dbDetails = new Dictionary<string, string>();
        try
        {
            dbOnline = _fsql.Ado.ExecuteConnectTest();
            if (dbOnline)
            {
                dbDetails["Type"] = _fsql.Ado.DataType.ToString();
                if (_fsql.Ado.DataType == FreeSql.DataType.PostgreSQL)
                {
                    // 版本
                    var versionObj = await _fsql.Ado.ExecuteScalarAsync("SELECT version();");
                    if (versionObj != null)
                    {
                        var versionStr = versionObj.ToString();
                        dbDetails["Version"] = versionStr!.Split(' ').Length > 1 ? versionStr.Split(' ')[1] : versionStr;
                    }

                    // 连接数
                    var connCount = await _fsql.Ado.ExecuteScalarAsync("SELECT count(*) FROM pg_stat_activity;");
                    if (connCount != null) dbDetails["Connections"] = connCount.ToString()!;

                    // 数据库大小
                    var sizeObj = await _fsql.Ado.ExecuteScalarAsync("SELECT pg_size_pretty(pg_database_size(current_database()));");
                    if (sizeObj != null) dbDetails["Size"] = sizeObj.ToString()!;

                    // 表数量
                    var tableCount = await _fsql.Ado.ExecuteScalarAsync("SELECT count(*) FROM information_schema.tables WHERE table_schema = 'public';");
                    if (tableCount != null) dbDetails["Tables"] = tableCount.ToString()!;

                    // 索引数量
                    var indexCount = await _fsql.Ado.ExecuteScalarAsync("SELECT count(*) FROM pg_indexes WHERE schemaname = 'public';");
                    if (indexCount != null) dbDetails["Indexes"] = indexCount.ToString()!;

                    // 事务统计
                    var xactStats = await _fsql.Ado.ExecuteArrayAsync("SELECT xact_commit, xact_rollback FROM pg_stat_database WHERE datname = current_database();");
                    if (xactStats != null && xactStats.Length >= 2)
                    {
                        dbDetails["Commits"] = xactStats[0]?.ToString() ?? "0";
                        dbDetails["Rollbacks"] = xactStats[1]?.ToString() ?? "0";
                    }
                }
                else if (_fsql.Ado.DataType == FreeSql.DataType.Sqlite)
                {
                    var versionObj = await _fsql.Ado.ExecuteScalarAsync("SELECT sqlite_version();");
                    if (versionObj != null) dbDetails["Version"] = versionObj.ToString()!;

                    var tableCount = await _fsql.Ado.ExecuteScalarAsync("SELECT count(*) FROM sqlite_master WHERE type='table';");
                    if (tableCount != null) dbDetails["Tables"] = tableCount.ToString()!;

                    var pageSize = await _fsql.Ado.ExecuteScalarAsync("PRAGMA page_size;");
                    var pageCount = await _fsql.Ado.ExecuteScalarAsync("PRAGMA page_count;");
                    if (pageSize != null && pageCount != null)
                    {
                        var sizeInBytes = Convert.ToInt64(pageSize) * Convert.ToInt64(pageCount);
                        dbDetails["Size"] = (sizeInBytes / 1024.0 / 1024.0).ToString("F2") + " MB";
                    }
                }
            }
        }
        catch { }
        dbWatch.Stop();
        statuses.Add(new ServiceStatus { Name = "Database", Status = dbOnline ? "Online" : "Offline", Latency = dbWatch.ElapsedMilliseconds, Details = dbDetails });

        // Redis 状态
        var redisWatch = Stopwatch.StartNew();
        var redisOnline = false;
        var redisDetails = new Dictionary<string, string>();
        try
        {
            var pong = _redis.Ping();
            redisOnline = pong == "PONG";
            if (redisOnline)
            {
                var info = _redis.Info();
                var lines = info.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                long hits = 0;
                long misses = 0;

                foreach (var line in lines)
                {
                    var parts = line.Split(':', 2);
                    if (parts.Length < 2) continue;
                    var key = parts[0].Trim();
                    var val = parts[1].Trim();

                    switch (key)
                    {
                        case "redis_version": redisDetails["Version"] = val; break;
                        case "connected_clients": redisDetails["Clients"] = val; break;
                        case "used_memory_human": redisDetails["Memory"] = val; break;
                        case "used_memory_peak_human": redisDetails["PeakMemory"] = val; break;
                        case "mem_fragmentation_ratio": redisDetails["FragRatio"] = val; break;
                        case "instantaneous_ops_per_sec": redisDetails["OPS"] = val; break;
                        case "total_commands_processed": redisDetails["TotalCmds"] = val; break;
                        case "uptime_in_days": redisDetails["Uptime(Days)"] = val; break;
                        case "role": redisDetails["Role"] = val; break;
                        case "keyspace_hits": long.TryParse(val, out hits); break;
                        case "keyspace_misses": long.TryParse(val, out misses); break;
                    }
                }

                if (hits + misses > 0)
                {
                    var hitRate = (double)hits / (hits + misses) * 100;
                    redisDetails["HitRate"] = hitRate.ToString("F1") + "%";
                }
            }
        }
        catch { }
        redisWatch.Stop();
        statuses.Add(new ServiceStatus { Name = "Redis", Status = redisOnline ? "Online" : "Offline", Latency = redisWatch.ElapsedMilliseconds, Details = redisDetails });

        return statuses;
    }
}

public record ServerInfoResult
{
    public SystemInfo System { get; init; } = null!;
    public CpuInfo Cpu { get; init; } = null!;
    public MemoryInfo Memory { get; init; } = null!;
    public List<DiskInfo> Disk { get; init; } = null!;
    public List<ServiceStatus> Services { get; init; } = null!;
    public long OnlineUsers { get; init; }
}

public record SystemInfo
{
    public string Os { get; init; } = null!;
    public string Architecture { get; init; } = null!;
    public string Framework { get; init; } = null!;
    public DateTime StartTime { get; init; }
    public TimeSpan UpTime { get; init; }
}

public record CpuInfo
{
    public string Name { get; init; } = null!;
    public int Cores { get; init; }
    public double Usage { get; init; }
}

public record MemoryInfo
{
    public long Total { get; init; }
    public long Used { get; init; }
    public long FrameworkUsed { get; init; }
}

public record DiskInfo
{
    public string Name { get; init; } = null!;
    public long Total { get; init; }
    public long Free { get; init; }
}

public record ServiceStatus
{
    public string Name { get; init; } = null!;
    public string Status { get; init; } = null!;
    public long Latency { get; init; }
    public Dictionary<string, string> Details { get; init; } = new();
}
