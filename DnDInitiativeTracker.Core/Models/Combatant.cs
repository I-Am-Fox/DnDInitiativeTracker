namespace DnDInitiativeTracker.Core.Models;

public sealed class Combatant
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public int InitiativeRoll { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPlayerCharacter { get; init; }
    public int MaxHitPoints { get; init; }
    public int CurrentHitPoints { get; set; }
    public int ArmorClass { get; init; }
    public string? ImageUrl { get; init; }
    public string? SourceCreatureId { get; init; }
}

