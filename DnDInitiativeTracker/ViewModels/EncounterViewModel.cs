using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDInitiativeTracker.Core.Models;
using DnDInitiativeTracker.Core.Repositories;
using DnDInitiativeTracker.Core.Services;
using DnDInitiativeTracker.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace DnDInitiativeTracker.ViewModels;

public sealed partial class EncounterViewModel : ObservableObject
{
    private readonly IEncounterRepository _encounterRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly IInitiativeOrderingService _initiativeOrderingService;
    private readonly ICreatureCatalogService _creatureCatalogService;
    private readonly ISnackbarService _snackbar;
    private ShellViewModel? _shell;

    private string _campaignSlug = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EncounterName))]
    private Encounter? _selectedEncounter;

    [ObservableProperty]
    private string _newEncounterName = string.Empty;

    [ObservableProperty]
    private bool _isCreatingEncounter;

    [ObservableProperty]
    private bool _isImportingCreature;

    [ObservableProperty]
    private bool _isImportingFromCampaign;

    [ObservableProperty]
    private string _creatureSearchQuery = string.Empty;

    [ObservableProperty]
    private CreatureTemplate? _selectedCreatureTemplate;

    [ObservableProperty]
    private string _newCreatureInitiative = "0";

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _newCombatantName = string.Empty;

    [ObservableProperty]
    private string _newCombatantInitiative = string.Empty;

    [ObservableProperty]
    private bool _isAddingCombatant;

    // ── Campaign context ─────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCampaign))]
    [NotifyPropertyChangedFor(nameof(HasNoCampaign))]
    [NotifyPropertyChangedFor(nameof(SelectedCampaignName))]
    private Campaign? _activeCampaign;

    public bool HasCampaign => ActiveCampaign is not null;
    public bool HasNoCampaign => ActiveCampaign is null;

    public string SelectedCampaignName =>
        ActiveCampaign is not null ? $"Campaign: {ActiveCampaign.Name}" : "No campaign selected";

    public string EncounterName =>
        SelectedEncounter is not null ? SelectedEncounter.Name : "Select an encounter";

    // ── Collections ──────────────────────────────────────────────────────────

    public ObservableCollection<Encounter> Encounters { get; } = new();
    public ObservableCollection<CombatantViewModel> Combatants { get; } = new();
    public ObservableCollection<CreatureTemplate> SearchResults { get; } = new();
    public ObservableCollection<CampaignCharacterImportViewModel> CampaignCharacters { get; } = new();

    public EncounterViewModel(
        IEncounterRepository encounterRepository,
        ICharacterRepository characterRepository,
        IInitiativeOrderingService initiativeOrderingService,
        ICreatureCatalogService creatureCatalogService,
        ISnackbarService snackbar)
    {
        _encounterRepository = encounterRepository;
        _characterRepository = characterRepository;
        _initiativeOrderingService = initiativeOrderingService;
        _creatureCatalogService = creatureCatalogService;
        _snackbar = snackbar;
    }

    /// <summary>
    /// Called after DI construction to wire up the shell reference (avoids circular DI).
    /// </summary>
    public void SetShell(ShellViewModel shell) => _shell = shell;

    public async Task LoadForCampaignAsync(string campaignSlug, Campaign campaign)
    {
        _campaignSlug = campaignSlug;
        ActiveCampaign = campaign;
        var encounters = await _encounterRepository.GetAllForCampaignAsync(campaignSlug);
        Encounters.Clear();
        foreach (var e in encounters)
            Encounters.Add(e);
        SelectedEncounter = null;
        Combatants.Clear();
    }

    public void ClearCampaign()
    {
        ActiveCampaign = null;
        _campaignSlug = string.Empty;
        Encounters.Clear();
        SelectedEncounter = null;
        Combatants.Clear();
    }

    [RelayCommand]
    private void SelectEncounter(Encounter encounter)
    {
        SelectedEncounter = encounter;
        _shell?.NavigateTo<InitiativePage>();
    }

    [RelayCommand]
    private async Task DeleteEncounterAsync(Encounter encounter)
    {
        await _encounterRepository.DeleteAsync(_campaignSlug, encounter.Id);
        await LoadForCampaignAsync(_campaignSlug, ActiveCampaign!);

        _snackbar.Show("Encounter Deleted", $"\"{encounter.Name}\" has been removed.",
            ControlAppearance.Caution, new SymbolIcon(SymbolRegular.Delete24), TimeSpan.FromSeconds(3));
    }

    [RelayCommand]
    private void BeginCreateEncounter()
    {
        NewEncounterName = string.Empty;
        ErrorMessage = null;
        IsCreatingEncounter = true;
    }

    [RelayCommand]
    private void CancelCreateEncounter() => IsCreatingEncounter = false;

    [RelayCommand]
    private async Task ConfirmCreateEncounterAsync()
    {
        if (string.IsNullOrWhiteSpace(NewEncounterName))
        {
            ErrorMessage = "Encounter name is required.";
            return;
        }

        var encounter = new Encounter
        {
            Id = Guid.NewGuid().ToString("N"),
            CampaignId = _campaignSlug,
            Name = NewEncounterName.Trim()
        };

        await _encounterRepository.SaveAsync(_campaignSlug, encounter);
        await LoadForCampaignAsync(_campaignSlug, ActiveCampaign!);
        SelectedEncounter = Encounters.FirstOrDefault(e => e.Id == encounter.Id);
        IsCreatingEncounter = false;
        ErrorMessage = null;

        _snackbar.Show("Encounter Created", $"\"{encounter.Name}\" is ready.",
            ControlAppearance.Success, new SymbolIcon(SymbolRegular.Checkmark24), TimeSpan.FromSeconds(3));

        _shell?.NavigateTo<InitiativePage>();
    }

    partial void OnSelectedEncounterChanged(Encounter? value)
    {
        Combatants.Clear();
        if (value is null) return;

        var sorted = _initiativeOrderingService.AutoSort(value.Combatants);
        foreach (var c in sorted)
            Combatants.Add(new CombatantViewModel(c));
    }

    [RelayCommand]
    private void AutoSortCombatants()
    {
        if (SelectedEncounter is null) return;

        var sorted = _initiativeOrderingService.AutoSort(SelectedEncounter.Combatants);
        _initiativeOrderingService.ApplyDisplayOrder(SelectedEncounter.Combatants);

        Combatants.Clear();
        foreach (var c in sorted)
            Combatants.Add(new CombatantViewModel(c));
    }

    [RelayCommand]
    private void BeginAddCombatant()
    {
        NewCombatantName = string.Empty;
        NewCombatantInitiative = string.Empty;
        ErrorMessage = null;
        IsAddingCombatant = true;
    }

    [RelayCommand]
    private void CancelAddCombatant() => IsAddingCombatant = false;

    [RelayCommand]
    private async Task ConfirmAddCombatantAsync()
    {
        if (SelectedEncounter is null) return;

        if (string.IsNullOrWhiteSpace(NewCombatantName))
        {
            ErrorMessage = "Combatant name is required.";
            return;
        }

        if (!int.TryParse(NewCombatantInitiative, out var initiative))
        {
            ErrorMessage = "Initiative must be a whole number.";
            return;
        }

        var combatant = new Combatant
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = NewCombatantName.Trim(),
            InitiativeRoll = initiative,
            DisplayOrder = Combatants.Count,
            IsPlayerCharacter = true,
            MaxHitPoints = 0,
            CurrentHitPoints = 0
        };

        SelectedEncounter.Combatants.Add(combatant);
        await _encounterRepository.SaveAsync(_campaignSlug, SelectedEncounter);
        AutoSortCombatants();
        IsAddingCombatant = false;
        ErrorMessage = null;
    }

    [RelayCommand]
    private void BeginImportCreature()
    {
        CreatureSearchQuery = string.Empty;
        SearchResults.Clear();
        SelectedCreatureTemplate = null;
        NewCreatureInitiative = "0";
        ErrorMessage = null;
        IsImportingCreature = true;
    }

    [RelayCommand]
    private void CancelImportCreature() => IsImportingCreature = false;

    [RelayCommand]
    private async Task SearchCreaturesAsync()
    {
        if (string.IsNullOrWhiteSpace(CreatureSearchQuery)) return;

        IsSearching = true;
        ErrorMessage = null;
        SearchResults.Clear();

        try
        {
            var results = await _creatureCatalogService.SearchAsync(CreatureSearchQuery);
            foreach (var r in results)
                SearchResults.Add(r);

            if (SearchResults.Count == 0)
                ErrorMessage = "No creatures found.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Search failed: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task ConfirmImportCreatureAsync()
    {
        if (SelectedEncounter is null || SelectedCreatureTemplate is null) return;

        if (!int.TryParse(NewCreatureInitiative, out var initiative))
        {
            ErrorMessage = "Initiative must be a whole number.";
            return;
        }

        IsSearching = true;
        ErrorMessage = null;

        try
        {
            var detail = await _creatureCatalogService.GetByIdAsync(SelectedCreatureTemplate.SourceId)
                         ?? SelectedCreatureTemplate;

            var combatant = new Combatant
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = detail.Name,
                InitiativeRoll = initiative,
                DisplayOrder = Combatants.Count,
                IsPlayerCharacter = false,
                MaxHitPoints = detail.HitPoints ?? 0,
                CurrentHitPoints = detail.HitPoints ?? 0,
                ArmorClass = detail.ArmorClass ?? 0,
                SourceCreatureId = detail.SourceId
            };

            SelectedEncounter.Combatants.Add(combatant);
            await _encounterRepository.SaveAsync(_campaignSlug, SelectedEncounter);
            AutoSortCombatants();

            _snackbar.Show("Creature Imported", $"\"{detail.Name}\" added to initiative.",
                ControlAppearance.Success, new SymbolIcon(SymbolRegular.Checkmark24), TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Import failed: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
            IsImportingCreature = false;
        }
    }

    public async Task MoveCombatantAsync(int oldIndex, int newIndex)
    {
        if (SelectedEncounter is null) return;
        if (oldIndex == newIndex) return;

        var item = Combatants[oldIndex];
        Combatants.RemoveAt(oldIndex);
        Combatants.Insert(newIndex, item);

        for (var i = 0; i < Combatants.Count; i++)
        {
            Combatants[i].Model.DisplayOrder = i;
        }

        await _encounterRepository.SaveAsync(_campaignSlug, SelectedEncounter);
    }

    [RelayCommand]
    private async Task RemoveCombatantAsync(CombatantViewModel vm)
    {
        if (SelectedEncounter is null) return;
        SelectedEncounter.Combatants.Remove(vm.Model);
        await _encounterRepository.SaveAsync(_campaignSlug, SelectedEncounter);
        Combatants.Remove(vm);

        _snackbar.Show("Combatant Removed", $"\"{vm.Name}\" removed from initiative.",
            ControlAppearance.Caution, new SymbolIcon(SymbolRegular.Delete24), TimeSpan.FromSeconds(3));
    }

    // ── Import from Campaign ─────────────────────────────────────────────────

    private static void ShowPersistentError(string title, string message)
    {
        System.Windows.MessageBox.Show(message, title,
            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
    }

    [RelayCommand]
    private async Task BeginImportFromCampaignAsync()
    {
        if (ActiveCampaign is null)
        {
            ShowPersistentError("No Campaign", "Select a campaign first.");
            return;
        }

        if (SelectedEncounter is null)
        {
            ShowPersistentError("No Encounter", "Select an encounter first from the Encounters page.");
            return;
        }

        try
        {
            ErrorMessage = null;
            CampaignCharacters.Clear();

            var characters = await _characterRepository.GetAllForCampaignAsync(_campaignSlug);
            if (characters.Count == 0)
            {
                ShowPersistentError("No Characters",
                    "No characters in this campaign. Add characters from the Campaign Details page first.");
                return;
            }

            foreach (var c in characters)
                CampaignCharacters.Add(new CampaignCharacterImportViewModel(c));

            IsImportingFromCampaign = true;
        }
        catch (Exception ex)
        {
            ShowPersistentError("Import Failed", ex.ToString());
        }
    }

    [RelayCommand]
    private void CancelImportFromCampaign()
    {
        IsImportingFromCampaign = false;
        CampaignCharacters.Clear();
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task ConfirmImportFromCampaignAsync()
    {
        if (SelectedEncounter is null) return;

        var selected = CampaignCharacters.Where(c => c.IsSelected).ToList();
        if (selected.Count == 0)
        {
            ErrorMessage = "Select at least one character to import.";
            return;
        }

        try
        {
            foreach (var entry in selected)
            {
                if (!int.TryParse((string)entry.InitiativeText, out var initiative))
                {
                    ErrorMessage = $"Initiative for \"{entry.Name}\" must be a whole number.";
                    return;
                }

                var combatant = new Combatant
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = entry.Name,
                    InitiativeRoll = initiative,
                    DisplayOrder = Combatants.Count,
                    IsPlayerCharacter = true,
                    MaxHitPoints = 0,
                    CurrentHitPoints = 0,
                    ImageUrl = entry.Character.DirectImageUrl
                };

                SelectedEncounter.Combatants.Add(combatant);
            }

            await _encounterRepository.SaveAsync(_campaignSlug, SelectedEncounter);
            AutoSortCombatants();

            IsImportingFromCampaign = false;
            CampaignCharacters.Clear();
            ErrorMessage = null;

            _snackbar.Show("Characters Imported",
                $"{selected.Count} character(s) added to initiative.",
                ControlAppearance.Success, new SymbolIcon(SymbolRegular.Checkmark24), TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            ShowPersistentError("Import Failed", ex.ToString());
        }
    }
}

