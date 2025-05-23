using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using StreamApplicationLauncher.Models;

namespace StreamApplicationLauncher.Converters;

public class LogLevelToBackgroundBrushConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value switch {
            LogLevel.Info => Brushes.Transparent,
            LogLevel.Warning => new SolidColorBrush(Color.FromArgb(20, 255, 165, 0)), // light orange
            LogLevel.Error => new SolidColorBrush(Color.FromArgb(30, 255, 0, 0)), // light red
            _ => Brushes.Transparent
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}