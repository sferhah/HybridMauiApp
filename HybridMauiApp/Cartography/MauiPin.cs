namespace HybridMauiApp.Cartography;

public class MauiPin : BindableObject
{
    public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(MauiPosition), typeof(MauiPin), default(MauiPosition));
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(MauiPin));
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(MauiPin));

    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public MauiPosition Position { get => (MauiPosition)GetValue(PositionProperty); set => SetValue(PositionProperty, value); }
    public Color Color { get => (Color)GetValue(ColorProperty); set => SetValue(ColorProperty, value); }
    public object? NativeObject { get; set; }
}
