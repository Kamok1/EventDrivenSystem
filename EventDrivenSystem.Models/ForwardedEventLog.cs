namespace EventDrivenSystem.Models;

public class ForwardedEventLog
{
    public int Id { get; set; }
    public Guid OriginalEventId { get; set; }
    public Guid ForwardedEventId { get; set; }
    public string ForwardedEventType { get; set; } = string.Empty;
    public DateTime ForwardedAt { get; set; } = DateTime.UtcNow;
}
