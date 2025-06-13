using System.Globalization;
using HybridMauiApp.Cartography;
using HybridMauiApp.Core;

namespace HybridMauiApp.Converters;

[AcceptEmptyServiceProvider]
public class PositionConverter : IValueConverter, IMarkupExtension
{
    public object ProvideValue(IServiceProvider serviceProvider) => this;

    object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is CrossPosition position ? position.ToGoogleMapsPoition() : null;

    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is MauiPosition position ? position.FromGoogleMapsPoition() : null;
}

public static class MapExtensions
{
    public static MauiPosition? ToGoogleMapsPoition(this CrossPosition? crossPlatformPosition) => crossPlatformPosition?.ToGoogleMapsPoition();

    public static MauiPosition ToGoogleMapsPoition(this CrossPosition crossPlatformPosition) => new MauiPosition(crossPlatformPosition.Latitude, crossPlatformPosition.Longitude);

    public static CrossPosition? FromGoogleMapsPoition(this MauiPosition? position) => position?.FromGoogleMapsPoition();

    public static CrossPosition FromGoogleMapsPoition(this MauiPosition crossPlatformPosition) => new CrossPosition(crossPlatformPosition.Latitude, crossPlatformPosition.Longitude);
}