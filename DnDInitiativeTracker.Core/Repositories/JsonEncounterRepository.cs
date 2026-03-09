using System.Text.Json;
using DnDInitiativeTracker.Core.Interfaces.Repositories;
using DnDInitiativeTracker.Core.Models;

namespace DnDInitiativeTracker.Core.Repositories;

public sealed class JsonEncounterRepository : IEncounterRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _campaignsRoot;

    public JsonEncounterRepository(string dataRoot)
    {
        _campaignsRoot = Path.Combine(dataRoot, "Campaigns");
    }

    private string EncountersDir(string campaignSlug)
        => Path.Combine(_campaignsRoot, campaignSlug, "encounters");

    public async Task<IReadOnlyList<Encounter>> GetAllForCampaignAsync(string campaignSlug, CancellationToken cancellationToken = default)
    {
        var dir = EncountersDir(campaignSlug);
        if (!Directory.Exists(dir)) return Array.Empty<Encounter>();

        var list = new List<Encounter>();

        foreach (var file in Directory.GetFiles(dir, "*.json"))
        {
            await using var stream = File.OpenRead(file);
            var encounter = await JsonSerializer.DeserializeAsync<Encounter>(stream, JsonOptions, cancellationToken);
            if (encounter is not null)
                list.Add(encounter);
        }

        return list.OrderBy(e => e.CreatedAt).ToList();
    }

    public async Task<Encounter?> GetByIdAsync(string campaignSlug, string encounterId, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(EncountersDir(campaignSlug), $"{encounterId}.json");
        if (!File.Exists(path)) return null;

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<Encounter>(stream, JsonOptions, cancellationToken);
    }

    public async Task SaveAsync(string campaignSlug, Encounter encounter, CancellationToken cancellationToken = default)
    {
        var dir = EncountersDir(campaignSlug);
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, $"{encounter.Id}.json");
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, encounter, JsonOptions, cancellationToken);
    }

    public Task DeleteAsync(string campaignSlug, string encounterId, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(EncountersDir(campaignSlug), $"{encounterId}.json");
        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }
}

