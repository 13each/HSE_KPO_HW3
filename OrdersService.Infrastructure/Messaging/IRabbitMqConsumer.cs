namespace OrdersService.Infrastructure.Messaging;

public interface IRabbitMqConsumer
{
    void Subscribe(string exchange, string queue, Action<string, string> onMessage);
}
