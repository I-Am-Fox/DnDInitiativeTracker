using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDInitiativeTracker.Core.Models;
using DnDInitiativeTracker.Core.Repositories;
using DnDInitiativeTracker.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace DnDInitiativeTracker.ViewModels;

/// <summary>
/// Shell view model that owns navigation, page switching, and campaign coordination.
/// </summary>
public sealed partial class ShellViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;

    public MainViewModel MainViewModel { get; }
    public EncounterViewModel EncounterViewModel { get; }
    public CampaignDetailViewModel CampaignDetailViewModel { get; }

    [ObservableProperty]
    private Page? _currentPage;

    [ObservableProperty]
    private int _selectedNavIndex;

    // Nav items: 0=Campaigns, 1=CampaignDetails, 2=Encounters, 3=Initiative, 4=Settings
    public ShellViewModel(
        MainViewModel mainViewModel,
        EncounterViewModel encounterViewModel,
        CampaignDetailViewModel campaignDetailViewModel,
        IServiceProvider serviceProvider)
    {
        MainViewModel = mainViewModel;
        EncounterViewModel = encounterViewModel;
        CampaignDetailViewModel = campaignDetailViewModel;
        _serviceProvider = serviceProvider;

        MainViewModel.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedCampaign))
            {
                await OnCampaignSelectedAsync(MainViewModel.SelectedCampaign);
            }
        };
    }

    public void NavigateTo<TPage>() where TPage : Page
    {
        CurrentPage = _serviceProvider.GetRequiredService<TPage>();
        UpdateSelectedNavIndex<TPage>();
    }

    public void NavigateTo(Type pageType)
    {
        if (_serviceProvider.GetRequiredService(pageType) is Page page)
        {
            CurrentPage = page;
            UpdateSelectedNavIndexByType(pageType);
        }
    }

    [RelayCommand]
    public void Navigate(string target)
    {
        switch (target)
        {
            case "Campaigns":
                NavigateTo<CampaignsPage>();
                break;
            case "CampaignDetails":
                NavigateTo<CampaignDetailPage>();
                break;
            case "Encounters":
                NavigateTo<EncountersPage>();
                break;
            case "Initiative":
                NavigateTo<InitiativePage>();
                break;
            case "Settings":
                NavigateTo<SettingsPage>();
                break;
        }
    }

    private void UpdateSelectedNavIndex<TPage>() where TPage : Page
    {
        UpdateSelectedNavIndexByType(typeof(TPage));
    }

    private void UpdateSelectedNavIndexByType(Type pageType)
    {
        if (pageType == typeof(CampaignsPage)) SelectedNavIndex = 0;
        else if (pageType == typeof(CampaignDetailPage)) SelectedNavIndex = 1;
        else if (pageType == typeof(EncountersPage)) SelectedNavIndex = 2;
        else if (pageType == typeof(InitiativePage)) SelectedNavIndex = 3;
        else if (pageType == typeof(SettingsPage)) SelectedNavIndex = 4;
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

