namespace DnDInitiativeTracker.Core.Models;

public sealed class Encounter
{
    public required string Id { get; init; }
    public required string CampaignId { get; init; }
    public required string Name { get; init; }
    public string SchemaVersion { get; init; } = "1.0";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public List<Combatant> Combatants { get; init; } = new();
}

