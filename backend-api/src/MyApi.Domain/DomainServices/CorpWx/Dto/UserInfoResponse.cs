using System.Text.Json.Serialization;

namespace MyApi.Domain.DomainServices.CorpWx.Dto;

public record UserInfoResponse
{
    [JsonPropertyName("errcode")] public int ErrorCode { get; init; }

    [JsonPropertyName("errmsg")] public string? ErrorMessage { get; init; }

    [JsonPropertyName("userid")] public string? UserId { get; init; }

    [JsonPropertyName("user_ticket")] public string? UserTicket { get; init; }
}