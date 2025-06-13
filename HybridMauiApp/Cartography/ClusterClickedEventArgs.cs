namespace HybridMauiApp.Cartography;

public sealed class ClusterClickedEventArgs : EventArgs
{
    public ICollection<MauiPin> Pins { get; }

    internal ClusterClickedEventArgs(ICollection<MauiPin> pins)
    {
        Pins = pins;
    }
}
