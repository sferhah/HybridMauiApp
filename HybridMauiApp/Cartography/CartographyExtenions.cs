using GoogleMapsComponents.Maps;

namespace HybridMauiApp.Cartography;

public static class CartographyExtenions
{
    public static AdvancedMarkerElementOptions ToAdvancedMarkerElementOptions(this MauiPin pin) => new AdvancedMarkerElementOptions
    {
        Position = pin.Position.ToLatLngLiteral(),
        Content = (pin.Color ?? Colors.Red).ToPinElement()
    };

    public static PinElement ToPinElement(this Color color) => new PinElement
    {
        BorderColor = color.WithLuminosity(color.GetLuminosity() * 0.8f).ToRgbaHex(),
        Background = color.ToRgbaHex(),
        GlyphColor = color.WithLuminosity(color.GetLuminosity() * 0.6f).ToRgbaHex(),        
    };

    public static LatLngLiteral ToLatLngLiteral(this MauiPosition position) => new LatLngLiteral(position.Latitude, position.Longitude);

    public static MapRegion ToRegion(this LatLngBoundsLiteral bounds) 
        => new MapRegion { NearLeft = new MauiPosition(bounds.South, bounds.West), FarRight = new MauiPosition(bounds.North, bounds.East) };
}
