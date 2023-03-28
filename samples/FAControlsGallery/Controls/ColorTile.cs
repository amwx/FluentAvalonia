using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Controls;

public class ColorTile : TemplatedControl
{
    public static readonly StyledProperty<string> ColorNameProperty =
        AvaloniaProperty.Register<ColorTile, string>(nameof(ColorName));

    public static readonly StyledProperty<string> DescriptionProperty =
        SettingsExpander.DescriptionProperty.AddOwner<ColorTile>();

    public static readonly StyledProperty<string> WarningProperty =
        AvaloniaProperty.Register<ColorTile, string>(nameof(Warning));

    public static readonly StyledProperty<string> ColorBrushNameProperty =
       AvaloniaProperty.Register<ColorTile, string>(nameof(ColorBrushName));

    public static readonly StyledProperty<string> ColorValueProperty =
        AvaloniaProperty.Register<ColorTile, string>("ColorValue");

    public static readonly StyledProperty<bool> ShowSeparatorProperty =
        AvaloniaProperty.Register<ColorTile, bool>(nameof(ShowSeparator));

    public string ColorName
    {
        get => GetValue(ColorNameProperty);
        set => SetValue(ColorNameProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string Warning
    {
        get => GetValue(WarningProperty);
        set => SetValue(WarningProperty, value);
    }

    public string ColorBrushName
    {
        get => GetValue(ColorBrushNameProperty);
        set => SetValue(ColorBrushNameProperty, value);
    }

    public string ColorValue
    {
        get => GetValue(ColorValueProperty);
    }

    public bool ShowSeparator
    {
        get => GetValue(ShowSeparatorProperty);
        set => SetValue(ShowSeparatorProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WarningProperty)
        {
            PseudoClasses.Set(":warning", change.NewValue != null);
        }
        else if (change.Property == BackgroundProperty)
        {
            var val = change.NewValue;

            if (val is ILinearGradientBrush lgb)
            {
                SetValue(ColorValueProperty, "Linear Gradient");
            }
            else if (val is ISolidColorBrush scb)
            {
                uint rgb = scb.Color.ToUint32();                
                SetValue(ColorValueProperty, FormattableString.Invariant($"#{rgb:x8}").ToUpper());
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_copyButton != null)
            _copyButton.Click -= CopyButtonClick;

        _copyButton = e.NameScope.Get<Button>("CopyButton");
        _copyButton.Click += CopyButtonClick;
    }

    private async void CopyButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await Application.Current.Clipboard.SetTextAsync(ColorBrushName);
        }
        catch
        {

        }
    }

    private Button _copyButton;
}
