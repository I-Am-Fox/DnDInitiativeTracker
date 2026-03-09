using System.Text.Json;
using DnDInitiativeTracker.Core.Interfaces.Repositories;
using DnDInitiativeTracker.Core.Models;

namespace DnDInitiativeTracker.Core.Repositories;

public sealed class JsonCharacterRepository : ICharacterRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _campaignsRoot;

    public JsonCharacterRepository(string dataRoot)
    {
        _campaignsRoot = Path.Combine(dataRoot, "Campaigns");
    }

    private string CharactersFile(string campaignSlug)
        => Path.Combine(_campaignsRoot, campaignSlug, "characters.json");

    public async Task<IReadOnlyList<CharacterProfile>> GetAllForCampaignAsync(
        string campaignSlug, CancellationToken cancellationToken = default)
    {
        var path = CharactersFile(campaignSlug);
        if (!File.Exists(path))
            return Array.Empty<CharacterProfile>();

        await using var stream = File.OpenRead(path);
        var list = await JsonSerializer.DeserializeAsync<List<CharacterProfile>>(stream, JsonOptions, cancellationToken);
        return list ?? new List<CharacterProfile>();
    }

    public async Task SaveAllAsync(string campaignSlug, IReadOnlyList<CharacterProfile> characters,
        CancellationToken cancellationToken = default)
    {
        var dir = Path.Combine(_campaignsRoot, campaignSlug);
        Directory.CreateDirectory(dir);

        var path = CharactersFile(campaignSlug);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, characters, JsonOptions, cancellationToken);
    }
}

