namespace HybridMauiApp;

public static class GoogleApiKeyService
{
    // Don't forget to protect your key
    public static async Task<string> GetGoogleApiKey()
    {
        await Task.Delay(1);
        return ""; // using an empty key displays a map with "For development purposes only" message
    }
}