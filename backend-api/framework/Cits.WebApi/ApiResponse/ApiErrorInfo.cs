using System.Collections;

namespace Cits.ApiResponse;

[Serializable]
public class ApiErrorInfo
{
    public ApiErrorInfo()
    {
    }

    public ApiErrorInfo(string message, string? details = null, string? code = null, IDictionary? data = null)
    {
        Message = message;
        Details = details;
        Code = code;
        Data = data;
    }

    public string? Code { get; set; }


    public string? Message { get; set; }


    public string? Details { get; set; }


    public IDictionary? Data { get; set; }


    public ApiValidationErrorInfo[]? ValidationErrors { get; set; }
}