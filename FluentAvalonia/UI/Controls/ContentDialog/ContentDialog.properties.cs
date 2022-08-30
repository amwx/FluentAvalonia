using Avalonia.Controls.Templates;
using Avalonia;
using FluentAvalonia.Core;
using System;
using System.Windows.Input;

namespace FluentAvalonia.UI.Controls;

public partial class ContentDialog
{
    /// <summary>
    /// Defines the <see cref="CloseButtonCommand"/> property
    /// </summary>
    public static readonly StyledProperty<ICommand> CloseButtonCommandProperty =
        AvaloniaProperty.Register<ContentDialog, ICommand>(nameof(CloseButtonCommand));

    /// <summary>
    /// Defines the <see cref="CloseButtonCommandParameter"/> property
    /// </summary>
    public static readonly StyledProperty<object> CloseButtonCommandParameterProperty =
        AvaloniaProperty.Register<ContentDialog, object>(nameof(CloseButtonCommandParameter));

    /// <summary>
    /// Defines the <see cref="CloseButtonText"/> property
    /// </summary>
    public static readonly StyledProperty<string> CloseButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string>(nameof(CloseButtonText));

    /// <summary>
    /// Defines the <see cref="DefaultButton"/> property
    /// </summary>
    public static readonly StyledProperty<ContentDialogButton> DefaultButtonProperty =
        AvaloniaProperty.Register<ContentDialog, ContentDialogButton>(nameof(DefaultButton), ContentDialogButton.None);

    /// <summary>
    /// Defines the <see cref="IsPrimaryButtonEnabled"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsPrimaryButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsPrimaryButtonEnabled), true);

    /// <summary>
    /// Defines the <see cref="IsSecondaryButtonEnabled"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsSecondaryButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsSecondaryButtonEnabled), true);

    /// <summary>
    /// Defines the <see cref="PrimaryButtonCommand"/> property
    /// </summary>
    public static readonly StyledProperty<ICommand> PrimaryButtonCommandProperty =
        AvaloniaProperty.Register<ContentDialog, ICommand>(nameof(PrimaryButtonCommand));

    /// <summary>
    /// Defines the <see cref="PrimaryButtonCommandParameter"/> property
    /// </summary>
    public static readonly StyledProperty<object> PrimaryButtonCommandParameterProperty =
        AvaloniaProperty.Register<ContentDialog, object>(nameof(PrimaryButtonCommandParameter));

    /// <summary>
    /// Defines the <see cref="PrimaryButtonText"/> property
    /// </summary>
    public static readonly StyledProperty<string> PrimaryButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string>(nameof(PrimaryButtonText));

    /// <summary>
    /// Defines the <see cref="SecondaryButtonCommand"/> property
    /// </summary>
    public static readonly StyledProperty<ICommand> SecondaryButtonCommandProperty =
        AvaloniaProperty.Register<ContentDialog, ICommand>(nameof(SecondaryButtonCommand));

    /// <summary>
    /// Defines the <see cref="SecondaryButtonCommandParameter"/> property
    /// </summary>
    public static readonly StyledProperty<object> SecondaryButtonCommandParameterProperty =
        AvaloniaProperty.Register<ContentDialog, object>(nameof(SecondaryButtonCommandParameter));

    /// <summary>
    /// Defines the <see cref="SecondaryButtonText"/> property
    /// </summary>
    public static readonly StyledProperty<string> SecondaryButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string>(nameof(SecondaryButtonText));

    /// <summary>
    /// Defines the <see cref="Title"/> property
    /// </summary>
    public static readonly StyledProperty<object> TitleProperty =
        AvaloniaProperty.Register<ContentDialog, object>(nameof(Title), "");

    /// <summary>
    /// Defines the <see cref="TitleTemplate"/> property
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> TitleTemplateProperty =
        AvaloniaProperty.Register<ContentDialog, IDataTemplate>(nameof(TitleTemplate));

