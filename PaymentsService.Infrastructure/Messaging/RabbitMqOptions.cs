namespace PaymentsService.Infrastructure.Messaging
{
public class RabbitMqOptions
{
    public string HostName { get; set; } = "rabbitmq";
    public int    Port     { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}
}