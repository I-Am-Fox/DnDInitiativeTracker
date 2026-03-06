using System.Text.Json;
using DnDInitiativeTracker.Core.Models;

namespace DnDInitiativeTracker.Core.Repositories;

public sealed class JsonCampaignRepository : ICampaignRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _campaignsRoot;

    public JsonCampaignRepository(string dataRoot)
    {
        _campaignsRoot = Path.Combine(dataRoot, "Campaigns");
    }

    public async Task<IReadOnlyList<Campaign>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_campaignsRoot))
            return Array.Empty<Campaign>();

        var list = new List<Campaign>();

        foreach (var dir in Directory.GetDirectories(_campaignsRoot))
        {
            var path = Path.Combine(dir, "campaign.json");
            if (!File.Exists(path)) continue;

            await using var stream = File.OpenRead(path);
            var campaign = await JsonSerializer.DeserializeAsync<Campaign>(stream, JsonOptions, cancellationToken);
            if (campaign is not null)
                list.Add(campaign);
        }

        return list.OrderBy(c => c.Name).ToList();
    }

    public async Task<Campaign?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_campaignsRoot, slug, "campaign.json");
        if (!File.Exists(path)) return null;

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<Campaign>(stream, JsonOptions, cancellationToken);
    }

    public async Task SaveAsync(Campaign campaign, CancellationToken cancellationToken = default)
    {
        var campaignRoot = Path.Combine(_campaignsRoot, campaign.Slug);
        Directory.CreateDirectory(campaignRoot);
        Directory.CreateDirectory(Path.Combine(campaignRoot, "encounters"));
        Directory.CreateDirectory(Path.Combine(campaignRoot, "assets"));

        var path = Path.Combine(campaignRoot, "campaign.json");
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, campaign, JsonOptions, cancellationToken);
    }

    public Task DeleteAsync(string slug, CancellationToken cancellationToken = default)
    {
        var campaignRoot = Path.Combine(_campaignsRoot, slug);
        if (Directory.Exists(campaignRoot))
            Directory.Delete(campaignRoot, recursive: true);

        return Task.CompletedTask;
    }
}

