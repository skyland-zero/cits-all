namespace Cits;

public class UserFriendlyException : Exception
{
    /// <summary>
    ///     构造函数（仅消息）
    /// </summary>
    public UserFriendlyException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     构造函数（消息 + 错误代码）
    /// </summary>
    public UserFriendlyException(string message, string? code)
        : base(message)
    {
        Code = code;
    }

    /// <summary>
    ///     构造函数（消息 + 错误代码 + 额外数据）
    /// </summary>
    public UserFriendlyException(string message, string? code, IDictionary<string, object?>? extraData)
        : base(message)
    {
        Code = code;
        ExtraData = extraData;
    }

    /// <summary>
    ///     构造函数（消息 + 内部异常）
    /// </summary>
    public UserFriendlyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     构造函数（消息 + 错误代码 + 内部异常）
    /// </summary>
    public UserFriendlyException(string message, string? code, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    /// <summary>
    ///     错误代码（可选）
    /// </summary>
    public string? Code { get; }

    /// <summary>
    ///     额外数据（可选）
    /// </summary>
    public IDictionary<string, object?>? ExtraData { get; }
}