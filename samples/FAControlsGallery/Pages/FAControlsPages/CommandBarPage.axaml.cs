using Avalonia.Markup.Xaml;

namespace FAControlsGallery.Pages;

public partial class CommandBarPage : FAControlsPageBase
{
    public CommandBarPage()
    {
        InitializeComponent();

        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

}
