using ADA_Contracts.Events;
using ADA_EmailConsumer.Consumers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ADA_EmailConsumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly AssessmentCreatedConsumer _assessmentCreatedConsumer;

    public Worker(
        ILogger<Worker> logger,
        AssessmentCreatedConsumer assessmentCreatedConsumer)
    {
        _logger = logger;
        _assessmentCreatedConsumer = assessmentCreatedConsumer;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        await using var connection =
            await factory.CreateConnectionAsync(stoppingToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "assessment-created",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            var body = args.Body.ToArray();

            var message = Encoding.UTF8.GetString(body);

            var @event = JsonSerializer.Deserialize<AssessmentCreated>(
                message);

            if (@event is null)
            {
                _logger.LogError(
                    "Could not deserialize AssessmentCreated event.");

                return;
            }

            _logger.LogInformation(
                "AssessmentCreated event received for Assessment {AssessmentId}",
                @event.AssessmentId);

            await _assessmentCreatedConsumer.Handle(
                @event,
                stoppingToken);
        };

        await channel.BasicConsumeAsync(
            queue: "assessment-created",
            autoAck: true,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Email Consumer is waiting for AssessmentCreated events.");

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }
}
