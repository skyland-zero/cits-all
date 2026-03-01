using Microsoft.AspNetCore.SignalR;

namespace MyApi.HttpApi.Hubs;

public class UploadHub: Hub
{
    // 客户端连接时可以调用，用于获取 ConnectionId
    public string GetConnectionId() => Context.ConnectionId;
}