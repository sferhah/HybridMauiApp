namespace HybridMauiApp.Cartography;

public class InfoWindowClickedEventArgs : EventArgs
{
    public MauiPin Pin { get; }
    public InfoWindowClickedEventArgs(MauiPin pin) => Pin = pin;
}
