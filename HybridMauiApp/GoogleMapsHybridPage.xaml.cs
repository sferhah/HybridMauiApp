using CommunityToolkit.Maui.Extensions;
using HybridMauiApp.Cartography;
using HybridMauiApp.Converters;
using HybridMauiApp.Core;

namespace HybridMauiApp;

public partial class GoogleMapsHybridPage : ContentPage
{
    public GoogleMapsHybridPage()
    {
        BindingContext = new GeolocationPageViewModel
        {
            MoveToPosition = (Core.CrossPosition position) => this.MyMap.SetCenter(position.ToGoogleMapsPoition()),
            RetrieveCenterPosition = async () => ((await this.MyMap.GetBounds())?.Center)?.FromGoogleMapsPoition()
        };

        InitializeComponent();

        this.MyMap.ItemTemplate = new DataTemplate(() =>
        {
            var pin = new MauiPin();
            pin.SetBinding(MauiPin.PositionProperty, static (EntityViewModel entry) => entry.Position, converter: new PositionConverter());
            pin.SetBinding(MauiPin.LabelProperty, static (EntityViewModel entry) => entry.Label);
            pin.SetBinding(MauiPin.ColorProperty, static (EntityViewModel entry) => entry.Color, converter: new ColorConverter());
            return pin;
        });

        this.MyMap.ClusterClicked += MyMap_ClusterClicked;
        this.MyMap.InfoWindowClicked += MyMap_InfoWindowClicked;
    }

    private async void MyMap_InfoWindowClicked(object? sender, InfoWindowClickedEventArgs e)
    {
        if (e.Pin.BindingContext is not EntityViewModel entityViewModel)
        {
            return;
        }

        await DisplayAlert("InfoWindow", $"'{entityViewModel.Label}' clicked", "Ok");        
    }

    private async void MyMap_ClusterClicked(object? sender, ClusterClickedEventArgs e)
    {
        var clusterAsString = string.Join("\n", e.Pins.Select(p => p.BindingContext).OfType<EntityViewModel>().Select(i => i.Label));
        await DisplayAlert("Cluster", $"{clusterAsString}", "Ok");
    }

    private async void MyPositionButton_Clicked(object sender, EventArgs e)
    {
        var permission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>().ConfigureAwait(true);

        if (permission == PermissionStatus.Granted)
        {
            var position = await Geolocation.GetLastKnownLocationAsync();

            if (position != null) 
            {
                await this.MyMap.SetCenter(new MauiPosition(position.Latitude, position.Longitude));
            }            
        }        
    }
}
