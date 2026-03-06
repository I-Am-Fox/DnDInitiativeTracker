using CommunityToolkit.Mvvm.ComponentModel;
using DnDInitiativeTracker.Core.Models;

namespace DnDInitiativeTracker.ViewModels;

/// <summary>
/// Wraps a Combatant model for WPF binding, exposing mutable observable properties.
/// </summary>
public sealed partial class CombatantViewModel : ObservableObject
{
    public Combatant Model { get; }

    [ObservableProperty]
    private int _initiativeRoll;

    [ObservableProperty]
    private int _currentHitPoints;

    [ObservableProperty]
    private int _displayOrder;

    public string Name => Model.Name;
    public bool IsPlayerCharacter => Model.IsPlayerCharacter;
    public int MaxHitPoints => Model.MaxHitPoints;
    public int ArmorClass => Model.ArmorClass;
    public string? ImageUrl => Model.ImageUrl;

    public CombatantViewModel(Combatant model)
    {
        Model = model;
        _initiativeRoll = model.InitiativeRoll;
        _currentHitPoints = model.CurrentHitPoints;
        _displayOrder = model.DisplayOrder;
    }

    partial void OnInitiativeRollChanged(int value) => Model.InitiativeRoll = value;
    partial void OnCurrentHitPointsChanged(int value) => Model.CurrentHitPoints = value;
    partial void OnDisplayOrderChanged(int value) => Model.DisplayOrder = value;
}

