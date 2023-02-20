using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;

public partial class InfoBarPage : FAControlsPageBase
{
    public InfoBarPage()
    {
        InitializeComponent();

        DataContext = new InfoBarPageViewModel(this);
        TargetType = typeof(InfoBar);

        _apiInActionBar = this.FindControl<InfoBar>("Bar4");

        var cb = this.Find<ComboBox>("Bar4IconType");
        cb.SelectionChanged += (s, e) =>
        {
            if (s is Avalonia.Controls.ComboBox c)
            {
                if (c.SelectedIndex == 0)
                {
                    _apiInActionBar.IconSource = null;
                }
                else
                {
                    _apiInActionBar.IconSource = new SymbolIconSource { Symbol = Symbol.MapPin };
                }
            }
        };

        var cb2 = this.Find<ComboBox>("Bar4ButtonType");
        cb2.SelectionChanged += (s, e) =>
        {
            if (s is Avalonia.Controls.ComboBox c)
            {
                if (c.SelectedIndex == 0)
                {
                    _apiInActionBar.ActionButton = null;
                }
                else if (c.SelectedIndex == 1)
                {
                    _apiInActionBar.ActionButton = new Button { Content = "Button" };
                }
                else if (c.SelectedIndex == 2)
                {
                    _apiInActionBar.ActionButton = new HyperlinkButton { Content = "Hyperlink" };
                }
                else if (c.SelectedIndex == 3)
                {
                    _apiInActionBar.ActionButton = new Avalonia.Controls.Button
                    { Content = "Button", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right };
                }
                else if (c.SelectedIndex == 4)
                {
                    _apiInActionBar.ActionButton = new HyperlinkButton
                    { Content = "Hyperlink", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right };
                }
            }
        };
    }

    internal void UpdateBar2ActionButton(int type)
    {
        // 0 = none, 1 = Button, 2 = Hyperlink

        var ib = this.FindControl<InfoBar>("Bar2");
        if (ib != null)
        {
            if (type == 0)
            {
                ib.ActionButton = null;
            }
            else if (type == 1)
            {
                ib.ActionButton = new Avalonia.Controls.Button { Content = "Action" };
            }
            else if (type == 2)
            {
                ib.ActionButton = new HyperlinkButton { Content = "Informational Link" };
            }
        }
    }

    bool hasCustomBG = false;
    public void SetCustomBackground()
    {
        if (hasCustomBG)
            _apiInActionBar.Background = Brushes.Transparent;
        else
            _apiInActionBar.Background = Brushes.Purple;

        hasCustomBG = !hasCustomBG;
    }

    public void SetForeground()
    {
        _apiInActionBar.Foreground = Brushes.Red;
    }

    private InfoBar _apiInActionBar;
}
