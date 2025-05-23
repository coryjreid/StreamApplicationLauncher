using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using StreamApplicationLauncher.Models;

namespace StreamApplicationLauncher.Converters;

public class LogLevelToBackgroundBrushConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value switch {
            LogLevel.Warning => new SolidColorBrush(Color.FromArgb(20, 255, 165, 0)),
            LogLevel.Error => new SolidColorBrush(Color.FromArgb(30, 255, 0, 0)),
            LogLevel.Critical => new SolidColorBrush(Color.FromArgb(40, 139, 0, 0)), // dark red shade
            _ => Brushes.Transparent
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return Binding.DoNothing;
    }
}