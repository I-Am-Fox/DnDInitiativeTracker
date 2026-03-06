namespace DnDInitiativeTracker.Core.Models;

public sealed class Campaign
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? Description { get; init; }
    public string SchemaVersion { get; init; } = "1.0";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

