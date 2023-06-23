using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using FAControlsGallery.ViewModels;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class IconsPage : ControlsPageBase
{
    public IconsPage()
    {
        InitializeComponent();

        DataContext = new IconElementPageViewModel();

        // Change the target type (for 'Show Definition') based on
        // which tab the page is on
        TabControl1.SelectionChanged += (s, e) =>
        {
            var idx = TabControl1.SelectedIndex;

            if (idx == 2)
            {
                TargetType = null;
                SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            }
            else
            {
                SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
                TargetType = idx switch
                {
                    0 => typeof(FAIconElement),
                    1 => typeof(IconSource),
                    _ => null
                };
            }            
        };
    }
}
