namespace OrdersService.Infrastructure.Messaging
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync(string exchange, string messageType, string payload);
    }
}