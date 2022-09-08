using System.Collections.Generic;
using FluentAvalonia.UI.Controls;

namespace FluentAvaloniaSamples.ViewModels;

public class MenuFlyoutPageViewModel : ViewModelBase
{
    public MenuFlyoutPageViewModel()
    {
        TestMenuItems = new List<TempMenuItemBase>
        {
            new TempMenuItem { Text = "Item 1", Icon = Symbol.Cut },
            new TempMenuItem { Text = "Item 2", Icon = Symbol.Copy },
            new TempMenuItem { Text = "Item 3", Icon = Symbol.Paste },
            new TempMenuSeparator(),
            new TempSubItem
            {
                Text = "Cascading Menu",
                SubItems = new List<TempMenuItemBase>
                {
                    new TempMenuItem { Text = "Sub Item 1", Icon = Symbol.Globe },
                    new TempMenuItem { Text = "Sub Item 2", Icon = Symbol.Games },
                    new TempMenuItem { Text = "Sub Item 3", Icon = Symbol.Mail },
                }
            },
            new TempMenuSeparator(),
            new TempToggleMenuItem {Text = "Toggle 1"},
            new TempToggleMenuItem {Text = "Toggle 1"},
            new TempMenuSeparator(),
            new TempRadioMenuItem { Text = "Radio1 Group 1", GroupName = "Group1", IsChecked = true},
            new TempRadioMenuItem { Text = "Radio2 Group 1", GroupName = "Group1"},
            new TempMenuSeparator(),
            new TempRadioMenuItem { Text = "Radio1 Group 2", GroupName = "Group2"},
            new TempRadioMenuItem { Text = "Radio2 Group 2", GroupName = "Group2", IsChecked = true},
        };
    }

    public List<TempMenuItemBase> TestMenuItems { get; set; }
}

public abstract class TempMenuItemBase : ViewModelBase
{ }

public class TempMenuItem : TempMenuItemBase
{
    public string Text { get; set; }

    public Symbol Icon { get; set; }
}

public class TempMenuSeparator : TempMenuItemBase { }

public class TempToggleMenuItem : TempMenuItemBase
{
    public string Text { get; set; }

    public bool IsChecked
    {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }

    private bool _isChecked;
}

public class TempRadioMenuItem : TempMenuItemBase
{
    public string Text { get; set; }

    public bool IsChecked
    {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }

    public string GroupName { get; set; }

    private bool _isChecked;
}

public class TempSubItem : TempMenuItemBase
{
    public string Text { get; set; }
    public List<TempMenuItemBase> SubItems { get; set; }
}
