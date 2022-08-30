using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvaloniaSamples.Pages.NVSamplePages;

namespace FluentAvaloniaSamples.Pages;

public partial class FramePage : FAControlsPageBase
{
    public FramePage()
    {
        InitializeComponent();

        TargetType = typeof(Frame);
        WinUINamespace = "Microsoft.UI.Xaml.Controls.Frame";
        WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.frame?view=winui-3.0");
        Description = "Displays Page (UserControl) instances, supports navigation to new pages, and maintains a navigation history to support forward and backward navigation.";

        var lb = this.FindControl<ListBox>("PageSelection");
        PageOptions = new List<string>
        {
            "Sample Page 1\n(Default Animation)",
            "Sample Page 2\n(Slide Animation)",
            "Sample Page 3\n(DrillIn Animation)",
            "Sample Page 4\n(No Animation)"
        };
        lb.SelectionChanged += OnListBoxSelectionChanged;

        _frame = this.FindControl<Frame>("TestFrame");
        _frame.Navigate(typeof(NVSamplePage6), null, new SuppressNavigationTransitionInfo());

        DataContext = this;
    }

    public List<string> PageOptions { get; }

    private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        switch ((sender as ListBox).SelectedIndex)
        {
            case 0:
                _frame.Navigate(typeof(NVSamplePage1));
                break;

            case 1:
                _frame.Navigate(typeof(NVSamplePage2), null, new SlideNavigationTransitionInfo());
                break;

            case 2:
                _frame.Navigate(typeof(NVSamplePage3), null, new DrillInNavigationTransitionInfo());
                break;

            case 3:
                _frame.Navigate(typeof(NVSamplePage4), null, new SuppressNavigationTransitionInfo());
                break;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private Frame _frame;
}
