namespace ADAProjectAPIVerticalSlice.Infrastructure.Messaging
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync<T>(
            T message,
            string queueName,
            CancellationToken cancellationToken);
    }
}
