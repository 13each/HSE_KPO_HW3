using System.Text.Json;
using Common.Messages.Commands;
using Common.Messages.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentsService.Domain.Entities;
using PaymentsService.Infrastructure.Data;

namespace PaymentsService.Infrastructure.Messaging
{
    public class InboxProcessorHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitMqConsumer   _consumer;

        public InboxProcessorHostedService(IServiceScopeFactory scopeFactory, IRabbitMqConsumer consumer)
        {
            _scopeFactory = scopeFactory;
            _consumer     = consumer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(
                exchange: "orders.exchange",
                queue:    "payments_inbox",
                onMessage: async (type, payload) =>
                {
                    if (type == nameof(OrderCreated))
                    {
                        var evt = JsonSerializer.Deserialize<OrderCreated>(payload)!;

                        var cmd = new ProcessOrderPayment(
                            evt.UserId,
                            evt.OrderId,
                            evt.Amount
                        );

                        await Handle(cmd);
                    }
                });

            return Task.CompletedTask;
        }

        private async Task Handle(ProcessOrderPayment cmd)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db   = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

            var acct    = await db.Accounts.SingleOrDefaultAsync(a => a.UserId == cmd.UserId);
            var success = acct != null && acct.Balance >= cmd.Amount;
            if (success)
            {
                acct.Balance -= cmd.Amount;
            }

            db.OutboxMessages.Add(new OutboxMessage
            {
                Id        = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                Type      = nameof(PaymentProcessed),
                Payload   = JsonSerializer.Serialize(
                    new PaymentProcessed(
                        cmd.OrderId,
                        paymentId: Guid.NewGuid(),
                        amount: cmd.Amount,
                        success: success
                    )
                ),
                Processed = false
            });


            await db.SaveChangesAsync();
        }
    }
}
