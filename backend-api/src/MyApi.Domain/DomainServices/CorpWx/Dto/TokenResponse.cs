using System.Text.Json.Serialization;

namespace MyApi.Domain.DomainServices.CorpWx.Dto;

public record TokenResponse
{
    [JsonPropertyName("errcode")] public int ErrorCode { get; init; }

    [JsonPropertyName("errmsg")] public string? ErrorMessage { get; init; }

    [JsonPropertyName("access_token")] public string? AccessToken { get; init; }

    [JsonPropertyName("expires_in")] public int? ExpiresIn { get; init; }
}