using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using System;
using System.Collections.Generic;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// CommandBar used for a <see cref="CommandBarFlyout"/>
/// </summary>
/// <remarks>
/// This class should be treated as internal to FluentAvalonia and not used outside of 
/// the CommandBarFlyout implementations.
/// </remarks>
public class CommandBarFlyoutCommandBar : CommandBar, IStyleable
{
    Type IStyleable.StyleKey => typeof(CommandBarFlyoutCommandBar);

    // As said in the Template, this is a modified version of whats in WinUI b/c the WinUI version
    // is stupid. They have two popups that are blended to make this control - one for the flyout,
    // and one with the CommandBar. Instead of doing that, which would just be a giant headache,
    // it's combined into one Popup (the CommandBar Popup is removed and its all contained in one, 
    // and we just show/hide the overflow as necessary)
    // I genuinely think this is because some legacy aspect of the AppBar requires a popup to be
    // present, b/c I cannot think of any reason for this design. Anyway, things are different, but
    // the end result behavior should still be the same (or very close to it)
    // One drawback, is we always open down, at least for now.

    public CommandBarFlyoutCommandBar()
    {
        // Yes, all this is done in the ctor in WinUI

        // Treated as Loaded Event
        AttachedToVisualTree += (_, __) =>
        {
            //UpdateUI(!_commandBarFlyoutIsOpening);

            // This ensures that even in Transient ShowMode, focus is still directed into the Flyout, which technically
            // goes against the description of Transient mode, but it's what WinUI does, so whatever
            // Logic is modified...
            var commands = PrimaryCommands.Count > 0 ? PrimaryCommands : (SecondaryCommands.Count > 0 ? SecondaryCommands : null);

            if (commands != null)
            {
                // post this to the dispatcher so it's delayed, otherwise we'll take focus before we actually open
                // In case of TextCommandBarFlyout, this will end up clearing the Textbox selection because the 
                // flyout isn't open yet, but we pulled focus
                Dispatcher.UIThread.Post(() =>
                {
                    if (PrimaryCommands.Count > 0)
                    {
                        bool handled = false;
                        for (int i = 0; i < PrimaryCommands.Count; i++)
                        {
                            if (IsControlFocusable(PrimaryCommands[i] as IControl, false))
                            {
                                FocusManager.Instance.Focus(PrimaryCommands[i] as IInputElement, NavigationMethod.Unspecified);
                                handled = true;
                                break;
                            }
                        }

                        if (!handled)
                        {
                            if (_moreButton != null && _moreButton.IsVisible)
                            {
                                FocusManager.Instance?.Focus(_moreButton, NavigationMethod.Unspecified);
                            }
                        }
                    }
                    else
                    {
                        if (_moreButton != null && _moreButton.IsVisible)
                        {
                            FocusManager.Instance?.Focus(_moreButton, NavigationMethod.Unspecified);
                        }
                    }

                }, DispatcherPriority.Loaded);
            }
        };

        Closing += (_, __) =>
        {
            if (_owningFlyout != null)
            {
                if (_owningFlyout.AlwaysExpanded)
                {
                    // Don't close the secondary commands list when the flyout is AlwaysExpanded
                    IsOpen = true;
                }
            }
        };

        PrimaryCommands.CollectionChanged += (_, __) =>
        {
            PopulateAccessibleControls();
        };

        SecondaryCommands.CollectionChanged += (_, __) =>
        {
            PopulateAccessibleControls();
        };
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _moreButton = e.NameScope.Find<Button>("MoreButton");

        PopulateAccessibleControls();
    }

