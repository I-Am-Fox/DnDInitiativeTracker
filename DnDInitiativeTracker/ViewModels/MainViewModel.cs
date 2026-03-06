using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDInitiativeTracker.Core.Models;
using DnDInitiativeTracker.Core.Repositories;
using DnDInitiativeTracker.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace DnDInitiativeTracker.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ISnackbarService _snackbar;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private Campaign? _selectedCampaign;

    [ObservableProperty]
    private string _newCampaignName = string.Empty;

    [ObservableProperty]
    private string _newCampaignDescription = string.Empty;

    [ObservableProperty]
    private bool _isCreatingCampaign;

    [ObservableProperty]
    private string? _errorMessage;

    public ObservableCollection<Campaign> Campaigns { get; } = new();

    public MainViewModel(ICampaignRepository campaignRepository, ISnackbarService snackbar,
        INavigationService navigation)
    {
        _campaignRepository = campaignRepository;
        _snackbar = snackbar;
        _navigation = navigation;
    }

    public async Task InitializeAsync()
    {
        var campaigns = await _campaignRepository.GetAllAsync();
        Campaigns.Clear();
        foreach (var c in campaigns)
            Campaigns.Add(c);
    }

    [RelayCommand]
    private void SelectCampaign(Campaign campaign)
    {
        SelectedCampaign = campaign;
        _navigation.Navigate(typeof(CampaignDetailPage));
    }

    [RelayCommand]
    private void BeginCreateCampaign()
    {
        NewCampaignName = string.Empty;
        NewCampaignDescription = string.Empty;
        ErrorMessage = null;
        IsCreatingCampaign = true;
    }

    [RelayCommand]
    private void CancelCreateCampaign()
    {
        IsCreatingCampaign = false;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task ConfirmCreateCampaignAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCampaignName))
        {
            ErrorMessage = "Campaign name is required.";
            return;
        }

        var slug = ToSlug(NewCampaignName);

        if (Campaigns.Any(c => c.Slug == slug))
        {
            ErrorMessage = "A campaign with that name already exists.";
            return;
        }

        var campaign = new Campaign
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = NewCampaignName.Trim(),
            Slug = slug,
            Description = string.IsNullOrWhiteSpace(NewCampaignDescription)
                ? null
                : NewCampaignDescription.Trim()
        };

        await _campaignRepository.SaveAsync(campaign);
        await InitializeAsync();

        IsCreatingCampaign = false;
        SelectedCampaign = Campaigns.FirstOrDefault(c => c.Slug == slug);
        ErrorMessage = null;

        _snackbar.Show("Campaign Created", $"\"{campaign.Name}\" has been added.",
            ControlAppearance.Success, new SymbolIcon(SymbolRegular.Checkmark24), TimeSpan.FromSeconds(3));
    }

    [RelayCommand]
    private async Task DeleteCampaignAsync(Campaign campaign)
    {
        await _campaignRepository.DeleteAsync(campaign.Slug);
        await InitializeAsync();

        if (SelectedCampaign?.Slug == campaign.Slug)
            SelectedCampaign = null;

        _snackbar.Show("Campaign Deleted", $"\"{campaign.Name}\" has been removed.",
            ControlAppearance.Caution, new SymbolIcon(SymbolRegular.Delete24), TimeSpan.FromSeconds(3));
    }

    private static string ToSlug(string name)
    {
        var slug = name.Trim()
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("&", "and");

        // Remove characters that are invalid in Windows file/directory names
        foreach (var c in Path.GetInvalidFileNameChars())
            slug = slug.Replace(c.ToString(), "");

        // Collapse multiple consecutive dashes into one and trim leading/trailing dashes
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");

        return slug.Trim('-');
    }
}
