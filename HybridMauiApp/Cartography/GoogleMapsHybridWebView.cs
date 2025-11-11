using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Collections;
using System.Text.Json.Serialization.Metadata;

namespace HybridMauiApp.Cartography;

public partial class GoogleMapsHybridWebView : HybridWebView
{
    public GoogleMapsHybridWebView()
    {
        FlowDirection = FlowDirection.LeftToRight;
        this.HybridRoot = "wwwroot/googlemaps";
        this.DefaultFile = "index.html";
        this.SetInvokeJavaScriptTarget(new HybridGoogleMapsJSInvoke(this));


        Loaded += HybridGoogleMaps_Loaded;
        Unloaded += HybridGoogleMaps_Unloaded;
    }

    private void HybridGoogleMaps_Unloaded(object? sender, EventArgs e)
    {
        _ = ToggleLocationAsync(false);        
    }

    private void HybridGoogleMaps_Loaded(object? sender, EventArgs e)
    {
        _ = ToggleLocationAsync(true);
    }

    public async Task ToggleLocationAsync(bool toggleOn)
    {
        if (toggleOn)
        {
            var permission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>().ConfigureAwait(true);      

            if (permission == PermissionStatus.Granted)
            {
                var position = await Geolocation.GetLastKnownLocationAsync()
                        ?? await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Default, TimeSpan.FromSeconds(20)));

                if (position != null)
                {
                    await OnNewLocation(position.Latitude, position.Longitude);
                }

                while (Geolocation.IsListeningForeground)
                {
                    await Task.Delay(300);
                }

                if (await Geolocation.StartListeningForegroundAsync(new GeolocationListeningRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5))))
                {
                    Geolocation.LocationChanged += Geolocation_LocationChanged;
                }
            }
        }
        else
        {
            Geolocation.LocationChanged -= Geolocation_LocationChanged;
            Geolocation.StopListeningForeground();
        }
    }


    private void Geolocation_LocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
    {
        Dispatcher.Dispatch(async () =>
        {
            await OnNewLocation(e.Location.Latitude, e.Location.Longitude);
        });
    }

    public async Task<MapRegion?> GetBounds()
    {
        var result = await this.InvokeJavaScriptAsync("getBounds", GoogleMapsInvokeJsContext.Default.MapRegion);
        return result;
    }

    public Task SetCenter(MauiPosition position) 
        => this.InvokeJavaScriptAsync("setCenter", GoogleMapsInvokeJsContext.Default.Double, [position.Latitude, position.Longitude], [GoogleMapsInvokeJsContext.Default.Double, GoogleMapsInvokeJsContext.Default.Double]);

    public Task OnNewLocation(double lat, double lng) 
        => this.InvokeJavaScriptAsync("onNewLocation", GoogleMapsInvokeJsContext.Default.Double, [lat, lng], [GoogleMapsInvokeJsContext.Default.Double, GoogleMapsInvokeJsContext.Default.Double]);

    public new async Task<TReturnType?> InvokeJavaScriptAsync<TReturnType>(
            string methodName,
            JsonTypeInfo<TReturnType> returnTypeJsonTypeInfo,
            object?[]? paramValues = null,
            JsonTypeInfo?[]? paramJsonTypeInfos = null)
    {
        await WaitForMapToBeReadyAsync();
        return await base.InvokeJavaScriptAsync(methodName, returnTypeJsonTypeInfo, paramValues, paramJsonTypeInfos);
    }

    public event EventHandler<InfoWindowClickedEventArgs>? InfoWindowClicked;
    public event EventHandler<ClusterClickedEventArgs>? ClusterClicked;
    public event EventHandler<MapClickedEventArgs>? MapClicked;


    private TaskCompletionSource<bool> _map_tcs = new TaskCompletionSource<bool>();
    public Task WaitForMapToBeReadyAsync() => _map_tcs.Task;

    public class HybridGoogleMapsJSInvoke
    {
        readonly GoogleMapsHybridWebView _hybridGoogleMaps;
        IDispatcher Dispatcher => _hybridGoogleMaps.Dispatcher;

        public HybridGoogleMapsJSInvoke(GoogleMapsHybridWebView hybridRichTextEditor)
            => _hybridGoogleMaps = hybridRichTextEditor;

        public async Task<HybridGoogleMapsConfiguration> GetGoogleMapsConfiguration() => new HybridGoogleMapsConfiguration 
        {
            Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
            MapControlPosition = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? "RIGHT_TOP" : "TOP_LEFT",
            ApiKey = await GoogleApiKeyService.GetGoogleApiKey(),
            Translations = new Dictionary<string, string>
            {
                { "GoogleMapsClusterLibraryLoadError", "Failed to load Google Maps Cluster library. Please try again later."},
                { "GoogleMapsMarkerLibraryLoadError", "Failed to load Google Maps Marker library after multiple attempts. Please try again later." },
                { "MapLoadFailure", "Unable to load the map. Please try again later." },
                { "Refresh", "Refresh" },
            },
            Center = new MauiPosition
            {
                Latitude = 48.864716,                
                Longitude = 2.349014,
            }
        };

        public void OnMapReady()
        {
            _hybridGoogleMaps._map_tcs.TrySetResult(true);
        }

        public void OnInfoWindowClicked(Guid guid)
        {
            var pin = _hybridGoogleMaps._pins.FirstOrDefault(x => (x.NativeObject as JsPin)?.Guid == guid);

            if (pin != null)
            {
                Dispatcher.Dispatch(() => _hybridGoogleMaps.InfoWindowClicked?.Invoke(this, new InfoWindowClickedEventArgs(pin)));
            }
        }

        public void OnClusterClicked(params Guid[] guids)
        {
            var pins = _hybridGoogleMaps._pins.Where(x => guids.Cast<Guid?>().Contains((x.NativeObject as JsPin)?.Guid)).ToList();
            Dispatcher.Dispatch(() => _hybridGoogleMaps.ClusterClicked?.Invoke(this, new ClusterClickedEventArgs(pins)));
        }

        public void OnMapClicked(double latitude, double longitude)
           => Dispatcher.Dispatch(() => _hybridGoogleMaps.MapClicked?.Invoke(this, new MapClickedEventArgs(new MauiPosition(latitude, longitude))));
    }

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(GoogleMapsHybridWebView), default(IEnumerable),
   propertyChanged: (b, o, n) => ((GoogleMapsHybridWebView)b).OnItemsSourcePropertyChanged((IEnumerable)o, (IEnumerable)n));
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    void OnItemsSourcePropertyChanged(IEnumerable oldItemsSource, IEnumerable newItemsSource)
    {
        if (oldItemsSource is INotifyCollectionChanged ncc)
        {
            ncc.CollectionChanged -= OnItemsSourceCollectionChanged;
        }

        if (newItemsSource is INotifyCollectionChanged ncc1)
        {
            ncc1.CollectionChanged += OnItemsSourceCollectionChanged;
        }

        _ = ReplacePinItems();
    }

    async void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewStartingIndex == -1)
                    goto case NotifyCollectionChangedAction.Reset;
                await CreatePins(e.NewItems?.Cast<object>() ?? []);
                break;
            case NotifyCollectionChangedAction.Move:
                if (e.OldStartingIndex == -1 || e.NewStartingIndex == -1)
                    goto case NotifyCollectionChangedAction.Reset;
                // Not tracking order
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldStartingIndex == -1)
                    goto case NotifyCollectionChangedAction.Reset;
                await RemovePins(e.OldItems?.Cast<object>() ?? []);
                break;
            case NotifyCollectionChangedAction.Replace:
                if (e.OldStartingIndex == -1)
                    goto case NotifyCollectionChangedAction.Reset;
                await ReplacePins(e.OldItems?.Cast<object>() ?? [], e.NewItems?.Cast<object>() ?? []);
                break;
            case NotifyCollectionChangedAction.Reset:
                await ClearPins();
                break;
        }
    }


    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(GoogleMapsHybridWebView), default(DataTemplate),
    propertyChanged: (b, o, n) => ((GoogleMapsHybridWebView)b).OnItemTemplatePropertyChanged((DataTemplate)o, (DataTemplate)n));

    private TaskCompletionSource<bool> _template_tcs = new TaskCompletionSource<bool>();
    void OnItemTemplatePropertyChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        if (newItemTemplate != null)
        {
            _template_tcs.TrySetResult(true);
        }
        else
        {
            _template_tcs.SetCanceled();
            _template_tcs = new TaskCompletionSource<bool>();
        }

        _ = ReplacePinItems();
    }

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }


    readonly ObservableCollection<MauiPin> _pins = new ObservableCollection<MauiPin>();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    async Task ReplacePinItems()
    {
        await _semaphore.WaitAsync();
        try
        {
            await ReplacePins(_pins.Select(item => item.BindingContext).ToList(), ItemsSource?.Cast<object>() ?? []);
        }
        finally
        {
            _semaphore.Release();
        }
    }


    Task ClearPins() => RemovePins(_pins.Select(item => item.BindingContext).ToList());
    Task CreatePins(IEnumerable<object> newItems) => ReplacePins([], newItems);
    Task RemovePins(IEnumerable<object> items) => ReplacePins(items, []);


    async Task ReplacePins(IEnumerable<object> oldItems, IEnumerable<object> newItems)
    {
        await _template_tcs.Task;

        var oldPins = new List<MauiPin>();

        foreach (var itemToRemove in oldItems)
        {
            var pinToRemove = this._pins.FirstOrDefault(pin => pin.BindingContext?.Equals(itemToRemove) == true);
            if (pinToRemove != null)
            {
                this._pins.Remove(pinToRemove);
                oldPins.Add(pinToRemove);
            }
        }

        var oldArray = oldPins.Select(x => x.NativeObject).OfType<JsPin>().ToArray();

        var newPins = new List<MauiPin>();

        foreach (var newItem in newItems)
        {
            var pin = (MauiPin)ItemTemplate.CreateContent();
            pin.BindingContext = newItem;
            _pins.Add(pin);
            newPins.Add(pin);
        }

        var newArray = newPins.Select(pin => pin.NativeObject = ToJsPin(pin)).OfType<JsPin>().ToArray();

        if (oldArray.Length > 0 || newArray.Length > 0)
        {
            await this.InvokeJavaScriptAsync("replacePins", GoogleMapsInvokeJsContext.Default.Double, [oldArray, newArray], [GoogleMapsInvokeJsContext.Default.JsPinArray, GoogleMapsInvokeJsContext.Default.JsPinArray]);
        }
    }

    static JsPin ToJsPin(MauiPin pin)
    {
        var color = pin.Color ?? Colors.Red;

        return new JsPin
        {
            Label = WebUtility.HtmlEncode(pin.Label ?? "")
        .Replace("\r\n", "\r")
        .Replace("\n", "\r")
        .Replace("\r", "<br>")
        .Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;")
        .Replace("  ", " &nbsp;"),
            Position = new MauiPosition { Latitude = pin.Position.Latitude, Longitude = pin.Position.Longitude },
            BorderColor = color.WithLuminosity(color.GetLuminosity() * 0.8f).ToRgbaHex(),
            Background = color.ToRgbaHex(),
            GlyphColor = color.WithLuminosity(color.GetLuminosity() * 0.6f).ToRgbaHex()
        };
    }
}

public class JsPin
{
    public string? Label { get; set; }
    public MauiPosition Position { get; set; }
    public string? Background { get; set; }
    public string? BorderColor { get; set; }
    public string? Glyph { get; set; }
    public string? GlyphColor { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
}
