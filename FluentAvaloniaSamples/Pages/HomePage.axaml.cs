using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvaloniaSamples.Controls;
using FluentAvaloniaSamples.Services;

namespace FluentAvaloniaSamples.Pages;

public class HomePage : UserControl
{
    public HomePage()
    {
        this.InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AddHandler(OptionsDisplayItem.NavigationRequestedEvent, OnDisplayItemNavigationRequested);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        RemoveHandler(OptionsDisplayItem.NavigationRequestedEvent, OnDisplayItemNavigationRequested);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnDisplayItemNavigationRequested(object sender, RoutedEventArgs e)
    {
        if (e.Source is OptionsDisplayItem odi)
        {
            if (odi.Name == "GettingStartedItem")
            {
                NavigationService.Instance.Navigate(typeof(GettingStartedPage));
            }
            else if (odi.Name == "WhatsNewItem")
            {
                NavigationService.Instance.Navigate(typeof(WhatsNewPage));
            }
        }
    }
}
