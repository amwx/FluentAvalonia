using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FluentAvalonia.Core;
using Avalonia.Media;

namespace FluentAvalonia.UI.Controls;

[PseudoClasses(s_pcHosted, s_pcHidden, SharedPseudoclasses.s_pcOpen)]
[PseudoClasses(SharedPseudoclasses.s_pcHeader, s_pcSubheader, SharedPseudoclasses.s_pcIcon, s_pcFooter, s_pcFooterAuto, s_pcExpanded)]
[PseudoClasses(s_pcProgress, s_pcProgressError, s_pcProgressSuspend)]
[PseudoClasses(s_pcHeaderForeground, s_pcIconForeground)]
[TemplatePart(s_tpButtonsHost, typeof(ItemsControl))]
[TemplatePart(s_tpCommandsHost, typeof(ItemsControl))]
[TemplatePart(s_tpMoreDetailsButton, typeof(Button))]
[TemplatePart(s_tpProgressBar, typeof(ProgressBar))]
public partial class TaskDialog
{
    /// <summary>
    /// Defines the <see cref="Title"/> property
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<TaskDialog, string>(nameof(Title));

    /// <summary>
    /// Defines the <see cref="Header"/> property
    /// </summary>
    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<TaskDialog, string>(nameof(Header));

    /// <summary>
    /// Defines the <see cref="SubHeader"/> property
    /// </summary>
    public static readonly StyledProperty<string> SubHeaderProperty =
        AvaloniaProperty.Register<TaskDialog, string>(nameof(SubHeader));

    /// <summary>
    /// Defines the <see cref="IconSource"/> property
    /// </summary>
    public static readonly StyledProperty<IconSource> IconSourceProperty =
        AvaloniaProperty.Register<TaskDialog, IconSource>(nameof(IconSource));

    /// <summary>
    /// Defines the <see cref="Buttons"/> property
    /// </summary>
    public static readonly DirectProperty<TaskDialog, IList<TaskDialogButton>> ButtonsProperty =
        AvaloniaProperty.RegisterDirect<TaskDialog, IList<TaskDialogButton>>(nameof(Buttons),
            x => x.Buttons, (x,v) => x.Buttons = v);

    /// <summary>
    /// Defines the <see cref="Commands"/> property
    /// </summary>
    public static readonly DirectProperty<TaskDialog, IList<TaskDialogCommand>> CommandsProperty =
        AvaloniaProperty.RegisterDirect<TaskDialog, IList<TaskDialogCommand>>(nameof(Commands),
            x => x.Commands, (x, v) => x.Commands = v);

    /// <summary>
    /// Defines the <see cref="FooterVisibility"/> property
    /// </summary>
    public static readonly StyledProperty<TaskDialogFooterVisibility> FooterVisibilityProperty =
        AvaloniaProperty.Register<TaskDialog, TaskDialogFooterVisibility>(nameof(FooterVisibility));

    /// <summary>
    /// Defines the <see cref="IsFooterExpanded"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsFooterExpandedProperty =
        AvaloniaProperty.Register<TaskDialog, bool>(nameof(IsFooterExpanded));

    /// <summary>
    /// Defines the <see cref="Footer"/> property
    /// </summary>
    public static readonly StyledProperty<object> FooterProperty =
        AvaloniaProperty.Register<TaskDialog, object>(nameof(Footer));

    /// <summary>
    /// Defines the <see cref="FooterTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> FooterTemplateProperty =
        AvaloniaProperty.Register<TaskDialog, IDataTemplate>(nameof(FooterTemplate));

    /// <summary>
    /// Defines the <see cref="ShowProgressBar"/> property
    /// </summary>
    public static readonly StyledProperty<bool> ShowProgressBarProperty =
        AvaloniaProperty.Register<TaskDialog, bool>(nameof(ShowProgressBar));

    /// <summary>
    /// Defines the <see cref="HeaderBackground"/> property
    /// </summary>
    public static readonly StyledProperty<IBrush> HeaderBackgroundProperty =
        AvaloniaProperty.Register<TaskDialog, IBrush>(nameof(HeaderBackground));

    /// <summary>
    /// Defines the <see cref="HeaderForeground"/> property
    /// </summary>
    public static readonly StyledProperty<IBrush> HeaderForegroundProperty =
        AvaloniaProperty.Register<TaskDialog, IBrush>(nameof(HeaderForeground));

    /// <summary>
    /// Defines the <see cref="IconForeground"/> property
    /// </summary>
    public static readonly StyledProperty<IBrush> IconForegroundProperty =
        AvaloniaProperty.Register<TaskDialog, IBrush>(nameof(IconForeground));

