namespace ModularMonolith.Shared.Messaging.MassTransit.RabbitMQ;

public class RabbitMQOptions
{
    public required string Host { get; init; }

    public required string Username { get; init; }

    public required string Password { get; init; }
}
