using System.Windows;
using GalaxyClassifierCapstone.Pages;
using GalaxyClassifierCapstone.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Abstractions;

namespace GalaxyClassifierCapstone.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
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
        // Navigate to the first page
        _navigation.Navigate(typeof(IdentificationPage));
    }
}