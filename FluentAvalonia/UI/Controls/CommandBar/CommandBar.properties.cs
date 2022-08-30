using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using FluentAvalonia.Core;
using System;

namespace FluentAvalonia.UI.Controls;

public partial class CommandBar
{
    /// <summary>
    /// Defines the <see cref="IsSticky"/> property
    /// </summary>
    public static readonly DirectProperty<CommandBar, bool> IsStickyProperty =
        AvaloniaProperty.RegisterDirect<CommandBar, bool>(nameof(IsSticky),
            x => x.IsSticky, (x, v) => x.IsSticky = v);

    /// <summary>
    /// Define the <see cref="IsOpen"/> property
    /// </summary>
    public static readonly DirectProperty<CommandBar, bool> IsOpenProperty =
        AvaloniaProperty.RegisterDirect<CommandBar, bool>(nameof(IsOpen),
            x => x.IsOpen, (x, v) => x.IsOpen = v);

    /// <summary>
    /// Defines the <see cref="ClosedDisplayMode"/> property
    /// </summary>
    public static readonly StyledProperty<CommandBarClosedDisplayMode> ClosedDisplayModeProperty =
        AvaloniaProperty.Register<CommandBar, CommandBarClosedDisplayMode>(nameof(ClosedDisplayMode));

    /// <summary>
    /// Defines the <see cref="PrimaryCommands"/> property
    /// </summary>
    public static readonly DirectProperty<CommandBar, IAvaloniaList<ICommandBarElement>> PrimaryCommandsProperty =
        AvaloniaProperty.RegisterDirect<CommandBar, IAvaloniaList<ICommandBarElement>>(nameof(PrimaryCommands),
            x => x.PrimaryCommands);

    /// <summary>
    /// Defines the <see cref="SecondaryCommands"/> property
    /// </summary>
    public static readonly DirectProperty<CommandBar, IAvaloniaList<ICommandBarElement>> SecondaryCommandsProperty =
        AvaloniaProperty.RegisterDirect<CommandBar, IAvaloniaList<ICommandBarElement>>(nameof(SecondaryCommands),
            x => x.SecondaryCommands);

    /// <summary>
    /// Defines the <see cref="OverflowButtonVisibility"/> property
    /// </summary>
    public static readonly StyledProperty<CommandBarOverflowButtonVisibility> OverflowButtonVisibilityProperty =
        AvaloniaProperty.Register<CommandBar, CommandBarOverflowButtonVisibility>(nameof(OverflowButtonVisibility));

    /// <summary>
    /// Defines the <see cref="IsDynamicOverflowEnabled"/>
    /// </summary>
    public static readonly DirectProperty<CommandBar, bool> IsDynamicOverflowEnabledProperty =
        AvaloniaProperty.RegisterDirect<CommandBar, bool>(nameof(SecondaryCommands),
            x => x.IsDynamicOverflowEnabled, (x, v) => x.IsDynamicOverflowEnabled = v);

    /// <summary>
    /// Defines the <see cref="ItemsAlignment"/> property
    /// </summary>
    public static readonly StyledProperty<HorizontalAlignment> ItemsAlignmentProperty =
        AvaloniaProperty.Register<CommandBar, HorizontalAlignment>(nameof(ItemsAlignment), HorizontalAlignment.Left);

    /// <summary>
    /// Defines the <see cref="DefaultLabelPosition"/> property
    /// </summary>
    public static readonly StyledProperty<CommandBarDefaultLabelPosition> DefaultLabelPositionProperty =
        AvaloniaProperty.Register<CommandBar, CommandBarDefaultLabelPosition>(nameof(DefaultLabelPosition));

    /// <summary>
    /// Gets or sets a value that indicates whether the CommandBar does not close on light dismiss.
    /// </summary>
    public bool IsSticky
    {
        get => _isSticky;
        set => SetAndRaise(IsStickyProperty, ref _isSticky, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the CommandBar is open.
    /// </summary>
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (value)
            {
                OnOpening();
            }
            else
            {
                OnClosing();
            }

            if (SetAndRaise(IsOpenProperty, ref _isOpen, value))
            {
                PseudoClasses.Set(":open", value);
                SetElementVisualStateForOpen(value);

                if (value)
                {
                    OnOpened();
                }
                else
                {
                    OnClosed();
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether icon buttons are displayed when the 
    /// command bar is not completely open.
    /// </summary>
    public CommandBarClosedDisplayMode ClosedDisplayMode
    {
        get => GetValue(ClosedDisplayModeProperty);
        set => SetValue(ClosedDisplayModeProperty, value);
    }

    /// <summary>
    /// Gets the collection of primary command elements for the CommandBar.
    /// </summary>
    public IAvaloniaList<ICommandBarElement> PrimaryCommands
    {
        get => _primaryCommands;
        private set => SetAndRaise(PrimaryCommandsProperty, ref _primaryCommands, value);
    }

    /// <summary>
    /// Gets the collection of secondary command elements for the CommandBar.
    /// </summary>
    public IAvaloniaList<ICommandBarElement> SecondaryCommands
    {
        get => _secondaryCommands;
        private set => SetAndRaise(SecondaryCommandsProperty, ref _secondaryCommands, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates when a command bar's overflow button is shown.
    /// </summary>
    public CommandBarOverflowButtonVisibility OverflowButtonVisibility
    {
        get => GetValue(OverflowButtonVisibilityProperty);
        set => SetValue(OverflowButtonVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether primary commands automatically 
    /// move to the overflow menu when space is limited.
    /// </summary>
    public bool IsDynamicOverflowEnabled
    {
        get => _isDynamicOverflowEnabled;
        set => SetAndRaise(IsDynamicOverflowEnabledProperty, ref _isDynamicOverflowEnabled, value);
    }

    /// <summary>
    /// Gets or sets how the <see cref="PrimaryCommands"/> align in the CommandBar
    /// </summary>
    /// <remarks>
    /// This property doesn't exist in WinUI, where PrimaryCommands are always right aligned.
    /// This property gives you more flexibility to control this behavior.
    /// </remarks>
    public HorizontalAlignment ItemsAlignment
    {
        get => GetValue(ItemsAlignmentProperty);
        set => SetValue(ItemsAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates the placement and visibility of the labels 
    /// on the command bar's buttons.
    /// </summary>
    public CommandBarDefaultLabelPosition DefaultLabelPosition
    {
        get => GetValue(DefaultLabelPositionProperty);
        set => SetValue(DefaultLabelPositionProperty, value);
    }

    /// <summary>
    /// Occurs when the CommandBar changes from hidden to visible.
    /// </summary>
    public event TypedEventHandler<CommandBar, EventArgs> Opened;

    /// <summary>
    /// Occurs when the CommandBar starts to change from hidden to visible.
    /// </summary>
    public event TypedEventHandler<CommandBar, EventArgs> Opening;

    /// <summary>
    /// Occurs when the CommandBar changes from visible to hidden.
    /// </summary>
    public event TypedEventHandler<CommandBar, EventArgs> Closed;

    /// <summary>
    /// Occurs when the CommandBar starts to change from visible to hidden.
    /// </summary>
    public event TypedEventHandler<CommandBar, EventArgs> Closing;

    // TODO:
    //public event TypedEventHandler<CommandBar, DynamicOverflowItemsChangingEventArgs> DynamicOverflowItemsChanging;

    private bool _isSticky = true;
    private bool _isOpen;
    private IAvaloniaList<ICommandBarElement> _primaryCommands;
    private IAvaloniaList<ICommandBarElement> _secondaryCommands;
    private bool _isDynamicOverflowEnabled = true;
}
