using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace FAControlsGallery.Controls;

public sealed class PageHeaderControl : TemplatedControl
{
    public PageHeaderControl()
    {

    }

    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<PageHeaderControl, string>(nameof(Header));

    public static readonly StyledProperty<string> SubTitleProperty =
        AvaloniaProperty.Register<PageHeaderControl, string>(nameof(SubTitle));
      
    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string SubTitle
    {
        get => GetValue(SubTitleProperty);
        set => SetValue(SubTitleProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SubTitleProperty)
        {
            PseudoClasses.Set(":subtitle", change.GetNewValue<string>() != null);
        }
    }
}
