using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DnDInitiativeTracker.Converters;

/// <summary>
/// Returns Visible when the value is a non-null, non-empty string; Collapsed otherwise.
/// </summary>
public sealed class NotNullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrEmpty(s)
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

