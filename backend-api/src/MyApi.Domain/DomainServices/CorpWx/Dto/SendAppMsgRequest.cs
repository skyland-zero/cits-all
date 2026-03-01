using System.Text.Json.Serialization;

namespace MyApi.Domain.DomainServices.CorpWx.Dto;

public class SendAppMsgRequest
{
    [JsonPropertyName("touser")] public string ToUser { get; set; }

    [JsonPropertyName("msgtype")] public string MsgType { get; set; } = "text";

    [JsonPropertyName("agentid")] public int AgentId { get; set; }

    [JsonPropertyName("text")] public TextContent Text { get; set; }

    [JsonPropertyName("safe")] public int Safe { get; set; } = 1;

    public class TextContent
    {
        [JsonPropertyName("content")] public string Content { get; set; }
    }
}