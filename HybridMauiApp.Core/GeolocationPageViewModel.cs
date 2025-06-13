using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Drawing;

namespace HybridMauiApp.Core;

public partial class GeolocationPageViewModel : ObservableObject
{
    public Func<CrossPosition, Task>? MoveToPosition { get; set; }
    public Func<Task<CrossPosition?>>? RetrieveCenterPosition { get; set; }

    [ObservableProperty]
    private ObservableCollection<EntityViewModel> entities = new ObservableCollection<EntityViewModel>();
    

    [RelayCommand]
    private async Task LoadPins()
    {
        if (MoveToPosition == null)
        {
            await Task.Delay(100);
        }

        if (MoveToPosition != null)
        {
            await MoveToPosition(new CrossPosition(36.6248851300317, 4.340352156036844));
        }

        Entities.Clear();

        List<EntityViewModel> ls =
        [
            new EntityViewModel
            {
                Label = "Igufaf",
                Position = new CrossPosition(36.6248851300317, 4.340352156036844),
                Color = Color.Red,
            },
            new EntityViewModel
            {
                Label = "Ṣṣwameɛ",
                Position = new CrossPosition(36.641667,4.341667),
                Color = Color.Yellow,
            },
            new EntityViewModel
            {
                Label = "Taqa",
                Position = new CrossPosition(36.610801641980046, 4.3250374826073115),
                Color = Color.Green,
            },
            new EntityViewModel
            {
                Label = "Targust",
                Position = new CrossPosition(36.62576185297967, 4.331842104549809),
                Color = Color.Blue,
            },
            new EntityViewModel
            {
                Label = "Iferḥaten",
                Position = new CrossPosition(36.62127430554682, 4.332407409873423),
                Color = Color.Crimson,
            },
            new EntityViewModel
            {
                Label = "At Crif",
                Position = new CrossPosition(36.63324119008672, 4.341437809383102),
                Color = Color.LightGray,
            },
            new EntityViewModel
            {
                Label = "Pin 6",
                Position = new CrossPosition(36.60162498482117, 4.343956546030581),
                Color = Color.Beige,
            },
            new EntityViewModel
            {
                Label = "Pin 6 bis",
                Position = new CrossPosition(36.60162498482117, 4.343956546030581),
                Color = Color.Maroon,
            },
        ];

        foreach(var item in ls)
        {
            Entities.Add(item);
        }
    }

    [RelayCommand]
    private async Task AddPin()
    {
        if (RetrieveCenterPosition == null)
        {
            return;            
        }

        var center = await RetrieveCenterPosition();

        Entities.Add(new EntityViewModel
        {
            Label = $"Pin {Entities.Count}",
            Position = center,
            Color = Color.Red,
        });
    }
}
