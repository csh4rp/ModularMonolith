using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace ModularMonolith.Tests.Utils.SqlServer;

public class SqlServerReadinessCheck : IWaitUntil
{
    private readonly string[] _command = ["/opt/mssql-tools18/bin/sqlcmd", "-Q", "SELECT 1;", "-C"];

    public async Task<bool> UntilAsync(IContainer container)
    {
        var execResult = await container.ExecAsync(_command)
            .ConfigureAwait(false);

        return 0L.Equals(execResult.ExitCode);
    }
}
