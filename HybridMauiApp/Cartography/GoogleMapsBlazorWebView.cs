using Microsoft.AspNetCore.Components.WebView.Maui;
using HybridMauiApp.Components.Pages;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Specialized;
using GoogleMapsComponents.Maps;
using System.Globalization;

namespace HybridMauiApp.Cartography;

public partial class GoogleMapsBlazorWebView : BlazorWebView, MyMapComponent.IMyMapComponentListener
{
    private MyMapComponent? _map;
    public void OnAfterMapInit(MyMapComponent? map)
    {
        this._map = map;
        _map_tcs.SetResult(true);
    }

    public GoogleMapsBlazorWebView()
    {
        FlowDirection = FlowDirection.LeftToRight;
        HostPage = "wwwroot/index_googlemaps.html";
        RootComponents.Add(new RootComponent
        {
            Selector = "#app",
            ComponentType = typeof(MyMapComponent),
            Parameters = new Dictionary<string, object?>
            {
                { nameof(MyMapComponent.Listener), this },
                { nameof(MyMapComponent.DefaultCenter), new MauiPosition(48.864716, 2.349014) },
                { nameof(MyMapComponent.DefaultControlPosition), CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? ControlPosition.TopRight : ControlPosition.TopLeft },
                { nameof(MyMapComponent.ClusteringEnabled), true }
            }
        });
        Loaded += GoogleMapsBlazorWebView_Loaded;
        Unloaded += GoogleMapsBlazorWebView_Unloaded;
    }

    private TaskCompletionSource<bool> _map_tcs = new TaskCompletionSource<bool>();
    public async Task<MyMapComponent> GetMapAsync()
    {
        await _map_tcs.Task;
        return _map!;
    }

    private void GoogleMapsBlazorWebView_Unloaded(object? sender, EventArgs e)
    {
        _ = ToggleLocationAsync(false);
    }

    private void GoogleMapsBlazorWebView_Loaded(object? sender, EventArgs e)
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
                    await (await GetMapAsync()).OnNewLocation(position.Latitude, position.Longitude);
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



    private void Geolocation_LocationChanged(object? sender, GeolocationLocationChangedEventArgs e) => Dispatcher.Dispatch(async () => await (await GetMapAsync()).OnNewLocation(e.Location.Latitude, e.Location.Longitude));

    public event EventHandler<InfoWindowClickedEventArgs>? InfoWindowClicked;
    public event EventHandler<ClusterClickedEventArgs>? ClusterClicked;
    public event EventHandler<MapClickedEventArgs>? MapClicked;

    public void OnInfoWindowClicked(MauiPin pin) => InfoWindowClicked?.Invoke(this, new InfoWindowClickedEventArgs(pin));
    public void OnClusterClick(ICollection<MauiPin> selected_pins) => ClusterClicked?.Invoke(this, new ClusterClickedEventArgs(selected_pins));
    public void OnMapClicked(MauiPosition point) => MapClicked?.Invoke(this, new MapClickedEventArgs(point));
    public async Task ToggleMapType() => await (await GetMapAsync()).ToggleMapType();
    public async Task SetCenter(MauiPosition position) => await (await GetMapAsync()).SetCenter(position);
    public async Task FitBounds() => await (await GetMapAsync()).FitBounds();
    public async Task<MapRegion?> GetBounds() => await (await GetMapAsync()).GetBounds();


    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(GoogleMapsBlazorWebView), default(IEnumerable),
    propertyChanged: (b, o, n) => ((GoogleMapsBlazorWebView)b).OnItemsSourcePropertyChanged((IEnumerable)o, (IEnumerable)n));
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
                await RemovePins(e.OldItems?.Cast<object>() ?? []);
                await CreatePins(e.NewItems?.Cast<object>() ?? []);
                break;
            case NotifyCollectionChangedAction.Reset:
                await ClearPins();
                break;
        }
    }

    
    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(GoogleMapsBlazorWebView), default(DataTemplate),
    propertyChanged: (b, o, n) => ((GoogleMapsBlazorWebView)b).OnItemTemplatePropertyChanged((DataTemplate)o, (DataTemplate)n));

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

        var newPins = new List<MauiPin>();

        foreach (var newItem in newItems)
        {
            var pin = (MauiPin)ItemTemplate.CreateContent();
            pin.BindingContext = newItem;
            _pins.Add(pin);
            newPins.Add(pin);
        }

        if (oldPins.Count > 0 || newPins.Count > 0)
        {
            await (await GetMapAsync()).ReplacePins(oldPins, newPins);
        }
    }
}