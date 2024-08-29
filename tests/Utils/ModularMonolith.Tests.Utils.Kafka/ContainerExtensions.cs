using System.Diagnostics;
using MassTransit;
using Testcontainers.Kafka;


namespace ModularMonolith.Tests.Utils.Kafka;

public static class ContainerExtensions
{
    public static async Task CreateTopic<T>(this KafkaContainer container) where T : class
    {
        var formattedName = DefaultEndpointNameFormatter.Instance.Message<T>();

        var result = await container.ExecAsync(new List<string>
        {
            "/bin/kafka-topics",
            "--zookeeper",
            "localhost:2181",
            "--create",
            "--topic",
            formattedName,
            "--partitions",
            "2",
            "--replication-factor",
            "1"
        });

        Debug.Assert(result.ExitCode == 0);
    }
}
