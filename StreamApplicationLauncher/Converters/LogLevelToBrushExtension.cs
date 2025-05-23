using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using StreamApplicationLauncher.Models;

namespace StreamApplicationLauncher.Converters;

[MarkupExtensionReturnType(typeof(Brush))]
public class LogLevelToBrushExtension : MarkupExtension, IValueConverter {
    private static readonly LogLevelToBrushExtension Instance = new();

    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

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

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}