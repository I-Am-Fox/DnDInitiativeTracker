using CommunityToolkit.Mvvm.ComponentModel;
using DnDInitiativeTracker.Core.Models;

namespace DnDInitiativeTracker.ViewModels;

/// <summary>
/// Wraps a CharacterProfile for the import-from-campaign UI,
/// adding a selection checkbox and an initiative text field.
/// </summary>
public sealed partial class CampaignCharacterImportViewModel : ObservableObject
{
    public CharacterProfile Character { get; }

    public string Name => Character.Name;

    [ObservableProperty]
    private bool _isSelected = true;

    [ObservableProperty]
    private string _initiativeText = "0";

    public CampaignCharacterImportViewModel(CharacterProfile character)
    {
        Character = character;
    }
}

