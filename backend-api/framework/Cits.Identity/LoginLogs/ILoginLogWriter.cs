using System.Threading.Channels;

namespace Cits.LoginLogs;

public interface ILoginLogWriter
{
    ValueTask WriteAsync(LoginLog entry, CancellationToken cancellationToken = default);

    Channel<LoginLog> GetChannel();
}