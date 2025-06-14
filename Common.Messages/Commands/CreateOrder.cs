namespace Common.Messages.Commands
{
    public record CreateOrder(
        Guid UserId,
        decimal Amount,
        string Description
    );
}