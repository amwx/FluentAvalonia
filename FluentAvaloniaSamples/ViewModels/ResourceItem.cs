using Avalonia.Media;

namespace FluentAvaloniaSamples.ViewModels;

public abstract class ResourceItemBase : ViewModelBase
{
    public string ResourceKey { get; set; }

    public abstract string ResourceType { get; }
}

public class ColorResourceItem : ResourceItemBase
{
    public Color Color
    {
        get => _color;
        set
        {
            RaiseAndSetIfChanged(ref _color, value);

            RaisePropertyChanged(nameof(ColorCode));
        }
    }

    public string ColorCode => Color.ToString();

    public override string ResourceType => typeof(Color).Name;

    private Color _color;
}

public class BrushResourceItem : ResourceItemBase
{
    public IBrush Brush
    {
        get => _brush;
        set
        {
            RaiseAndSetIfChanged(ref _brush, value);
            RaisePropertyChanged(nameof(ColorCode));
        }
    }

    public string ColorCode => Brush is ISolidColorBrush solid ? solid.Color.ToString() : string.Empty;

    public override string ResourceType => _brush.GetType().Name;

    private IBrush _brush;
}

public class FontFamilyResourceItem : ResourceItemBase
{
    public FontFamily FontFamily
    {
        get => _font;
        set => RaiseAndSetIfChanged(ref _font, value);
    }

    public override string ResourceType => typeof(FontFamily).Name;

    private FontFamily _font;
}

public class PrimitiveResourceItem : ResourceItemBase
{
    public object Value
    {
        get => _value;
        set => RaiseAndSetIfChanged(ref _value, value);
    }

    public override string ResourceType => _value.GetType().Name;

    private object _value;
}
