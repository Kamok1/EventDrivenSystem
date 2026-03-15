namespace EventDrivenSystem.Models;

public abstract class BaseEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string SourceService { get; init; } = string.Empty;
    public string Data { get; init; } = string.Empty;
}
