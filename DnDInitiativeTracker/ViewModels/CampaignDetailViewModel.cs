using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDInitiativeTracker.Core.Models;
using DnDInitiativeTracker.Core.Repositories;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace DnDInitiativeTracker.ViewModels;

public sealed partial class CampaignDetailViewModel : ObservableObject
{
    private readonly ICharacterRepository _characterRepository;
    private readonly ISnackbarService _snackbar;

    private string _campaignSlug = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CampaignName))]
    [NotifyPropertyChangedFor(nameof(CampaignDescription))]
    [NotifyPropertyChangedFor(nameof(HasCampaign))]
    private Campaign? _campaign;

    [ObservableProperty]
    private bool _isAddingCharacter;

    [ObservableProperty]
    private string _newCharacterName = string.Empty;


    [ObservableProperty]
    private string? _errorMessage;

    public string CampaignName => Campaign?.Name ?? "No Campaign";
    public string? CampaignDescription => Campaign?.Description;
    public bool HasCampaign => Campaign is not null;

    public ObservableCollection<CharacterProfile> Characters { get; } = new();

    public CampaignDetailViewModel(ICharacterRepository characterRepository, ISnackbarService snackbar)
    {
        _characterRepository = characterRepository;
        _snackbar = snackbar;
    }

    public async Task LoadAsync(Campaign campaign)
    {
        Campaign = campaign;
        _campaignSlug = campaign.Slug;

        var characters = await _characterRepository.GetAllForCampaignAsync(_campaignSlug);
        Characters.Clear();
        foreach (var c in characters)
            Characters.Add(c);
    }

    [RelayCommand]
    private void BeginAddCharacter()
    {
        NewCharacterName = string.Empty;
        ErrorMessage = null;
        IsAddingCharacter = true;
    }

    [RelayCommand]
    private void CancelAddCharacter()
    {
        IsAddingCharacter = false;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task ConfirmAddCharacterAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCharacterName))
        {
            ErrorMessage = "Character name is required.";
            return;
        }

        var character = new CharacterProfile
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = NewCharacterName.Trim()
        };

        Characters.Add(character);
        await SaveCharactersAsync();

        IsAddingCharacter = false;
        ErrorMessage = null;

        _snackbar.Show("Character Added", $"\"{character.Name}\" has been added to the campaign.",
            ControlAppearance.Success, new SymbolIcon(SymbolRegular.Checkmark24), TimeSpan.FromSeconds(3));
    }

    [RelayCommand]
    private async Task DeleteCharacterAsync(CharacterProfile character)
    {
        Characters.Remove(character);
        await SaveCharactersAsync();

        _snackbar.Show("Character Removed", $"\"{character.Name}\" has been removed.",
            ControlAppearance.Caution, new SymbolIcon(SymbolRegular.Delete24), TimeSpan.FromSeconds(3));
    }

    private async Task SaveCharactersAsync()
    {
        await _characterRepository.SaveAllAsync(_campaignSlug, Characters.ToList());
    }
}

