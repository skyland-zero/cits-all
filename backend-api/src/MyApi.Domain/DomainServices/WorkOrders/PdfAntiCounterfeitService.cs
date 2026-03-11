using System.Security.Cryptography;
using System.Text;

namespace MyApi.Domain.DomainServices.WorkOrders;

/// <summary>
/// PDF 防伪指纹服务
/// </summary>
public class PdfAntiCounterfeitService
{
    private const string SecretKey = "CITS_SECURE_KEY_2026"; // 实际应从配置中心获取

    /// <summary>
    /// 生成工单防伪指纹 (HMAC-SHA256)
    /// </summary>
    /// <param name="orderNo">工单编号</param>
    /// <param name="processorId">处理人 ID</param>
    /// <param name="completionTime">完成时间</param>
    /// <returns>Base64 编码的哈希值</returns>
    public string GenerateFingerprint(string orderNo, Guid processorId, DateTime completionTime)
    {
        var rawData = $"{orderNo}|{processorId}|{completionTime:yyyyMMddHHmmss}";
        var keyBytes = Encoding.UTF8.GetBytes(SecretKey);
        var dataBytes = Encoding.UTF8.GetBytes(rawData);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// 验证指纹真实性
    /// </summary>
    public bool Verify(string orderNo, Guid processorId, DateTime completionTime, string fingerprint)
    {
        var expected = GenerateFingerprint(orderNo, processorId, completionTime);
        return expected == fingerprint;
    }
}
