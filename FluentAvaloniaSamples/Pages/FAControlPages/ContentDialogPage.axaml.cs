using System;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages;

public partial class ContentDialogPage : FAControlsPageBase
{
    public ContentDialogPage()
    {
        InitializeComponent();

        TargetType = typeof(ContentDialog);
        WinUINamespace = "Microsoft.UI.Xaml.Controls.ContentDialog";
        WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.contentdialog?view=winui-3.0");
        WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/controls/dialogs-and-flyouts/dialogs");
        Description = "Use a ContentDialog to show relevant information or to provide a modal dialog experience that can show any XAML content";


        DataContext = new ContentDialogPageViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
