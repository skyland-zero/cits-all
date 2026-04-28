using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Cits.OperationLogs;

public class OperationLogWriter : IOperationLogWriter
{
    private readonly Channel<OperationLog> _channel;

    public OperationLogWriter(IOptions<OperationLogOptions> options)
    {
        _channel = Channel.CreateBounded<OperationLog>(new BoundedChannelOptions(options.Value.QueueCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    public ValueTask WriteAsync(OperationLog entry, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(entry, cancellationToken);
    }

    public Channel<OperationLog> GetChannel() => _channel;
}