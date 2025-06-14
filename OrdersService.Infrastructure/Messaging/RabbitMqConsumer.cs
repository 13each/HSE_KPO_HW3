using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrdersService.Infrastructure.Messaging
{
    public class RabbitMqConsumer : IRabbitMqConsumer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel      _channel;

        public RabbitMqConsumer(IOptions<RabbitMqOptions> opts)
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
        }

        public void Subscribe(string exchange, string queue, Action<string, string> onMessage)
        {
            _channel.ExchangeDeclare(exchange, ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue, exchange, "");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (_, ea) =>
            {
                var type    = ea.BasicProperties.Type;
                var payload = Encoding.UTF8.GetString(ea.Body.ToArray());

                onMessage(type, payload);
                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue, autoAck: false, consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
