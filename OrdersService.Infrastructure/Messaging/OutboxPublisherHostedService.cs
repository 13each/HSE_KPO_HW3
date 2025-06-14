using Microsoft.EntityFrameworkCore;
using OrdersService.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OrdersService.Infrastructure.Messaging
{
    public class OutboxPublisherHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitMqPublisher   _publisher;
        private const string ExchangeName = "orders.exchange";

        public OutboxPublisherHostedService(
            IServiceScopeFactory scopeFactory,
            IRabbitMqPublisher   publisher)
        {
            _scopeFactory = scopeFactory;
            _publisher    = publisher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

                var messages = await db.OutboxMessages
                    .Where(m => !m.Processed)
                    .OrderBy(m => m.OccurredAt)
                    .ToListAsync(stoppingToken);

                foreach (var msg in messages)
                {
                    await _publisher.PublishAsync(
                        ExchangeName,
                        msg.Type,
                        msg.Payload);

                    msg.Processed   = true;
                    msg.ProcessedAt = DateTime.UtcNow;
                }

                await db.SaveChangesAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
