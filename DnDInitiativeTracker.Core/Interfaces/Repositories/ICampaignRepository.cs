using DnDInitiativeTracker.Core.Models;

namespace DnDInitiativeTracker.Core.Interfaces.Repositories;

public interface ICampaignRepository
{
    Task<IReadOnlyList<Campaign>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Campaign?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task SaveAsync(Campaign campaign, CancellationToken cancellationToken = default);
    Task DeleteAsync(string slug, CancellationToken cancellationToken = default);
}

