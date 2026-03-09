using DnDInitiativeTracker.Core.Models;

namespace DnDInitiativeTracker.Core.Interfaces.Services;

public interface ICreatureCatalogService
{
    /// <summary>
    /// Searches the community creature catalog for creatures matching the query.
    /// Only invoked on explicit user action (not at startup).
    /// </summary>
    Task<IReadOnlyList<CreatureTemplate>> SearchAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches full details for a single creature by its source ID.
    /// </summary>
    Task<CreatureTemplate?> GetByIdAsync(string sourceId, CancellationToken cancellationToken = default);
}

