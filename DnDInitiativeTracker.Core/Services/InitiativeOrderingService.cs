using DnDInitiativeTracker.Core.Interfaces.Services;
using DnDInitiativeTracker.Core.Models;

namespace DnDInitiativeTracker.Core.Services;

public sealed class InitiativeOrderingService : IInitiativeOrderingService
{
    public IReadOnlyList<Combatant> AutoSort(IEnumerable<Combatant> combatants)
    {
        return combatants
            .OrderByDescending(c => c.InitiativeRoll)
            .ThenBy(c => c.DisplayOrder)
            .ToList();
    }

    public void ApplyDisplayOrder(IList<Combatant> orderedCombatants)
    {
        for (var i = 0; i < orderedCombatants.Count; i++)
        {
            orderedCombatants[i].DisplayOrder = i;
        }
    }
}

