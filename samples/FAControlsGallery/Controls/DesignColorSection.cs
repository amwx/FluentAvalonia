using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Controls;

public class DesignColorSection : HeaderedContentControl
{
    public static readonly StyledProperty<string> DescriptionProperty =
        SettingsExpander.DescriptionProperty.AddOwner<DesignColorSection>();

    public static readonly DirectProperty<DesignColorSection, IList<ColorTile>> TilesProperty =
        AvaloniaProperty.RegisterDirect<DesignColorSection, IList<ColorTile>>(nameof(Tiles),
            x => x.Tiles);

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IList<ColorTile> Tiles => _tiles ??= new List<ColorTile>();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _exampleThemeScopeProvider = e.NameScope.Get<ThemeVariantScope>("ThemeScopeProvider");
    }

    public void SetExampleTheme()
    {
        var theme = _exampleThemeScopeProvider.ActualThemeVariant;

        if (theme == ThemeVariant.Light)
        {
            _exampleThemeScopeProvider.RequestedThemeVariant = ThemeVariant.Dark;
        }
        else
        {
            _exampleThemeScopeProvider.RequestedThemeVariant = ThemeVariant.Light;
        }
    }

    private ThemeVariantScope _exampleThemeScopeProvider;
    private IList<ColorTile> _tiles;
}
