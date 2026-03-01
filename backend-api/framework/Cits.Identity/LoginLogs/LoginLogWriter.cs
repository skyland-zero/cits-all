using System.Threading.Channels;

namespace Cits.LoginLogs;

public class LoginLogWriter : ILoginLogWriter
{
    private readonly Channel<LoginLog> _channel;

    public LoginLogWriter()
    {
        // 创建高性能Channel（无界通道，根据需求可改为有界）
        _channel = Channel.CreateUnbounded<LoginLog>(new UnboundedChannelOptions
        {
            SingleWriter = false, // 支持多生产者
            SingleReader = true // 单消费者模式
        });
    }

    public async ValueTask WriteAsync(LoginLog entry, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(entry, cancellationToken);
    }

    public Channel<LoginLog> GetChannel() => _channel;
}