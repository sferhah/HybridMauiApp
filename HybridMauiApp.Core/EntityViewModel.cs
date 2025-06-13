using CommunityToolkit.Mvvm.ComponentModel;
using System.Drawing;

namespace HybridMauiApp.Core;

public class EntityViewModel : ObservableObject
{
    public string? Label { get; set; }
    public CrossPosition? Position { get; set; }
    public Color? Color { get; set; }
}
