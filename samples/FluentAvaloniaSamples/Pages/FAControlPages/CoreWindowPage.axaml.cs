using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FluentAvaloniaSamples.Pages;

public partial class CoreWindowPage : FAControlsPageBase
{
    public CoreWindowPage()
    {
        InitializeComponent();

        //TargetType = typeof(CoreWindow);
        Description = "A modern UWP window style for Windows system with a graceful fallback on Mac/Linux";
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
