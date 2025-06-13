using System.Text.Json.Serialization;

namespace HybridMauiApp.Cartography;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(MauiPosition))]
[JsonSerializable(typeof(MapRegion))]
[JsonSerializable(typeof(JsPin))]
[JsonSerializable(typeof(JsPin[]))]
[JsonSerializable(typeof(HybridGoogleMapsConfiguration))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(double))]
internal partial class GoogleMapsInvokeJsContext : JsonSerializerContext
{

}
