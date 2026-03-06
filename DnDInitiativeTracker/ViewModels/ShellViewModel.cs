using CommunityToolkit.Mvvm.ComponentModel;
using DnDInitiativeTracker.Core.Models;
using DnDInitiativeTracker.Core.Repositories;

namespace DnDInitiativeTracker.ViewModels;

/// <summary>
/// Shell view model that owns MainViewModel, EncounterViewModel,
/// and CampaignDetailViewModel, coordinating campaign selection.
/// </summary>
public sealed partial class ShellViewModel : ObservableObject
{
    public MainViewModel MainViewModel { get; }
    public EncounterViewModel EncounterViewModel { get; }
    public CampaignDetailViewModel CampaignDetailViewModel { get; }

    public ShellViewModel(MainViewModel mainViewModel, EncounterViewModel encounterViewModel,
        CampaignDetailViewModel campaignDetailViewModel)
    {
        MainViewModel = mainViewModel;
        EncounterViewModel = encounterViewModel;
        CampaignDetailViewModel = campaignDetailViewModel;

        MainViewModel.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedCampaign))
            {
                await OnCampaignSelectedAsync(MainViewModel.SelectedCampaign);
            }
        };
    }

    private async Task OnCampaignSelectedAsync(Campaign? campaign)
    {
        if (campaign is null)
        {
            EncounterViewModel.ClearCampaign();
            return;
        }

        await CampaignDetailViewModel.LoadAsync(campaign);
        await EncounterViewModel.LoadForCampaignAsync(campaign.Slug, campaign);
    }
}

