using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace PaymentsService.Infrastructure.Messaging
{
    public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel      _channel;
        private const string ExchangeName = "orders.exchange";

        public RabbitMqPublisher(IOptions<RabbitMqOptions> opts)
        {
            var cfg = opts.Value;
            var factory = new ConnectionFactory
            {
                HostName                 = cfg.HostName,
                Port                     = cfg.Port,
                UserName                 = cfg.UserName,
                Password                 = cfg.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval  = TimeSpan.FromSeconds(5)
            };

            var attempts = 0;
            while (true)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    break;
                }
                catch (Exception)
                {
                    attempts++;
                    if (attempts > 12)
                        throw;

                    Thread.Sleep(5000);
                }
            }

            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);
        }

        public Task PublishAsync(string exchange, string messageType, string payload)
        {
            var body  = Encoding.UTF8.GetBytes(payload);
            var props = _channel.CreateBasicProperties();
            props.Type       = messageType;
            props.Persistent = true;

            _channel.BasicPublish(
                exchange:        exchange,
                routingKey:      "",
                basicProperties: props,
                body:            body);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
