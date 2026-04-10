using Avalonia;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;

namespace FluentAvalonia.UI.Input;

/// <summary>
/// Derives from XamlUICommand, adding a set of standard platform commands with pre-defined properties.
/// </summary>
public class FAStandardUICommand : FAXamlUICommand
{
    public FAStandardUICommand() { }

    public FAStandardUICommand(FAStandardUICommandKind kind)
    {
        Kind = kind;

        SetupCommand();
    }

    /// <summary>
    /// Defines the <see cref="Kind"/> property
    /// </summary>
    public static readonly StyledProperty<FAStandardUICommandKind> KindProperty =
        AvaloniaProperty.Register<FAStandardUICommand, FAStandardUICommandKind>(nameof(Kind));

    /// <summary>
    /// Gets the platform command (with pre-defined properties such as icon, keyboard accelerator, 
    /// and description) that can be used with a StandardUICommand.
    /// </summary>
    public FAStandardUICommandKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == KindProperty)
        {
            SetupCommand();
        }
    }

    private void SetupCommand()
    {
        switch (Kind)
        {
            case FAStandardUICommandKind.None:
                Label = string.Empty;
                IconSource = null;
                Description = string.Empty;
                HotKey = null;
                break;

            case FAStandardUICommandKind.Cut:
                Label = "Cut";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Cut };
                Description = "Remove the selected content and put it on the clipboard";
                HotKey = new KeyGesture(Key.X, KeyModifiers.Control);
                break;

            case FAStandardUICommandKind.Copy:
                Label = "Copy";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Copy };
                Description = "Copy the selected content to the clipboard";
                HotKey = new KeyGesture(Key.C, KeyModifiers.Control);
                break;

            case FAStandardUICommandKind.Paste:
                Label = "Paste";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Paste };
                Description = "Insert the contents of the clipboard at the current location";
                HotKey = new KeyGesture(Key.V, KeyModifiers.Control);
                break;

            case FAStandardUICommandKind.SelectAll:
                Label = "Select All";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.SelectAll };
                Description = "Select all content";
                HotKey = new KeyGesture(Key.A, KeyModifiers.Control);
                break;

            case FAStandardUICommandKind.Delete:
                Label = "Delete";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Delete };
                Description = "Delete the selected content";
                HotKey = new KeyGesture(Key.Delete);
                break;

            case FAStandardUICommandKind.Share:
                Label = "Share";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Share };
                Description = "Share the selected content";
                // No HotKey
                break;

            case FAStandardUICommandKind.Save:
                Label = "Save";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Save };
                Description = "Save your changes";
                HotKey = new KeyGesture(Key.S, KeyModifiers.Control);
                break;

            case FAStandardUICommandKind.Open:
                Label = Description = "Open";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Open };
                HotKey = new KeyGesture(Key.O, KeyModifiers.Control);
                break;

            case FAStandardUICommandKind.Close:
                Label = Description = "Close";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Dismiss };
                HotKey = new KeyGesture(Key.W, KeyModifiers.Control);
                break;

            case FAStandardUICommandKind.Pause:
                Label = Description = "Pause";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Pause };
                // No HotKey
                break;

            case FAStandardUICommandKind.Play:
                Label = Description = "Play";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Play };
                // No HotKey
                break;

            case FAStandardUICommandKind.Stop:
                Label = Description = "Stop";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Stop };
                // No HotKey
                break;

            case FAStandardUICommandKind.Forward:
                Label = "Forward";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Forward };
                Description = "Go to the next item";
                // No HotKey
                break;

            case FAStandardUICommandKind.Backward:
                Label = "Backward";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Back };
                Description = "Back";
                // No HotKey
                break;

            case FAStandardUICommandKind.Undo:
                Label = "Undo";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Undo };
                Description = "Reverse the most recent action";
                HotKey = new KeyGesture(Key.Z, KeyModifiers.Control);
                break;

            case FAStandardUICommandKind.Redo:
                Label = "Redo";
                IconSource = new FASymbolIconSource { Symbol = FASymbol.Redo };
                Description = "Repeat the most recently undone action";
                HotKey = new KeyGesture(Key.Y, KeyModifiers.Control);
                break;

            default:
                throw new NotImplementedException();
        }
    }
}
