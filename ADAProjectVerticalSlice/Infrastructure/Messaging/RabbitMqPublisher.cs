using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ADAProjectAPIVerticalSlice.Infrastructure.Messaging;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    public async Task PublishAsync<T>(
        T message,
        string queueName,
        CancellationToken cancellationToken = default)
    {
        // Opretter en connectionFactory, der indeholder oplysninger om RabbitMQ-serveren
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        // Opretter en forbindelse til RabbitMQ-serveren
        await using var connection =
            await factory.CreateConnectionAsync(cancellationToken);

        // Opretter en kanal til at kommunikere med RabbitMQ-serveren
        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        // Opretter en kø, hvis den ikke allerede findes
        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        // Serialiserer meddelelsen til JSON-format
        var json = JsonSerializer.Serialize(message);
        // Konverterer JSON-strengen til en byte-array, da RabbitMQ transporterer data som byte-arrays
        var body = Encoding.UTF8.GetBytes(json);

        // Publicerer meddelelsen til den angivne kø
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            body: body,
            cancellationToken: cancellationToken);
    }
}