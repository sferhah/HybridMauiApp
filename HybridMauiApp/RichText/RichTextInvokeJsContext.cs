using System.Text.Json.Serialization;

namespace HybridMauiApp.RichText;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(HybridRichTextConfig))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class RichTextInvokeJsContext : JsonSerializerContext
{

}
