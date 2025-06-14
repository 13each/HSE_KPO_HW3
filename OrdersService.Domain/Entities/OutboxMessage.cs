namespace OrdersService.Domain.Entities
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public string Type { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public bool Processed { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}