namespace HybridMauiApp.Cartography;

public struct MauiPosition
{
    public MauiPosition()
    {
    }

    public MauiPosition(double latitude, double longitude)
    {
        Latitude = Math.Min(Math.Max(latitude, -90.0), 90.0);
        Longitude = Math.Min(Math.Max(longitude, -180.0), 180.0);
    }

    public double Latitude { get; set; }
    public double Longitude { get; set; }
}