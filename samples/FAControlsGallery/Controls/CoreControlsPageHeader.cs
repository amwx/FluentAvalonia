using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Controls;

public class CoreControlsPageHeader : ContentControl
{
    public static readonly StyledProperty<IconSource> IconSourceProperty =
        SettingsExpander.IconSourceProperty.AddOwner<CoreControlsPageHeader>();

    public IconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_toggleThemeButton != null)
            _toggleThemeButton.Click -= OnToggleThemeClick;

        base.OnApplyTemplate(e);

        _toggleThemeButton = e.NameScope.Find<Button>("ToggleThemeButton");
        _toggleThemeButton.Click += OnToggleThemeClick;
    }

    private void OnToggleThemeClick(object sender, RoutedEventArgs e)
    {
        var parent = this.FindAncestorOfType<UserControl>();
        if (parent != null)
        {
            var examples = parent.GetVisualDescendants().OfType<ControlExample>();

            foreach (var item in examples)
            {
                item.SetExampleTheme();
            }
        }
    }

    private Button _toggleThemeButton;
}
