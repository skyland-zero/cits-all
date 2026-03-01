namespace Cits.ApiResponse;

[Serializable]
public class ApiValidationErrorInfo
{
    public string Member { get; set; } = null!;


    public string[] Messages { get; set; } = null!;
}