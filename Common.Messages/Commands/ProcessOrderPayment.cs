namespace Common.Messages.Commands
{
    public record ProcessOrderPayment(
        Guid UserId,
        Guid OrderId,
        decimal Amount
    );
}