using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using StreamApplicationLauncher.Models;

namespace StreamApplicationLauncher.Converters;

public class LogLevelToBrushConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value switch {
            LogLevel.Info => Brushes.Green,
            LogLevel.Warning => Brushes.Orange,
            LogLevel.Error => Brushes.Red,
            LogLevel.Debug => Brushes.Gray,
            _ => Brushes.Black
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}