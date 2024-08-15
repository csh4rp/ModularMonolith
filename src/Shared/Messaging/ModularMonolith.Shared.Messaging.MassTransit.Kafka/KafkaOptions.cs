namespace ModularMonolith.Shared.Messaging.MassTransit.Kafka;

public class KafkaOptions
{
    public required string Host { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}
