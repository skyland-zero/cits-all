using System.Security.Cryptography;
using System.Text;

namespace Cits.Extensions;

/// <summary>
///     String扩展
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///     判断字符串是否为Null、空
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool IsNull(this string s)
    {
        return string.IsNullOrWhiteSpace(s);
    }

    /// <summary>
    ///     判断字符串是否不为Null、空
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool NotNull(this string s)
    {
        return !string.IsNullOrWhiteSpace(s);
    }

    public static bool IsNullOrEmpty(this string? str)
    {
        return string.IsNullOrEmpty(str);
    }
    
    public static bool IsNullOrWhiteSpace(this string? str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    /// <summary>
    ///     与字符串进行比较，忽略大小写
    /// </summary>
    /// <param name="s"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool EqualsIgnoreCase(this string s, string value)
    {
        return s.Equals(value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     首字母转小写
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string FirstCharToLower(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        var str = s.First().ToString().ToLower() + s.Substring(1);
        return str;
    }

    /// <summary>
    ///     首字母转大写
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string FirstCharToUpper(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        var str = s.First().ToString().ToUpper() + s.Substring(1);
        return str;
    }

    /// <summary>
    ///     转为Base64，UTF-8格式
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string ToBase64(this string s)
    {
        return s.ToBase64(Encoding.UTF8);
    }

    /// <summary>
    ///     转为Base64
    /// </summary>
    /// <param name="s"></param>
    /// <param name="encoding">编码</param>
    /// <returns></returns>
    public static string ToBase64(this string s, Encoding encoding)
    {
        if (s.IsNull())
            return string.Empty;

        var bytes = encoding.GetBytes(s);
        return bytes.ToBase64();
    }

    /// <summary>
    ///     转为Int32
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int StringToInt32(this string str)
    {
        var num = -1;
        if (int.TryParse(str, out num)) return num;

        return -1;
    }

    /// <summary>
    ///     将以英文逗号","作为分隔的int类型字符串转换为int数组
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int[] StringToArry(this string str)
    {
        return Array.ConvertAll(str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
            s => s.StringToInt32());
    }

    /// <summary>
    ///     将以英文逗号","作为分隔的int类型字符串转换为int数组
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int[] ToIntArray(this string str)
    {
        return str.ToIntArray(new[] { ',' });
    }

    /// <summary>
    ///     将以separat参数作为分隔的int类型字符串转换为int数组
    /// </summary>
    /// <param name="str"></param>
    /// <param name="separate"></param>
    /// <returns></returns>
    public static int[] ToIntArray(this string str, char[] separate)
    {
        return Array.ConvertAll(str.Split(separate, StringSplitOptions.RemoveEmptyEntries), s => s.StringToInt32());
    }

    /// <summary>
    ///     将以英文逗号","作为分隔的string类型字符串转换为string数组
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string[] ToStringArray(this string str)
    {
        return str.ToStringArray(new[] { ',' });
    }

    /// <summary>
    ///     将以separat参数作为分隔的string类型字符串转换为string数组
    /// </summary>
    /// <param name="str"></param>
    /// <param name="separate"></param>
    /// <returns></returns>
    public static string[] ToStringArray(this string str, char[] separate)
    {
        return str.Split(separate, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string ToSha256(this string str)
    {
        // 方法 1：使用 SHA256.HashData (推荐，.NET 5+)
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(str));
        var hashString = Convert.ToHexString(hashBytes); // 输出 16 进制字符串
        return hashString;
    }
}