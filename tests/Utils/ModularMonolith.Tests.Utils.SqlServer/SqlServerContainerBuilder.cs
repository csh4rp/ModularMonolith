using DotNet.Testcontainers.Builders;
using Testcontainers.MsSql;

namespace ModularMonolith.Tests.Utils.SqlServer;

public class SqlServerContainerBuilder
{
    public MsSqlContainer Build()
    {
        return new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithWaitStrategy(Wait.ForUnixContainer().AddCustomWaitStrategy(new SqlServerReadinessCheck()))
            .Build();
    }
}
