using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DnDInitiativeTracker.ViewModels;

public sealed partial class TranslatorViewModel : ObservableObject
{
    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private string _outputText = string.Empty;

    partial void OnInputTextChanged(string value)
    {
        // Live translate: always display upper-case so the Draconic font renders correctly
        OutputText = value.ToUpperInvariant();
    }

    [RelayCommand]
    private void Clear()
    {
        InputText = string.Empty;
        OutputText = string.Empty;
    }
}

