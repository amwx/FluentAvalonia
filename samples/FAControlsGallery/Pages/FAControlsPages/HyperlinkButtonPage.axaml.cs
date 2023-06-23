using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class HyperlinkButtonPage : ControlsPageBase
{
    public HyperlinkButtonPage()
    {
        InitializeComponent();

        TargetType = typeof(HyperlinkButton);        
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void HyperlinkButton_Click(object sender, RoutedEventArgs args)
    {
        //NavigationService.Instance.Navigate(typeof(SettingsPage));
    }
}
