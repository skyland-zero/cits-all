using System.Text.Json.Serialization;

namespace MyApi.Domain.DomainServices.CorpWx.Dto;

public record SendAppMsgResponse
{
    [JsonPropertyName("errcode")] public int ErrorCode { get; init; }
    [JsonPropertyName("errmsg")] public string? ErrorMessage { get; init; }

    [JsonPropertyName("response_code")] public string? ResponseCode { get; init; }
}