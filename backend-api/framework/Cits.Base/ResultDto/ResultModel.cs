using System.Text.Json.Serialization;

namespace Cits.ResultDto;

/// <summary>
///     返回结果
/// </summary>
public class ResultModel<T> : IResultModel<T>
{
    /// <summary>
    ///     状态码（返回0成功）
    /// </summary>
    public int Code
    {
        get
        {
            if (Successful) return 0;
            return -1;
        }
    }

    /// <summary>
    ///     错误
    /// </summary>
    public string Msg { get; private set; } = string.Empty;

    /// <summary>
    ///     处理是否成功
    /// </summary>
    [JsonIgnore]
    public bool Successful { get; private set; }

    /// <summary>
    ///     返回数据
    /// </summary>
    public T Data { get; private set; } = default!;

    /// <summary>
    ///     成功
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="msg">说明</param>
    public ResultModel<T> Success(T data, string msg = "success")
    {
        Successful = true;
        Data = data;
        Msg = msg;

        return this;
    }

    /// <summary>
    ///     失败
    /// </summary>
    /// <param name="msg">说明</param>
    public ResultModel<T> Failed(string msg = "failed")
    {
        Successful = false;
        Msg = msg;
        return this;
    }

    /// <summary>
    ///     失败
    /// </summary>
    /// <param name="msg">说明</param>
    public ResultModel<T> Failed(T data, string msg = "failed")
    {
        Successful = false;
        Msg = msg;
        Data = data;
        return this;
    }
}

/// <summary>
///     返回结果
/// </summary>
public static class ResultModel
{
    /// <summary>
    ///     数据已存在
    /// </summary>
    /// <returns></returns>
    public static IResultModel HasExists => Failed("数据已存在");

    /// <summary>
    ///     数据不存在
    /// </summary>
    public static IResultModel NotExists => Failed("数据不存在");

    /// <summary>
    ///     成功
    /// </summary>
    /// <param name="data">返回数据</param>
    /// <returns></returns>
    public static IResultModel<T> Success<T>(string mgs, T data = default!)
    {
        return new ResultModel<T>().Success(data, mgs);
    }

    /// <summary>
    ///     成功
    /// </summary>
    /// <param name="mgs"></param>
    /// <returns></returns>
    public static IResultModel Success(string mgs)
    {
        return new ResultModel<object>().Success(default!, mgs);
    }

    /// <summary>
    ///     成功
    /// </summary>
    /// <param name="data">返回数据</param>
    /// <returns></returns>
    public static IResultModel<T> Success<T>(T data = default!)
    {
        return new ResultModel<T>().Success(data);
    }

    /// <summary>
    ///     成功
    /// </summary>
    /// <returns></returns>
    public static IResultModel Success()
    {
        return Success<string>();
    }

    /// <summary>
    ///     失败
    /// </summary>
    /// <param name="error">错误信息</param>
    /// <returns></returns>
    public static IResultModel<T> Failed<T>(string? error = null)
    {
        return new ResultModel<T>().Failed(error ?? "failed");
    }

    /// <summary>
    ///     失败
    /// </summary>
    /// <param name="error">错误信息</param>
    /// <returns></returns>
    public static IResultModel<T> Failed<T>(string error, T data = default!)
    {
        return new ResultModel<T>().Failed(data, error ?? "failed");
    }

    /// <summary>
    ///     失败
    /// </summary>
    /// <returns></returns>
    public static IResultModel Failed(string? error = null)
    {
        return Failed<string>(error);
    }

    /// <summary>
    ///     根据布尔值返回结果
    /// </summary>
    /// <param name="success"></param>
    /// <returns></returns>
    public static IResultModel Result<T>(bool success)
    {
        return success ? Success<T>() : Failed<T>();
    }

    /// <summary>
    /// </summary>
    /// <param name="success"></param>
    /// <returns></returns>
    public static IResultModel Result(bool success)
    {
        return success ? Success() : Failed();
    }
}
