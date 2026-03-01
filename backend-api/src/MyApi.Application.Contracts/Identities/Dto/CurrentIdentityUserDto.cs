using System.Text.Json.Serialization;

namespace MyApi.Application.Identities.Dto;

public class CurrentIdentityUserDto
{
    /// <summary>
    ///     头像
    /// </summary>
    /// <value></value>
    public string Avatar { get; set; }

    /// <summary>
    ///     昵称
    /// </summary>
    /// <value></value>
    public string Surname { get; set; }

    /// <summary>
    ///     角色
    /// </summary>
    /// <value></value>
    public string[] Roles { get; set; }

    /// <summary>
    ///     用户id
    /// </summary>
    /// <value></value>
    public Guid UserId { get; set; }

    /// <summary>
    ///     用户名
    /// </summary>
    /// <value></value>
    [JsonPropertyName("username")]
    public string UserName { get; set; }

    /// <summary>
    ///     描述
    /// </summary>
    /// <value></value>
    public string Desc { get; set; }

    /// <summary>
    ///     首页路由
    /// </summary>
    /// <value></value>
    public string HomePath { get; set; }
    
    /// <summary>
    ///     是否已激活
    /// </summary>
    public bool IsActive { get; set; }
}