using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using GoogleMapsComponents;
using HybridMauiApp.Cartography;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;


namespace HybridMauiApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
        .UseMauiCommunityToolkit()
        .UseMauiCommunityToolkitMarkup();

#if DEBUG
		builder.Logging.AddDebug();
#endif

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddHybridWebViewDeveloperTools();
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif
        builder.Services.AddBlazorGoogleMaps(new MyBlazorGoogleMapsKeyService());


        builder.ConfigureLifecycleEvents(events =>
        {
#if WINDOWS
            events.AddWindows(wndLifeCycleBuilder =>
            {
                wndLifeCycleBuilder.OnWindowCreated(window =>
                {
                    // WinUIEx.WindowExtensions.CenterOnScreen(window, 1024, 768); //Set size and center on screen using WinUIEx extension method                        

                    var manager = WinUIEx.WindowManager.Get(window);
                    manager.PersistenceId = "MainWindowPersistanceId"; // Remember window position and size across runs
                    manager.MinWidth = 640;
                    manager.MinHeight = 480;
                });
            });
#endif
        });

        return builder.Build();
    }
}
