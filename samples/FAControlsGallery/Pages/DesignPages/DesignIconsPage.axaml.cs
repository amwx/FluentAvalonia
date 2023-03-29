using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using FAControlsGallery.ViewModels.DesignPages;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages.DesignPages;

public partial class DesignIconsPage : ControlsPageBase
{
    public DesignIconsPage()
    {
        InitializeComponent();

        DataContext = new DesignIconsPageViewModel();
        ControlName = "Icons";
        PreviewImage = (IconSource)App.Current.FindResource("TextPageIcon");

        Description = "FluentAvalonia ships with an embedded symbol font based on the FluentUI System Icons project. " +
            "The icons have been remapped and standardized to match the unicode codepoint found in the Segoe Fluent Icons " +
            "and Segoe MDL2 Assets font. As a result, on Windows, you can change the resource SymbolThemeFontFamily to " +
            "one of the Windows specific fonts without breaking anything.";

        Description1.Text = "* Note that not all icons featured in the Segoe families are featured here.";
        Description2.Text = "* Note that SymbolIcon/SymbolIconSource will always use this embedded font, " +
            "regardless of what SymbolThemeFontFamily is set to.";

        IconPreview.Tapped += IconPreviewTapped;
        IconPreview.ElementPrepared += IconPreviewElementPrepared;
        IconPreview.ElementClearing += IconPreviewElementClearing;
    }

    private void IconPreviewElementPrepared(object sender, ItemsRepeaterElementPreparedEventArgs e)
    {
        if (e.Element is ListBoxItem lbi && lbi == _selectedItem)
        {
            lbi.IsSelected = true;
        }
    }

    private void IconPreviewElementClearing(object sender, ItemsRepeaterElementClearingEventArgs e)
    {
        if (e.Element is ListBoxItem lbi && lbi.IsSelected)
        {
            lbi.IsSelected = false;
        }
    }


    private void IconPreviewTapped(object sender, TappedEventArgs e)
    {
        var item = ((Visual)e.Source).FindAncestorOfType<ListBoxItem>(true);

        if (item != null)
        {
            if (_selectedItem != null && _selectedItem != item)
            {
                _selectedItem.IsSelected = false;
            }

            _selectedItem = item;
            item.IsSelected = true;

            IconInfoTip.Target = item;
            IconInfoTip.Content = item.Content;
            IconInfoTip.IsOpen = true;
        }
    }

    private ListBoxItem _selectedItem;
}
