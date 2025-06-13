namespace HybridMauiApp;

public partial class RichTextEditorHybridPage : ContentPage
{
    public RichTextEditorHybridPage() => InitializeComponent();

    bool firstApper = true;
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (firstApper)
        {
            firstApper = false;

            var html = @"<div>
                        <h2>Lorem Ipsum</h2>
                        <p><strong>Lorem Ipsum</strong> sit amet, consectetur adipiscing elit. Aenean at orci magna. Nulla lacus eros, sagittis eu dignissim et, faucibus vel urna. Vestibulum imperdiet, nunc non efficitur pretium, purus nisi lobortis turpis, at iaculis metus mauris in orci. Pellentesque a rutrum elit. Integer maximus felis id nisi semper interdum eget nec velit. Integer interdum felis ex, ac lacinia quam consequat vitae. Sed vel purus vitae neque maximus sagittis. Nulla facilisi.
                        </p>
                    </div>";
            _ = MyRichText.SetHtml(html);            
        }       
    }

    private async void CopyToolbarItem_Clicked(object sender, EventArgs e)
    {
        var html = await MyRichText.GetHtml();
        await Clipboard.Default.SetTextAsync(html);
        await DisplayAlert(string.Empty, "Copied to clipboard", "Ok");
    }
}
