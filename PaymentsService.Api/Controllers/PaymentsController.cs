using System.Text.Json;
using Common.Messages.Commands;
using Common.Messages.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Entities;
using PaymentsService.Infrastructure.Data;

namespace PaymentsService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentsDbContext _db;

        public PaymentsController(PaymentsDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Process([FromBody] ProcessOrderPayment cmd)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var payment = new Payment {
                Id        = Guid.NewGuid(),
                OrderId   = cmd.OrderId,
                Amount    = cmd.Amount,
                Status    = PaymentStatus.Processed,
                CreatedAt = DateTime.UtcNow
            };
            _db.Payments.Add(payment);

            var @event = new PaymentProcessed(
                cmd.OrderId,
                payment.Id,
                cmd.Amount,
                success: true
            );

            _db.OutboxMessages.Add(new OutboxMessage {
                Id         = Guid.NewGuid(),
                OccurredAt = @event.OccurredAt,
                Type       = nameof(PaymentProcessed),
                Payload    = JsonSerializer.Serialize(@event),
                Processed  = false
            });


            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Accepted(@event);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<IActionResult> GetByOrder(Guid orderId)
        {
            var p = await _db.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (p == null) 
                return NotFound();

            return Ok(new {
                p.Id,
                p.OrderId,
                p.Amount,
                p.Status,
                p.CreatedAt
            });
        }
    }
}
