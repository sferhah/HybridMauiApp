using System.Globalization;

namespace HybridMauiApp.Converters;

[AcceptEmptyServiceProvider]
public class ColorConverter : IValueConverter, IMarkupExtension
{
    public object ProvideValue(IServiceProvider serviceProvider)
        => this;

    object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is System.Drawing.Color crossColor ? Color.FromRgba(crossColor.R, crossColor.G, crossColor.B, crossColor.A) : Colors.Transparent;

    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}