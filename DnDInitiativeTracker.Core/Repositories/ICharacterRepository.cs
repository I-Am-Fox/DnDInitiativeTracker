using DnDInitiativeTracker.Core.Models;

namespace DnDInitiativeTracker.Core.Repositories;

public interface ICharacterRepository
{
    Task<IReadOnlyList<CharacterProfile>> GetAllForCampaignAsync(string campaignSlug, CancellationToken cancellationToken = default);
    Task SaveAllAsync(string campaignSlug, IReadOnlyList<CharacterProfile> characters, CancellationToken cancellationToken = default);
}

