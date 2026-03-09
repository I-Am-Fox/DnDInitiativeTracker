using DnDInitiativeTracker.Core.Models;

namespace DnDInitiativeTracker.Core.Interfaces.Services;

public interface IInitiativeOrderingService
{
    /// <summary>
    /// Sorts combatants by roll descending, then by existing DisplayOrder for ties.
    /// Returns a new list; does not mutate input.
    /// </summary>
    IReadOnlyList<Combatant> AutoSort(IEnumerable<Combatant> combatants);

    /// <summary>
    /// Applies sequential DisplayOrder values to the given ordered list in place.
    /// </summary>
    void ApplyDisplayOrder(IList<Combatant> orderedCombatants);
}

