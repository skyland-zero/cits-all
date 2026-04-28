namespace Cits.LoginLogs;

public interface IIpLocationService
{
    /// <summary>
    /// 根据 IP 地址获取地理位置
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    string? Search(string? ip);
}