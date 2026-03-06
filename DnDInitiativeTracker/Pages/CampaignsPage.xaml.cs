using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DnDInitiativeTracker.Core.Models;
using DnDInitiativeTracker.ViewModels;

namespace DnDInitiativeTracker.Pages;

public partial class CampaignsPage : Page
{
    public CampaignsPage(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void CampaignCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: Campaign campaign }
            && DataContext is MainViewModel vm)
        {
            vm.SelectCampaignCommand.Execute(campaign);
        }
    }
}