    /// <summary>
    /// Defines the <see cref="FullSizeDesired"/> property
    /// </summary>
    public static readonly StyledProperty<bool> FullSizeDesiredProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(FullSizeDesired));

    /// <summary>
    /// Gets or sets the command to invoke when the close button is tapped.
    /// </summary>
    public ICommand CloseButtonCommand
    {
        get
        {
            return GetValue(CloseButtonCommandProperty);
        }
        set
        {
            SetValue(CloseButtonCommandProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the command for the close button.
    /// </summary>
    public object CloseButtonCommandParameter
    {
        get
        {
            return GetValue(CloseButtonCommandParameterProperty);
        }
        set
        {
            SetValue(CloseButtonCommandParameterProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the text to display on the close button.
    /// </summary>
    public string CloseButtonText
    {
        get
        {
            return GetValue(CloseButtonTextProperty);
        }
        set
        {
            SetValue(CloseButtonTextProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates which button on the dialog is the default action.
    /// </summary>
    public ContentDialogButton DefaultButton
    {
        get
        {
            return GetValue(DefaultButtonProperty);
        }
        set
        {
            SetValue(DefaultButtonProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets whether the dialog's primary button is enabled.
    /// </summary>
    public bool IsPrimaryButtonEnabled
    {
        get
        {
            return GetValue(IsPrimaryButtonEnabledProperty);
        }
        set
        {
            SetValue(IsPrimaryButtonEnabledProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets whether the dialog's secondary button is enabled.
    /// </summary>
    public bool IsSecondaryButtonEnabled
    {
        get
        {
            return GetValue(IsSecondaryButtonEnabledProperty);
        }
        set
        {
            SetValue(IsSecondaryButtonEnabledProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the command to invoke when the primary button is tapped.
    /// </summary>
    public ICommand PrimaryButtonCommand
    {
        get
        {
            return GetValue(PrimaryButtonCommandProperty);
        }
        set
        {
            SetValue(PrimaryButtonCommandProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the command for the primary button.
    /// </summary>
    public object PrimaryButtonCommandParameter
    {
        get
        {
            return GetValue(PrimaryButtonCommandParameterProperty);
        }
        set
        {
            SetValue(PrimaryButtonCommandParameterProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the text to display on the primary button.
    /// </summary>
    public string PrimaryButtonText
    {
        get
        {
            return GetValue(PrimaryButtonTextProperty);
        }
        set
        {
            SetValue(PrimaryButtonTextProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the command to invoke when the secondary button is tapped.
    /// </summary>
    public ICommand SecondaryButtonCommand
    {
        get
        {
            return GetValue(SecondaryButtonCommandProperty);
        }
        set
        {
            SetValue(SecondaryButtonCommandProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the command for the secondary button.
    /// </summary>
    public object SecondaryButtonCommandParameter
    {
        get
        {
            return GetValue(SecondaryButtonCommandParameterProperty);
        }
        set
        {
            SetValue(SecondaryButtonCommandParameterProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the text to be displayed on the secondary button.
    /// </summary>
    public string SecondaryButtonText
    {
        get
        {
            return GetValue(SecondaryButtonTextProperty);
        }
        set
        {
            SetValue(SecondaryButtonTextProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the title of the dialog.
    /// </summary>
    public object Title
    {
        get
        {
            return GetValue(TitleProperty);
        }
        set
        {
            SetValue(TitleProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the title template.
    /// </summary>
    public IDataTemplate TitleTemplate
    {
        get
        {
            return GetValue(TitleTemplateProperty);
        }
        set
        {
            SetValue(TitleTemplateProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets whether the Dialog should show full screen
    /// On WinUI3, at least desktop, this just show the dialog at 
    /// the maximum size of a contentdialog.
    /// </summary>
    public bool FullSizeDesired
    {
        get => GetValue(FullSizeDesiredProperty);
        set => SetValue(FullSizeDesiredProperty, value);
    }

    /// <summary>
    /// Occurs before the dialog is opened
    /// </summary>
    public event TypedEventHandler<ContentDialog, EventArgs> Opening;

    /// <summary>
    /// Occurs after the dialog is opened.
    /// </summary>
    public event TypedEventHandler<ContentDialog, EventArgs> Opened;

    /// <summary>
    /// Occurs after the dialog starts to close, but before it is closed and before the Closed event occurs.
    /// </summary>
    public event TypedEventHandler<ContentDialog, ContentDialogClosingEventArgs> Closing;

    /// <summary>
    /// Occurs after the dialog is closed.
    /// </summary>
    public event TypedEventHandler<ContentDialog, ContentDialogClosedEventArgs> Closed;

    /// <summary>
    /// Occurs after the primary button has been tapped.
    /// </summary>
    public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> PrimaryButtonClick;

    /// <summary>
    /// Occurs after the secondary button has been tapped.
    /// </summary>
    public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> SecondaryButtonClick;

    /// <summary>
    /// Occurs after the close button has been tapped.
    /// </summary>
    public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> CloseButtonClick;
}
