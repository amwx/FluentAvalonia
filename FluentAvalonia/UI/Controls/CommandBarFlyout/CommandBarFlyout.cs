using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a specialized flyout that provides layout for CommandBarButton,
/// CommandBarToggleButton, and CommandBarSeparator controls.
/// </summary>
public class CommandBarFlyout : FlyoutBase
{
    public CommandBarFlyout()
    {
        // TEMPORARY FIX...REVERT TO CREATEPRESENTER() WHEN NRE ISSUE FIXED
        _commandBar = new CommandBarFlyoutCommandBar();
        _commandBar.SetOwningFlyout(this);

        PrimaryCommands = new AvaloniaList<ICommandBarElement>();
        SecondaryCommands = new AvaloniaList<ICommandBarElement>();

        PrimaryCommands.CollectionChanged += (s, e) =>
        {
            if (_commandBar == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _commandBar.PrimaryCommands.InsertRange(e.NewStartingIndex, e.NewItems.Cast<ICommandBarElement>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    _commandBar.PrimaryCommands.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                    break;

                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    _commandBar.PrimaryCommands.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                    _commandBar.PrimaryCommands.InsertRange(e.NewStartingIndex, e.NewItems.Cast<ICommandBarElement>());

                    break;

                case NotifyCollectionChangedAction.Reset:
                    _commandBar.PrimaryCommands.Clear();
                    break;
            }
        };

        SecondaryCommands.CollectionChanged += (sender, e) =>
        {
            if (_commandBar == null)
                return;

            // We want to ensure that any interaction with secondary items causes the CommandBarFlyout
            // to close, so we'll attach a Click handler to any buttons and Checked/Unchecked handlers
            // to any toggle buttons that we get and close the flyout when they're invoked.
            // The only exception is buttons with flyouts - in that case, clicking on the button
            // will just open the flyout rather than executing an action, so we don't want that to
            // do anything.
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _commandBar.SecondaryCommands.InsertRange(e.NewStartingIndex, e.NewItems.Cast<ICommandBarElement>());

                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        if (e.NewItems[i] is CommandBarButton b)
                        {
                            b.Click += OnCommandBarButtonInSecondaryCommandsClick;
                        }
                        else if (e.NewItems[i] is CommandBarToggleButton tb)
                        {
                            // Fortunately Click is fired even on ToggleButton so we
                            // don't need to hook Checked/Unchecked
                            tb.Click += OnCommandBarButtonInSecondaryCommandsClick;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    _commandBar.SecondaryCommands.RemoveRange(e.OldStartingIndex, e.OldItems.Count);

                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        if (e.OldItems[i] is CommandBarButton b)
                        {
                            b.Click += OnCommandBarButtonInSecondaryCommandsClick;
                        }
                        else if (e.OldItems[i] is CommandBarToggleButton tb)
                        {
                            // Fortunately Click is fired even on ToggleButton so we
                            // don't need to hook Checked/Unchecked
                            tb.Click += OnCommandBarButtonInSecondaryCommandsClick;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    _commandBar.SecondaryCommands.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                    _commandBar.SecondaryCommands.InsertRange(e.NewStartingIndex, e.NewItems.Cast<ICommandBarElement>());
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _commandBar.SecondaryCommands.Clear();
                    if (e.OldItems != null)
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            if (e.OldItems[i] is CommandBarButton b)
                            {
                                b.Click += OnCommandBarButtonInSecondaryCommandsClick;
                            }
                            else if (e.OldItems[i] is CommandBarToggleButton tb)
                            {
                                // Fortunately Click is fired even on ToggleButton so we
                                // don't need to hook Checked/Unchecked
                                tb.Click += OnCommandBarButtonInSecondaryCommandsClick;
                            }
                        }
                    }
                    break;
            }

        };
    }

    /// <summary>
    /// Defines the <see cref="AlwaysExpanded"/> property
    /// </summary>
    public static readonly StyledProperty<bool> AlwaysExpandedProperty =
        AvaloniaProperty.Register<CommandBarFlyout, bool>(nameof(AlwaysExpanded));

    /// <summary>
    /// Gets the collection of primary command elements for the CommandBarFlyout.
    /// </summary>
    [Content]
    public IAvaloniaList<ICommandBarElement> PrimaryCommands { get; }

    /// <summary>
    /// Gets the collection of secondary command elements for the CommandBarFlyout.
    /// </summary>
    public IAvaloniaList<ICommandBarElement> SecondaryCommands { get; }

    /// <summary>
    /// Gets or sets a value that indicates whether or not the CommandBarFlyout should 
    /// always stay in its Expanded state and block the user from entering the Collapsed state. 
    /// Defaults to false.
    /// </summary>
    public bool AlwaysExpanded
    {
        get => GetValue(AlwaysExpandedProperty);
        set => SetValue(AlwaysExpandedProperty, value);
    }

    protected override Control CreatePresenter()
    {
        _presenter = new FlyoutPresenter
        {
            Background = null,
            Foreground = null,
            BorderBrush = null,
            MinWidth = 0,
            MaxWidth = double.PositiveInfinity,
            MinHeight = 0,
            MaxHeight = double.PositiveInfinity,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            Content = _commandBar
        };

        return _presenter;
    }

    protected override void OnOpening(CancelEventArgs args)
    {
        base.OnOpening(args);

        if (PrimaryCommands.Count > 0 && _commandBar.PrimaryCommands.Count == 0)
        {
            _commandBar.PrimaryCommands.AddRange(PrimaryCommands);
        }
        if (SecondaryCommands.Count > 0 && _commandBar.SecondaryCommands.Count == 0)
        {
            _commandBar.SecondaryCommands.AddRange(SecondaryCommands);

            for (int i = 0; i < SecondaryCommands.Count; i++)
            {
                if (SecondaryCommands[i] is CommandBarButton b)
                {
                    b.Click += OnCommandBarButtonInSecondaryCommandsClick;
                }
                else if (SecondaryCommands[i] is CommandBarToggleButton tb)
                {
                    // Fortunately Click is fired even on ToggleButton so we
                    // don't need to hook Checked/Unchecked
                    tb.Click += OnCommandBarButtonInSecondaryCommandsClick;
                }
            }
        }

        if (AlwaysExpanded)
        {
            _commandBar.OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Collapsed;
            ShowMode = FlyoutShowMode.Standard;
        }
        else
        {
            _commandBar.OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Auto;
        }

        if (ShowMode == FlyoutShowMode.Standard && SecondaryCommands.Count > 0 || PrimaryCommands.Count == 0)
        {
            _commandBar.IsOpen = true;
        }
    }

    protected override void OnClosed()
    {
        base.OnClosed();

        _commandBar.IsOpen = false;
    }

    private void OnCommandBarButtonInSecondaryCommandsClick(object sender, RoutedEventArgs e)
    {
        HideCore(false);
    }

    protected CommandBarFlyoutCommandBar _commandBar;
    protected FlyoutPresenter _presenter;
}
