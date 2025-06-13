using GoogleMapsComponents;
using GoogleMapsComponents.Maps;
using System.Globalization;

namespace HybridMauiApp.Cartography;

public class MyBlazorGoogleMapsKeyService : IBlazorGoogleMapsKeyService
{
    public bool IsApiInitialized { get; set; }
    public async Task<MapApiLoadOptions> GetApiOptions()
    {
        var apiKey = await GoogleApiKeyService.GetGoogleApiKey();

        return new MapApiLoadOptions(apiKey)
        {
            Version = "beta",
            Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
            //Libraries = "places,visualization,drawing,marker",
        };
    }
}