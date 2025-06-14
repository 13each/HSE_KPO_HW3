namespace PaymentsService.Domain.Entities;

public class InboxMessage
{
    public Guid Id { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string Type { get; set; } = null!;
    public string Payload { get; set; } = null!;
}