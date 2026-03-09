using DnDInitiativeTracker.Core.Models;

namespace DnDInitiativeTracker.Core.Interfaces.Repositories;

public interface IEncounterRepository
{
    Task<IReadOnlyList<Encounter>> GetAllForCampaignAsync(string campaignSlug, CancellationToken cancellationToken = default);
    Task<Encounter?> GetByIdAsync(string campaignSlug, string encounterId, CancellationToken cancellationToken = default);
    Task SaveAsync(string campaignSlug, Encounter encounter, CancellationToken cancellationToken = default);
    Task DeleteAsync(string campaignSlug, string encounterId, CancellationToken cancellationToken = default);
}

