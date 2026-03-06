using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DnDInitiativeTracker.Core.Models;

namespace DnDInitiativeTracker.Core.Services;

/// <summary>
/// Adapter for the community 5e API at https://www.dnd5eapi.co
/// Only fetches on explicit user action; results are not cached here
/// (cache layer is handled by the repository).
/// </summary>
public sealed class FiveBitsCreatureCatalogService : ICreatureCatalogService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "https://www.dnd5eapi.co/api";

    public FiveBitsCreatureCatalogService(HttpClient http)
    {
        _http = http;
        _http.BaseAddress ??= new Uri(BaseUrl);
    }

    public async Task<IReadOnlyList<CreatureTemplate>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<CreatureTemplate>();

        var index = await _http.GetFromJsonAsync<MonsterIndexResponse>(
            "/api/monsters", cancellationToken);

        if (index?.Results is null)
            return Array.Empty<CreatureTemplate>();

        return index.Results
            .Where(r => r.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(20)
            .Select(r => new CreatureTemplate
            {
                SourceId = r.Index,
                Name = r.Name
            })
            .ToList();
    }

    public async Task<CreatureTemplate?> GetByIdAsync(string sourceId, CancellationToken cancellationToken = default)
    {
        var detail = await _http.GetFromJsonAsync<MonsterDetailResponse>(
            $"/api/monsters/{sourceId}", cancellationToken);

        if (detail is null) return null;

        return new CreatureTemplate
        {
            SourceId = detail.Index,
            Name = detail.Name,
            Size = detail.Size,
            Type = detail.Type,
            ArmorClass = detail.ArmorClass?.FirstOrDefault()?.Value,
            HitPoints = detail.HitPoints,
            DexterityModifier = CalculateModifier(detail.Dexterity),
            ChallengeRating = detail.ChallengeRating?.ToString(),
            CachedAt = DateTime.UtcNow
        };
    }

    private static int CalculateModifier(int? score)
        => score.HasValue ? (int)Math.Floor((score.Value - 10) / 2.0) : 0;

    // ── DTOs ──────────────────────────────────────────────────────────────────

    private sealed class MonsterIndexResponse
    {
        [JsonPropertyName("results")]
        public List<MonsterIndexItem>? Results { get; init; }
    }

    private sealed class MonsterIndexItem
    {
        [JsonPropertyName("index")]
        public required string Index { get; init; }

        [JsonPropertyName("name")]
        public required string Name { get; init; }
    }

    private sealed class MonsterDetailResponse
    {
        [JsonPropertyName("index")]
        public required string Index { get; init; }

        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("size")]
        public string? Size { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("armor_class")]
        public List<ArmorClassEntry>? ArmorClass { get; init; }

        [JsonPropertyName("hit_points")]
        public int? HitPoints { get; init; }

        [JsonPropertyName("dexterity")]
        public int? Dexterity { get; init; }

        [JsonPropertyName("challenge_rating")]
        public double? ChallengeRating { get; init; }
    }

    private sealed class ArmorClassEntry
    {
        [JsonPropertyName("value")]
        public int Value { get; init; }
    }
}

