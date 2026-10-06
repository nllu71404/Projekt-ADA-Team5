using ADA_Contracts;
using ADA_Contracts.Enums;
using ADA_Contracts.Events;
using ADA_Contracts.Other_contracts;
using ADA_EmailConsumer.Consumers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(
        ILogger<Worker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
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

        // Channel til AssessmentCreated
        await using var assessmentChannel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        // Channel til EmailScheduled
        await using var emailChannel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        await ConfigureEmailQueues(
            emailChannel,
            stoppingToken);

        await PublishTestEmail(
            emailChannel,
            stoppingToken);

        await assessmentChannel.QueueDeclareAsync(
            queue: "assessment-created",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // -----------------------------------------
        // Consumer 1: AssessmentCreated
        // -----------------------------------------

        var assessmentConsumer =
            new AsyncEventingBasicConsumer(assessmentChannel);

        assessmentConsumer.ReceivedAsync += async (sender, args) =>
        {
            var body = args.Body.ToArray();

            var message = Encoding.UTF8.GetString(body);

            var @event =
                JsonSerializer.Deserialize<AssessmentCreated>(message);

            if (@event is null)
            {
                _logger.LogError(
                    "Could not deserialize AssessmentCreated event.");

                return;
            }

            _logger.LogInformation(
                "AssessmentCreated event received for Assessment {AssessmentId}",
                @event.AssessmentId);

            using var scope =
                _scopeFactory.CreateScope();

            var assessmentCreatedConsumer =
                scope.ServiceProvider
                    .GetRequiredService<AssessmentCreatedConsumer>();

            await assessmentCreatedConsumer.Handle(
                @event,
                stoppingToken);
        };

        await assessmentChannel.BasicConsumeAsync(
            queue: "assessment-created",
            autoAck: true,
            consumer: assessmentConsumer,
            cancellationToken: stoppingToken);

        // -----------------------------------------
        // Consumer 2: EmailScheduled
        // -----------------------------------------

        var emailConsumer =
            new AsyncEventingBasicConsumer(emailChannel);

        emailConsumer.ReceivedAsync += async (sender, args) =>
        {
            var body = args.Body.ToArray();

            var message = Encoding.UTF8.GetString(body);


            var @event =
                JsonSerializer.Deserialize<EmailScheduled>(message);

            if (@event is null)
            {
                _logger.LogError(
                    "Could not deserialize EmailScheduled event.");

                return;
            }

            _logger.LogInformation(
                "EmailScheduled event received for Assessment {AssessmentId}. Type: {EmailType}",
                @event.AssessmentId,
                @event.EmailType);

            using var scope =
                _scopeFactory.CreateScope();

            var emailScheduledConsumer =
                scope.ServiceProvider
                    .GetRequiredService<EmailScheduledConsumer>();

            await emailScheduledConsumer.Handle(
                @event,
                stoppingToken);
        };

        await emailChannel.BasicConsumeAsync(
            queue: "email-queue",
            autoAck: true,
            consumer: emailConsumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Email Consumer is waiting for AssessmentCreated and EmailScheduled events.");

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }

    private static async Task ConfigureEmailQueues(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        // Dead Letter Exchange
        await channel.ExchangeDeclareAsync(
            exchange: "email-exchange",
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        // Final email queue
        await channel.QueueDeclareAsync(
            queue: "email-queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        // Bind email-queue to email-exchange
        await channel.QueueBindAsync(
            queue: "email-queue",
            exchange: "email-exchange",
            routingKey: "email",
            cancellationToken: cancellationToken);

        // Delay queue
        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = "email-exchange",
            ["x-dead-letter-routing-key"] = "email"
        };

        await channel.QueueDeclareAsync(
            queue: "email-delay-queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments,
            cancellationToken: cancellationToken);
    }
    private static async Task PublishTestEmail(
    IChannel channel,
    CancellationToken cancellationToken)
    {
        var testEvent = new EmailScheduled(
            AssessmentId: Guid.NewGuid(),
            EmailType: EmailType.HeadsUp,
            ApplicationName: "Test Application",
            StartDate: DateTime.UtcNow.AddDays(7),
            EndDate: DateTime.UtcNow.AddDays(14),
            SentDate: DateTime.UtcNow,
            Respondents:
            [
                new RespondentEmailContract(
                RespondentId: Guid.NewGuid(),
                EmailAddress: "test@example.com",
                AccessToken: "test-access-token")
            ]);

        var json = JsonSerializer.Serialize(testEvent);

        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true,
            Expiration = "5000"
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