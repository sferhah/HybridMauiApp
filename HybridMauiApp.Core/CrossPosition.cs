namespace HybridMauiApp.Core;

public readonly struct CrossPosition(double latitude, double longitude)
{
    public double Latitude { get; } = Math.Min(Math.Max(latitude, -90.0), 90.0);
    public double Longitude { get; } = Math.Min(Math.Max(longitude, -180.0), 180.0);
}