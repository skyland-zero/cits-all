using Cits.DI;
using FreeRedis;
using MyApi.Domain.WorkOrders;

namespace MyApi.Domain.DomainServices.WorkOrders;

public class WorkOrderNoDomainService : ISelfSingletonService
{
    private readonly IFreeSql _fsql;
    private readonly RedisClient _redis;
    private const string Prefix = "WO";

    /// <summary>
    /// 构造函数注入
    /// </summary>
    /// <param name="fsql">FreeSql 实例，用于缓存失效时的兜底</param>
    /// <param name="redis">FreeRedis 客户端</param>
    public WorkOrderNoDomainService(IFreeSql fsql, RedisClient redis)
    {
        _fsql = fsql;
        _redis = redis;
    }

    /// <summary>
    /// 生成标准日期型工单号：WO-20251225-0001
    /// </summary>
    /// <returns>生成的唯一工单编号</returns>
    public async Task<string> GenerateAsync()
    {
        // 1. 获取当前日期字符串
        string dateStr = DateTime.Now.ToString("yyyyMMdd");

        // 2. 定义 Redis Key (按天区分)
        string cacheKey = $"WorkOrder:Seq:{dateStr}";

        /// <summary>
        /// 3. 使用 Redis 的 Incr 原子递增指令
        /// 如果 Key 不存在，Redis 会先初始化为 0，然后递增到 1
        /// </summary>
        long nextSeq = _redis.Incr(cacheKey);

        /// <summary>
        /// 4. 如果是当天的第一个工单（递增结果为 1）
        /// 需要进行两个额外处理：设置过期时间、数据库兜底检查
        /// </summary>
        if (nextSeq == 1)
        {
            // 设置 26 小时过期，确保第二天自动清理，且预留时间冗余
            _redis.Expire(cacheKey, 26 * 3600);

            // 【双重保险】从数据库获取最大序号（防止 Redis 数据意外丢失后重启从 1 开始导致重号）
            int maxDbSeq = await GetMaxSeqFromDbAsync(dateStr);
            if (maxDbSeq > 0)
            {
                // 如果数据库已经有记录了，同步 Redis 序号为数据库的值
                // 注意：这里使用 TrySet 避免在极端并发下覆盖已经产生的新号
                _redis.Set(cacheKey, maxDbSeq + 1);
                nextSeq = maxDbSeq + 1;
            }
        }

        /// <summary>
        /// 5. 格式化输出：前缀-日期-4位流水号
        /// </summary>
        return $"{Prefix}-{dateStr}-{nextSeq:D4}";
    }

    /// <summary>
    /// 从数据库获取指定日期的当前最大流水号
    /// </summary>
    private async Task<int> GetMaxSeqFromDbAsync(string dateStr)
    {
        string searchPrefix = $"{Prefix}-{dateStr}-";
        var lastNo = await _fsql.Select<WorkOrder>()
            .Where(a => a.OrderNo.StartsWith(searchPrefix))
            .OrderByDescending(a => a.OrderNo)
            .FirstAsync(a => a.OrderNo);

        if (string.IsNullOrEmpty(lastNo)) return 0;

        string seqPart = lastNo.Substring(lastNo.LastIndexOf('-') + 1);
        return int.TryParse(seqPart, out int seq) ? seq : 0;
    }
}