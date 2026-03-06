namespace DnDInitiativeTracker.Core.Models;

public sealed class CharacterProfile
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? DirectImageUrl { get; init; }
    public int DexterityModifier { get; init; } = 0;
}

