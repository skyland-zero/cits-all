using IP2Region.Net.Abstractions;
using IP2Region.Net.XDB;
using Microsoft.Extensions.Logging;

namespace Cits.LoginLogs;

public class IpLocationService : IIpLocationService
{
    private readonly ISearcher? _searcher;
    private readonly ILogger<IpLocationService> _logger;

    public IpLocationService(ILogger<IpLocationService> logger)
    {
        _logger = logger;
        try
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "Resources", "ip2region.xdb");
            if (File.Exists(dbPath))
            {
                _searcher = new Searcher(CachePolicy.Content, dbPath);
                _logger.LogInformation("IP2Region 数据库加载成功: {Path}", dbPath);
            }
            else
            {
                _logger.LogWarning("IP2Region 数据库文件不存在，登录日志将无法解析地理位置: {Path}", dbPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化 IP2Region Searcher 失败");
        }
    }

    public string? Search(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip) || _searcher == null)
        {
            return null;
        }

        try
        {
            // IP2Region 返回格式通常为：国家|区域|省份|城市|ISP
            var result = _searcher.Search(ip);
            if (string.IsNullOrWhiteSpace(result)) return null;

            // 过滤掉未知和 0
            return result.Replace("0|", "").Replace("|0", "");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("IP 解析失败 [{IP}]: {Message}", ip, ex.Message);
            return null;
        }
    }
}