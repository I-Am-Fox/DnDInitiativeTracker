using System.Windows.Controls;
using DnDInitiativeTracker.ViewModels;

namespace DnDInitiativeTracker.Pages;

public partial class CampaignDetailPage : Page
{
    public CampaignDetailPage(CampaignDetailViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}

