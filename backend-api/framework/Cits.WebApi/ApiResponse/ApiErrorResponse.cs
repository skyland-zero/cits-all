namespace Cits.ApiResponse;

[Serializable]
public class ApiErrorResponse
{
    public ApiErrorResponse(ApiErrorInfo error)
    {
        Error = error;
    }

    public ApiErrorInfo Error { get; set; }
}