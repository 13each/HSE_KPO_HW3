namespace Common.Messages.Events
{
    public record PaymentProcessed(
        Guid OrderId,
        Guid PaymentId,
        decimal Amount,
        bool Success,
        DateTime OccurredAt
    )
    {
        public PaymentProcessed(Guid orderId, Guid paymentId, decimal amount, bool success)
            : this(orderId, paymentId, amount, success, DateTime.UtcNow) {}
    }
}