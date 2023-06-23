using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class CommandBarPage : ControlsPageBase
{
    public CommandBarPage()
    {
        InitializeComponent();

        DataContext = this;
        TargetType = typeof(CommandBar);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

}
