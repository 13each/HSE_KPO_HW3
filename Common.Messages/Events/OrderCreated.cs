namespace Common.Messages.Events
{
    public record OrderCreated(
        Guid UserId,
        Guid OrderId,
        decimal Amount,
        string Description,
        DateTime OccurredAt
    )
    {
        public OrderCreated(Guid userId, Guid orderId, decimal amount, string description)
            : this(userId, orderId, amount, description, DateTime.UtcNow) {}
    }
}