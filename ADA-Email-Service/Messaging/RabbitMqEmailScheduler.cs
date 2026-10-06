using ADA_Contracts;
using ADA_Contracts.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ADA_EmailConsumer.Messaging;

public class RabbitMqEmailScheduler
{
    public async Task ScheduleAsync(
        EmailScheduled email,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        await using var connection =
            await factory.CreateConnectionAsync(cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(email);

        var body = Encoding.UTF8.GetBytes(json);

        var expiration =
            Math.Max(0, (long)delay.TotalMilliseconds);

        var properties = new BasicProperties
        {
            Persistent = true,
            Expiration = expiration.ToString()
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "email-delay-queue",
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}
