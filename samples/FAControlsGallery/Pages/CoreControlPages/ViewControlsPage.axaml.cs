using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class ViewControlsPage : ControlsPageBase
{
    public ViewControlsPage()
    {
        InitializeComponent();

        ControlName = "View Controls";
        App.Current.Resources.TryGetResource("ViewPageIcon", null, out var icon);
        PreviewImage = (IconSource)icon;

        var CollapsingDisabledExpander = this.Get<Expander>("CollapsingDisabledExpander");
        var ExpandingDisabledExpander = this.Get<Expander>("ExpandingDisabledExpander");

        CollapsingDisabledExpander.Collapsing += (s, e) => { e.Cancel = true; };
        ExpandingDisabledExpander.Expanding += (s, e) => { e.Cancel = true; };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
