using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages;

public partial class NewControlsPage : UserControl
{
    public NewControlsPage()
    {
        InitializeComponent();

        DataContext = new NewControlsPageViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
