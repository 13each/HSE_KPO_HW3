namespace PaymentsService.Infrastructure.Messaging;

public interface IRabbitMqPublisher
{
    Task PublishAsync(string exchange, string messageType, string payload);
}
