using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Common.Messages.Events;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.Data;

namespace OrdersService.Infrastructure.Messaging;

public class PaymentInboxHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRabbitMqConsumer   _consumer;

    public PaymentInboxHostedService(
        IServiceScopeFactory scopeFactory,
        IRabbitMqConsumer   consumer)
    {
        _scopeFactory = scopeFactory;
        _consumer     = consumer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(
            exchange: "orders.exchange",
            queue:    "orders_inbox",
            onMessage: async (type, payload) =>
            {
                if (type == nameof(PaymentProcessed))
                {
                    var ev = JsonSerializer.Deserialize<PaymentProcessed>(payload)!;
                    await Handle(ev);
                }
            });

        return Task.CompletedTask;
    }

    private async Task Handle(PaymentProcessed ev)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db    = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        var order = await db.Orders.SingleOrDefaultAsync(o => o.Id == ev.OrderId);
        if (order == null) return;

        order.Status = ev.Success
            ? OrderStatus.Finished
            : OrderStatus.Cancelled;

        await db.SaveChangesAsync();
    }
}