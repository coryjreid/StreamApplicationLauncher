using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using StreamApplicationLauncher.Models;

namespace StreamApplicationLauncher.Converters;

public class LogLevelToBrushConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value switch {
            LogLevel.Trace => Brushes.LightSlateGray,
            LogLevel.Debug => Brushes.Gray,
            LogLevel.Info => Brushes.LightGray,
            LogLevel.Warning => Brushes.DarkOrange,
            LogLevel.Error => Brushes.Red,
            LogLevel.Critical => Brushes.DarkRed,
            _ => Brushes.White
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return Binding.DoNothing;
    }
}