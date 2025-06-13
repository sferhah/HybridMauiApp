namespace HybridMauiApp.Cartography;

public sealed class MapClickedEventArgs : EventArgs
{
    public MauiPosition Point { get; }

    public MapClickedEventArgs(MauiPosition point)
    {
        Point = point;
    }
}
