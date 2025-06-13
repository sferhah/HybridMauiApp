namespace HybridMauiApp.Cartography;

public class HybridGoogleMapsConfiguration
{
    public string? Language { get; set; }
    public string? MapControlPosition { get; set; }
    
    public string? ApiKey { get; set; }
    public Dictionary<string, string> Translations { get; set; } = new Dictionary<string, string>();

    public MauiPosition? Center { get; set; }
}
