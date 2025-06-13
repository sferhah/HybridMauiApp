using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;

namespace HybridMauiApp.RichText;

public class RichTextEditorHybridWebView : HybridWebView
{
    public RichTextEditorHybridWebView()
    {
        this.HybridRoot = "wwwroot/richtexteditor";
        this.DefaultFile = "index.html";        
        this.SetInvokeJavaScriptTarget(new HybridRichTextEditorJSInvoke(this));

        if (Application.Current != null)
        {
            Application.Current.RequestedThemeChanged += (s, e) =>
            {
                _ = this.InvokeJavaScriptAsync("setTheme", RichTextInvokeJsContext.Default.Double, [App.Current?.RequestedTheme == AppTheme.Dark], [RichTextInvokeJsContext.Default.Boolean]);
            };
        }
    }

    public async Task SetHtml(string? html)
    {
        html = html?.Replace("\r\n", "\r")
            .Replace("\n", "\r")
            .Replace("\r", string.Empty) ?? string.Empty;

        await this.InvokeJavaScriptAsync("setHtml", RichTextInvokeJsContext.Default.Double, [html], [RichTextInvokeJsContext.Default.String]);
    }

    public async Task<string?> GetHtml()
    {
        var data = await EvaluateJavaScriptAsync("getHtml()"); // Intentional usage of EvaluateJavaScriptAsync instead of InvokeJavaScriptAsync (crashes)
        string html = Regex.Unescape(data);
        return html;
    }

    public new async Task<string?> EvaluateJavaScriptAsync(string script)
    {
        await WaitForRichTextToBeReadyAsync();
        return await base.EvaluateJavaScriptAsync(script);
    }

    public new async Task<TReturnType?> InvokeJavaScriptAsync<TReturnType>(
        string methodName,
        JsonTypeInfo<TReturnType> returnTypeJsonTypeInfo,
        object?[]? paramValues = null,
        JsonTypeInfo?[]? paramJsonTypeInfos = null)
    {
        await WaitForRichTextToBeReadyAsync();
        return await base.InvokeJavaScriptAsync(methodName, returnTypeJsonTypeInfo, paramValues, paramJsonTypeInfos);
    }

    private TaskCompletionSource<bool> _tcs_RichText = new TaskCompletionSource<bool>();
    public Task WaitForRichTextToBeReadyAsync() => _tcs_RichText.Task;


    public class HybridRichTextEditorJSInvoke
    {
        readonly RichTextEditorHybridWebView _hybridRichTextEditor;

        public HybridRichTextEditorJSInvoke(RichTextEditorHybridWebView hybridRichTextEditor)
            => _hybridRichTextEditor = hybridRichTextEditor;

        public void OnRichTextReady()
            => _hybridRichTextEditor._tcs_RichText.TrySetResult(true);

        public HybridRichTextConfig GetHybridRichTextConfig() => new HybridRichTextConfig 
        {
            IsDarkTheme = App.Current?.RequestedTheme == AppTheme.Dark,
            Translations = new Dictionary<string, string>
            {
                { "title", "Title" },
                { "white", "White" },
                { "black", "Black" },
                { "brown", "Brown" },
                { "beige", "Beige" },
                { "darkBlue", "Dark Blue" },
                { "blue", "Blue" },
                { "lightBlue", "Light Blue" },
                { "darkRed", "Dark Red" },
                { "red", "Red" },
                { "darkGreen", "Dark Green" },
                { "green", "Green" },
                { "purple", "Purple" },
                { "darkTurquois", "Dark Turquois" },
                { "turquois", "Turquois" },
                { "darkOrange", "Dark Orange" },
                { "orange", "Orange" },
                { "yellow", "Yellow" },
                { "imageURL", "Image URL" },
                { "fileURL", "File URL" },
                { "linkText", "Link text" },
                { "url", "URL" },
                { "size", "Size" },
                { "responsive", "Responsive" },
                { "text", "Text" },
                { "openIn", "Open in" },
                { "sameTab", "Same tab" },
                { "newTab", "New tab" },
                { "align", "Align" },
                { "left", "Left" },
                { "justify", "Justify" },
                { "center", "Center" },
                { "right", "Right" },
                { "rows", "Rows" },
                { "columns", "Columns" },
                { "add", "Add" },
                { "pleaseEnterURL", "Please enter an URL" },
                { "videoURLnotSupported", "Video URL not supported" },
                { "pleaseSelectImage", "Please select an image" },
                { "pleaseSelectFile", "Please select a file" },
                { "bold", "Bold" },
                { "italic", "Italic" },
                { "underline", "Underline" },
                { "alignLeft", "Align Left" },
                { "alignCenter", "Align Centered" },
                { "alignRight", "Align Right" },
                { "addOrderedList", "Ordered List" },
                { "addUnorderedList", "Unordered List" },
                { "addHeading", "Heading/title" },
                { "addFont", "Font" },
                { "addFontColor", "Font color" },
                { "addBackgroundColor", "Background color" },
                { "addFontSize", "Font size" },
                { "addImage", "Add image" },
                { "addVideo", "Add video" },
                { "addFile", "Add file" },
                { "addURL", "Add URL" },
                { "addTable", "Add table" },
                { "removeStyles", "Remove styles" },
                { "code", "Show HTML code" },
                { "undo", "Undo" },
                { "redo", "Redo" },
                { "save", "Save" },
                { "close", "Close" }
            }
        };
    }
}
