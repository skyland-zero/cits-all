using System.Threading.Channels;

namespace Cits.OperationLogs;

public interface IOperationLogWriter
{
    ValueTask WriteAsync(OperationLog entry, CancellationToken cancellationToken = default);

    Channel<OperationLog> GetChannel();
}