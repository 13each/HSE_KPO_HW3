using System.Text.Json;
using Common.Messages.Commands;
using Common.Messages.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.Data;
using OrdersService.Infrastructure.Messaging;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersDbContext    _db;
        private readonly IRabbitMqPublisher _publisher;

        public OrdersController(
            OrdersDbContext db,
            IRabbitMqPublisher publisher)
        {
            _db        = db;
            _publisher = publisher;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrder cmd)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var order = new Order {
                Id          = Guid.NewGuid(),
                UserId      = cmd.UserId,
                Amount      = cmd.Amount,
                Description = cmd.Description,
                Status      = OrderStatus.New,
                CreatedAt   = DateTime.UtcNow
            };
            _db.Orders.Add(order);

            var @event = new OrderCreated(cmd.UserId, order.Id, cmd.Amount, cmd.Description);
            _db.OutboxMessages.Add(new OutboxMessage {
                Id         = Guid.NewGuid(),
                OccurredAt = @event.OccurredAt,
                Type       = nameof(OrderCreated),
                Payload    = JsonSerializer.Serialize(@event),
                Processed  = false
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = order.Id },
                @event
            );
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var o = await _db.Orders
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id);

            if (o == null) 
                return NotFound();

            return Ok(new {
                o.Id,
                o.UserId,
                o.Amount,
                o.Description,
                o.Status,
                o.CreatedAt
            });
        }

        [HttpGet]
        public async Task<IEnumerable<object>> GetAll()
        {
            var list = await _db.Orders
                .AsNoTracking()
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return list
                .Select(o => new {
                    o.Id,
                    o.UserId,
                    o.Amount,
                    o.Description,
                    o.Status,
                    o.CreatedAt
                });
        }
    }
}
