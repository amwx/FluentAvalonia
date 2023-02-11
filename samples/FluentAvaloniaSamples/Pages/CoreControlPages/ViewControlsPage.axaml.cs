using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FluentAvaloniaSamples.Pages;

public partial class ViewControlsPage : UserControl
{
    public ViewControlsPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
