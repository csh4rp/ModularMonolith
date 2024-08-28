using System.Diagnostics;
using Testcontainers.Kafka;


namespace ModularMonolith.Tests.Utils.Kafka;

public static class ContainerExtensions
{
    public static async Task CreateTopics(this KafkaContainer container, params string[] topics)
    {
        foreach (var topic in topics)
        {
            var result = await container.ExecAsync(new List<string>
            {
                "/bin/kafka-topics",
                "--zookeeper",
                "localhost:2181",
                "--create",
                "--topic",
                topic,
                "--partitions",
                "2",
                "--replication-factor",
                "1"
            });

            Debug.Assert(result.ExitCode == 0);
        }
    }
}
