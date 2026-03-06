using System.Windows;
using System.Windows.Threading;
using DnDInitiativeTracker.Pages;
using DnDInitiativeTracker.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Abstractions;

namespace DnDInitiativeTracker;

public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly INavigationService _navigation;
    private readonly ISnackbarService _snackbars;

    public MainWindow(MainWindowViewModel vm, INavigationService navigation,
        INavigationViewPageProvider pageProvider, ISnackbarService snackbars)
    {
        InitializeComponent();
        DataContext = vm;

        _snackbars = snackbars;
        _navigation = navigation;
        _navigation.SetNavigationControl(RootNavigationView);
        RootNavigationView.SetPageProviderService(pageProvider);

        _snackbars.SetSnackbarPresenter(SnackbarPresenter);

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(() => _navigation.Navigate(typeof(CampaignsPage)), DispatcherPriority.Loaded);
    }
}

