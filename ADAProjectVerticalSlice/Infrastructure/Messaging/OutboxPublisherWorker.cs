
using System.Text.Json;
using ADA_Contracts.Events;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Infrastructure.Messaging;

public class OutboxPublisherWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherWorker> _logger;

    public OutboxPublisherWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPublisherWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingMessages(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing outbox messages");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }

    private async Task PublishPendingMessages(
        CancellationToken cancellationToken)
    {
        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var publisher = scope.ServiceProvider
            .GetRequiredService<IRabbitMqPublisher>();

        var messages = await dbContext.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.CreatedOnUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                if (message.Type != nameof(AssessmentCreated))
                {
                    throw new InvalidOperationException(
                        $"Unknown event type: {message.Type}");
                }

                var @event =
                    JsonSerializer.Deserialize<AssessmentCreated>(
                        message.Payload
                    ) ?? throw new InvalidOperationException(
                        "Could not deserialize outbox event");

                await publisher.PublishAsync(
                    @event,
                    message.RoutingKey,
                    cancellationToken);

                message.ProcessedOnUtc = DateTime.UtcNow;
                message.LastError = null;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.LastError = ex.Message;

                _logger.LogError(
                    ex,
                    "Failed to publish outbox message {MessageId}",
                    message.Id);
            }

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
    }
}
