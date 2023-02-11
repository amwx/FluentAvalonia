using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FluentAvaloniaSamples.Pages;

public partial class TabViewWindowSampleContent : UserControl
{
    public TabViewWindowSampleContent()
    {
        InitializeComponent();
    }

    public TabViewWindowSampleContent(string header)
        : this()
    {
        this.FindControl<TextBlock>("HeaderContent").Text = header;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
