using Cits.Entities;

namespace Cits.LoginLogs;

public class LoginLog : Entity
{
    /// <summary>
    /// 登录IP
    /// </summary>
    public string? IP { get; set; }

    /// <summary>
    /// 浏览器
    /// </summary>
    public string? Browser { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    public string? Os { get; set; }

    /// <summary>
    /// 设备
    /// </summary>
    public string? Device { get; set; }

    /// <summary>
    /// 浏览器信息
    /// </summary>
    public string? BrowserInfo { get; set; }
    

    /// <summary>
    /// 登录状态（true成功，false失败）
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// 登录地点
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 操作信息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 姓名（昵称）
    /// </summary>
    public string? RealName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime LoginTime { get; set; }
    
    public string? UserAgent { get; set; }
}