    /// <summary>
    /// Gets or sets the title of the dialog
    /// </summary>
    /// <remarks>
    /// This is the window caption of the dialog displayed in the title bar. For platforms 
    /// where windowing is not supported, this property has no effect.
    /// </remarks>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the dialog header text
    /// </summary>
    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the dialog sub header text
    /// </summary>
    public string SubHeader
    {
        get => GetValue(SubHeaderProperty);
        set => SetValue(SubHeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the dialog Icon
    /// </summary>
    public IconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// <summary>
    /// Gets the list of buttons that display at the bottom of the TaskDialog
    /// </summary>
    public IList<TaskDialogButton> Buttons
    {
        get => _buttons;
        set => SetAndRaise(ButtonsProperty, ref _buttons, value);
    }

    /// <summary>
    /// Gets the list of Commands displayed in the TaskDialog
    /// </summary>
    public IList<TaskDialogCommand> Commands
    {
        get => _commands;
        set => SetAndRaise(CommandsProperty, ref _commands, value);
    }

    /// <summary>
    /// Gets or sets the visibility of the Footer area
    /// </summary>
    public TaskDialogFooterVisibility FooterVisibility
    {
        get => GetValue(FooterVisibilityProperty);
        set => SetValue(FooterVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the footer is visible
    /// </summary>
    public bool IsFooterExpanded
    {
        get => GetValue(IsFooterExpandedProperty);
        set => SetValue(IsFooterExpandedProperty, value);
    }

    /// <summary>
    /// Gets or sets the footer content
    /// </summary>
    public object Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    /// Gets or sets the IDataTemplate for the footer content
    /// </summary>
    public IDataTemplate FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this TaskDialog shows a progress bar
    /// </summary>
    public bool ShowProgressBar
    {
        get => GetValue(ShowProgressBarProperty);
        set => SetValue(ShowProgressBarProperty, value);
    }

    /// <summary>
    /// Gets or sets the background of the header region of the task dialog
    /// </summary>
    public IBrush HeaderBackground
    {
        get => GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground of the header text for the TaskDialog
    /// </summary>
    public IBrush HeaderForeground
    {
        get => GetValue(HeaderForegroundProperty);
        set => SetValue(HeaderForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground of the <see cref="IconSource"/> for the TaskDialog
    /// </summary>
    public IBrush IconForeground
    {
        get => GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the root visual that should host this dialog
    /// </summary>
    /// <remarks>
    /// For TaskDialogs declared in Xaml, this is automatically set. If you declare a 
    /// TaskDialog in C#, you MUST set this property before showing the dialog to prevent
    /// and error. For desktop platforms, set it to the Window that should own the dialog.
    /// For others, set it to the root TopLevel.
    /// </remarks>
    public Visual XamlRoot { get; set; }

    /// <summary>
    /// Raised when the TaskDialog is beginning to open, but is not yet visible
    /// </summary>
    public event TypedEventHandler<TaskDialog, EventArgs> Opening;

    /// <summary>
    /// Raised when the TaskDialog is opened and ready to be shown on screen
    /// </summary>
    public event TypedEventHandler<TaskDialog, EventArgs> Opened;

    /// <summary>
    /// Raised when the TaskDialog is beginning to close
    /// </summary>
    public event TypedEventHandler<TaskDialog, TaskDialogClosingEventArgs> Closing;

    /// <summary>
    /// Raised when the TaskDialog is closed
    /// </summary>
    public event TypedEventHandler<TaskDialog, EventArgs> Closed;

    private IList<TaskDialogButton> _buttons;
    private IList<TaskDialogCommand> _commands;

    private const string s_tpButtonsHost = "ButtonsHost";
    private const string s_tpCommandsHost = "CommandsHost";
    private const string s_tpProgressBar = "ProgressBar";
    private const string s_tpMoreDetailsButton = "MoreDetailsButton";

    private const string s_pcHidden = ":hidden";
    private const string s_pcHosted = ":hosted";
    private const string s_pcSubheader = ":subheader";
    private const string s_pcFooter = ":footer";
    private const string s_pcFooterAuto = ":footerAuto";
    private const string s_pcExpanded = ":expanded";
    private const string s_pcProgress = ":progress";
    private const string s_pcProgressError = ":progressError";
    private const string s_pcProgressSuspend = ":progressSuspend";
    private const string s_pcHeaderForeground = ":headerForeground";
    private const string s_pcIconForeground = ":iconForeground";

    private const string s_cFATDCom = "FA_TaskDialogCommand";
}
