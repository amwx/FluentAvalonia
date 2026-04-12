using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FAControlsGallery.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;

namespace FAControlsGallery.Pages.DesignPages;

public partial class ColorsPage : ControlsPageBase
{
    public ColorsPage()
    {
        InitializeComponent();
        Classes.Add("design");

        ControlName = "Colors";
        PreviewImage = (FAIconSource)App.Current.FindResource("TextPageIcon");

        Description = "Color provides an intuitive way of communicating information to users in your app. " +
            "It can be used to indicate interactivity, give feedback to user actions and give your interface " +
            "a sense of visual continuity" +
            "This page was adapted from the WinUI 3 Gallery.";

        TabHost.SelectionChanged += TabHostSelectionChanged;
        TabHostSelectionChanged(null, null);
    }

    private void TabHostSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var idx = TabHost.SelectedIndex;

        ColorsPageFrame.Navigate(idx switch
        {
            0 => typeof(TextColorsPage),
            1 => typeof(FillColorsPage),
            2 => typeof(StrokeColorsPage),
            3 => typeof(BackgroundColorsPage),
            4 => typeof(SignalColorsPage),
            _ => throw new ArgumentOutOfRangeException()
        }, null, GetTransitionInfo(_oldIndex, idx));

        _oldIndex = idx;
    }

    private FASlideNavigationTransitionEffect GetEffect(int oldIndex, int index)
    {
        if (oldIndex < 0)
            return FASlideNavigationTransitionEffect.FromBottom;

        if (oldIndex > index)
            return FASlideNavigationTransitionEffect.FromRight;
        else
            return FASlideNavigationTransitionEffect.FromLeft;
    }

    private FANavigationTransitionInfo GetTransitionInfo(int oldIndex, int index)
    {
        if (oldIndex == -1)
        {
            return new FASuppressNavigationTransitionInfo();
        }
        else
        {
            return new FASlideNavigationTransitionInfo
            {
                Effect = GetEffect(oldIndex, index)
            };
        }
    }

    protected override void ToggleThemeButtonClick(object sender, RoutedEventArgs e)
    {
        if (ThemeScopeProvider != null)
        {
            var theme = ThemeScopeProvider.ActualThemeVariant;

            if (theme == ThemeVariant.Light)
            {
                ThemeScopeProvider.RequestedThemeVariant = ThemeVariant.Dark;
            }
            else
            {
                ThemeScopeProvider.RequestedThemeVariant = ThemeVariant.Light;
            }
        }
    }

    private int _oldIndex = -1;
}
