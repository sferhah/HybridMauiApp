using System.Globalization;
using HybridMauiApp.Cartography;
using HybridMauiApp.Core;

namespace HybridMauiApp.Converters;

[AcceptEmptyServiceProvider]
public class PositionConverter : IValueConverter, IMarkupExtension
{
    public object ProvideValue(IServiceProvider serviceProvider) => this;

    object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is CrossPosition position ? position.ToMauiPosition() : null;

    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is MauiPosition position ? position.FromMauiPosition() : null;
}

public static class MapExtensions
{
    public static MauiPosition ToMauiPosition(this CrossPosition crossPlatformPosition) => new MauiPosition(crossPlatformPosition.Latitude, crossPlatformPosition.Longitude);

    public static CrossPosition FromMauiPosition(this MauiPosition crossPlatformPosition) => new CrossPosition(crossPlatformPosition.Latitude, crossPlatformPosition.Longitude);
}