    private void PopulateAccessibleControls()
    {
        if (_horizontallyAccessibleControls == null)
        {
            _horizontallyAccessibleControls = new List<IControl>();
            _verticallyAccessibleControls = new List<IControl>();
        }
        else
        {
            _horizontallyAccessibleControls.Clear();
            _verticallyAccessibleControls.Clear();
        }

        for (int i = 0; i < PrimaryCommands.Count; i++)
        {
            if (PrimaryCommands[i] is IControl c)
            {
                _horizontallyAccessibleControls.Add(c);
                _verticallyAccessibleControls.Add(c);
            }
        }

        if (_moreButton != null)
        {
            _horizontallyAccessibleControls.Add(_moreButton);
            _verticallyAccessibleControls.Add(_moreButton);
        }

        for (int i = 0; i < SecondaryCommands.Count; i++)
        {
            if (SecondaryCommands[i] is IControl c)
            {
                _verticallyAccessibleControls.Add(c);
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs args)
    {
        if (args.Handled)
            return;

        switch (args.Key)
        {
            case Key.Tab:
                var current = FocusManager.Instance?.Current;

                if (current == _moreButton)
                {
                    if (SecondaryCommands.Count > 0 && !IsOpen)
                    {
                        // Ensure the secondary commands flyout is open ...
                        IsOpen = true;
                    }

                    for (int i = 0; i < SecondaryCommands.Count; i++)
                    {
                        if (IsControlFocusable(SecondaryCommands[i] as IControl, false))
                        {
                            FocusManager.Instance.Focus(SecondaryCommands[i] as IInputElement, NavigationMethod.Tab);
                            args.Handled = true;
                            break;
                        }
                    }
                }

                if (!args.Handled && current != null)
                {
                    if (PrimaryCommands.Contains(current as ICommandBarElement))
                    {

                        // Despite calling IsOpen above, apparently the SecondaryCommands aren't yet visible
                        // and added to the tree, which means the below will fail to move focus and it will take
                        // two tabs to actually move the focus on the first time. So we use this workaround

                        bool neededOpen = !IsOpen;
                        if (SecondaryCommands.Count > 0 && !IsOpen)
                        {
                            // Ensure the secondary commands flyout is open ...
                            IsOpen = true;
                        }

                        void FocusFirstSecondary()
                        {
                            for (int i = 0; i < SecondaryCommands.Count; i++)
                            {
                                if (IsControlFocusable(SecondaryCommands[i] as IControl, false))
                                {
                                    FocusManager.Instance.Focus(SecondaryCommands[i] as IInputElement, NavigationMethod.Tab);
                                    args.Handled = true;
                                    //Debug.Assert(FocusManager.Instance.Current == SecondaryCommands[i]);
                                    break;
                                }
                            }
                        }

                        if (neededOpen)
                        {
                            Dispatcher.UIThread.Post(FocusFirstSecondary, DispatcherPriority.Layout);
                        }
                        else
                        {
                            FocusFirstSecondary();
                        }
                    }
                    else if (SecondaryCommands.Contains(current as ICommandBarElement))
                    {
                        for (int i = 0; i < PrimaryCommands.Count; i++)
                        {
                            if (IsControlFocusable(PrimaryCommands[i] as IControl, false))
                            {
                                FocusManager.Instance.Focus(PrimaryCommands[i] as IInputElement, NavigationMethod.Tab);
                                args.Handled = true;
                                break;
                            }
                        }

                        if (!args.Handled)
                        {
                            if (_moreButton != null && _moreButton.IsVisible)
                            {
                                FocusManager.Instance?.Focus(_moreButton, NavigationMethod.Tab);
                                args.Handled = true;
                            }
                        }
                    }
                }

                break;

            case Key.Right:
            case Key.Left:
            case Key.Down:
            case Key.Up:

                // INavigableContainer handles everything inside the StackPanel and we can't get around
                // that without using Preview Key handlers, which is a no-go
                // So if we make it here, we're at a point where INavigableContainer won't move the focus
                // so we need to handle it. Logic still adapted from WinUI

                // WinUI behavior, Left/Right only navigate in PrimaryCommands
                // Up/down will iterate through all commands

                bool isLeft = args.Key == Key.Left;
                bool isRight = args.Key == Key.Right;
                bool isUp = args.Key == Key.Up;
                bool isDown = args.Key == Key.Down;

                var accessibleCotnrols = (isUp || isDown) ? _verticallyAccessibleControls : _horizontallyAccessibleControls;
                int startIndex = (isLeft || isUp) ? accessibleCotnrols.Count - 1 : 0;
                int endIndex = (isLeft || isUp) ? -1 : accessibleCotnrols.Count;
                int deltaIndex = (isLeft || isUp) ? -1 : 1;
                bool shouldLoop = (isUp || isDown);
                IControl focused = null;
                int focusedIndex = -1;

                for (int i = startIndex;
                    (i != endIndex || shouldLoop) ||
                    (focusedIndex > 0 && i == focusedIndex); i += deltaIndex)
                {
                    if (i == endIndex)
                    {
                        if (focused != null)
                        {
                            i = startIndex;
                        }
                        else
                        {
                            break;
                        }
                    }

                    var control = accessibleCotnrols[i];

                    if (focused == null)
                    {
                        if (control.IsFocused)
                        {
                            focused = control;
                            focusedIndex = i;
                        }
                    }
                    else if (IsControlFocusable(control, false))
                    {
                        if (control is ICommandBarElement ele)
                        {
                            if (SecondaryCommands.Contains(ele) && !IsOpen)
                            {
                                IsOpen = true;
                            }
                        }

                        FocusManager.Instance.Focus(control, NavigationMethod.Directional);
                        args.Handled = true;
                        break;
                    }
                }

                if (!args.Handled)
                {
                    args.Handled = true;
                }
                break;
        }

        base.OnKeyDown(args);
    }

    private bool IsControlFocusable(IControl control, bool checkTabStop)
    {
        return control != null &&
            control.IsVisible && control.IsEnabled &&
            control.Focusable;// && (checkTabStop && KeyboardNavigation.GetIsTabStop(control as InputElement));
    }

    internal void SetOwningFlyout(CommandBarFlyout f)
    {
        _owningFlyout = f;
    }

    private List<IControl> _horizontallyAccessibleControls;
    private List<IControl> _verticallyAccessibleControls;

    private Button _moreButton;
    private CommandBarFlyout _owningFlyout;
}
