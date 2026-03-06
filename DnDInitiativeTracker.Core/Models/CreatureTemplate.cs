namespace DnDInitiativeTracker.Core.Models;

public sealed class CreatureTemplate
{
    public required string SourceId { get; init; }
    public required string Name { get; init; }
    public string? Size { get; init; }
    public string? Type { get; init; }
    public int? ArmorClass { get; init; }
    public int? HitPoints { get; init; }
    public int? DexterityModifier { get; init; }
    public string? ChallengeRating { get; init; }
    public DateTime CachedAt { get; init; } = DateTime.UtcNow;
}

