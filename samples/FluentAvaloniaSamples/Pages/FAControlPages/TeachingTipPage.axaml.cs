using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace FluentAvaloniaSamples.Pages;

public partial class TeachingTipPage : FAControlsPageBase
{
    public TeachingTipPage()
    {
        InitializeComponent();

        TargetType = typeof(TeachingTip);
        WinUINamespace = "Microsoft.UI.Xaml.Controls.TeachingTip";
        WinUIDocsLink = new Uri("https://learn.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.teachingtip?view=winui-2.8&viewFallbackFrom=winui-3.0");
        WinUIGuidelinesLink = new Uri("https://learn.microsoft.com/en-us/windows/apps/design/controls/dialogs-and-flyouts/teaching-tip");
        Description = "The XAML TeachingTip control provides a way for your app to guide and inform users in your application with a non-invasive and rich notification. " +
            "TeachingTip can be used for brining focus to a new or important feature, teaching users how to perform a task, or enhancing the user workflow by providing" +
            "contextually relevant information to their task at hand";
        
        DataContext = this;

        var b = this.FindControl<Button>("ShowTipButton");
        b.Click += (s, e) =>
        {
            var tip = this.FindControl<TeachingTip>("TeachingTip");
            tip.IsOpen = !tip.IsOpen;
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
