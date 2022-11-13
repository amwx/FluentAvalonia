using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Input.Raw;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Input;
using System;
using System.Collections.Generic;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a specialized command bar flyout that contains commands for editing text.
/// </summary>
public class TextCommandBarFlyout : CommandBarFlyout
{
    public TextCommandBarFlyout()
    {
        Opening += (_, __) =>
        {
            UpdateButtons();
        };

        Opened += (_, __) =>
        {
            // If there aren't any primary commands and we aren't opening expanded,
            // or if there are just no commands at all, then we'll have literally no UI to show. 
            // We'll just close the flyout in that case - nothing should be opening us
            // in this state anyway, but this makes sure we don't have a light-dismiss layer
            // with nothing visible to light dismiss.

            bool isStandard = ShowMode == FlyoutShowMode.Standard;

            if (PrimaryCommands.Count == 0 &&
            (SecondaryCommands.Count == 0) || (!_commandBar.IsOpen && !isStandard))
            {
                Hide();
            }
        };
    }


    private void InitializeButtonWithUICommand(Button b,
        XamlUICommand command, Action executeFunc)
    {
        // WinUI collects the event token for revoking later, but never actually does
        // This should be ok because the button is tied to the TextCommandBarFlyout
        // so it's only created once and we shouldn't leak
        command.ExecuteRequested += (_, __) => { executeFunc(); };

        b.Command = command;
    }

    private void UpdateButtons()
    {
        PrimaryCommands.Clear();
        SecondaryCommands.Clear();

        var buttonsToAdd = GetButtonsToAdd();

        void addButtonToCommandsIfPresent(TextControlButtons buttonType, IList<ICommandBarElement> commandsList)
        {
            if ((buttonsToAdd & buttonType) != TextControlButtons.None)
            {
                commandsList.Add(GetButton(buttonType));
            }
        }

        // No RichTextBox yet, so skipping this - Bold/Italic/Underline not supported yet
        //void addRichEditButtonToCommandsIfPresent(TextControlButtons buttonType, 
        //	IList<ICommandBarElement> commandsList, bool getIsChecked)
        //{
        //	if ((buttonsToAdd & buttonType) != TextControlButtons.None)
        //	{
        //		//TODO
        //	}
        //}

        // We don't have proofing flyouts, so skip that

        // We don't have FlyoutBase.InputDevicePrefersPrimaryCommands
        // So we'll always load Cut/Copy/Paste into Secondary

        addButtonToCommandsIfPresent(TextControlButtons.Cut, SecondaryCommands);
        addButtonToCommandsIfPresent(TextControlButtons.Copy, SecondaryCommands);
        addButtonToCommandsIfPresent(TextControlButtons.Paste, SecondaryCommands);

        //TODO: the bool arg
        //addRichEditButtonToCommandsIfPresent(TextControlButtons.Bold, PrimaryCommands, false);
        //addRichEditButtonToCommandsIfPresent(TextControlButtons.Italic, PrimaryCommands, false);
        //addRichEditButtonToCommandsIfPresent(TextControlButtons.Underline, PrimaryCommands, false);

        addButtonToCommandsIfPresent(TextControlButtons.Undo, SecondaryCommands);
        addButtonToCommandsIfPresent(TextControlButtons.Redo, SecondaryCommands);
        addButtonToCommandsIfPresent(TextControlButtons.SelectAll, SecondaryCommands);
    }

    private TextControlButtons GetButtonsToAdd()
    {
        TextControlButtons toAdd = TextControlButtons.None;
        var target = Target;

        // Since we don't have RichTextBox, RichTextBlock, or PasswordBox, we'll just let TextBox get all
        // of those where appropriate. TextBlock will stay the same. Basically no Bold/Italic/Underline
        // commands until RichTextBox is added to Avalonia
        if (target is TextBox tbTarget)
        {
            if (tbTarget.PasswordChar != default(char))
            {
                toAdd = GetPasswordBoxButtonsToAdd(tbTarget);
            }

            toAdd = GetTextBoxButtonsToAdd(tbTarget);
        }
        else if (target is TextBlock txtTarget)
        {
            toAdd = GetTextBlockButtonsToAdd(txtTarget);
        }

        return toAdd;
    }

    private TextControlButtons GetTextBoxButtonsToAdd(TextBox textBox)
    {
        TextControlButtons toAdd = TextControlButtons.None;

        var selLength = textBox.SelectionEnd - textBox.SelectionStart;
        if (!textBox.IsReadOnly)
        {
            if (selLength > 0)
            {
                toAdd |= TextControlButtons.Cut;
            }

            if (textBox.CanPaste)
            {
                toAdd |= TextControlButtons.Paste;
            }

            // We don't have CanUndo or CanRedo
            // In next verion of Avalonia, we'll get TextBox.IsUndoEnabled, but it's not the same
            // For now, we'll default to adding these, and probably just send the Undo/Redo keys

            toAdd |= TextControlButtons.Undo;
            toAdd |= TextControlButtons.Redo;
        }

        if (selLength > 0)
        {
            toAdd |= TextControlButtons.Copy;
        }

        if (!string.IsNullOrEmpty(textBox.Text) && textBox.Text.Length > 0)
        {
            toAdd |= TextControlButtons.SelectAll;
        }

        return toAdd;
    }

    private TextControlButtons GetTextBlockButtonsToAdd(TextBlock tb)
    {
        // TextBlocks aren't as robust as WinUI, but we should still be able 
        // to make Copy work. SelectAll won't though

        return TextControlButtons.Copy;
    }

    //private TextControlButtons GetRichEditBoxButtonsToAdd() { }
    //private TextControlButtons GetRichTextBlockButtonsToAdd() { }

    private TextControlButtons GetPasswordBoxButtonsToAdd(TextBox textBox)
    {
        TextControlButtons toAdd = TextControlButtons.None;

        if (textBox.CanPaste)
        {
            toAdd |= TextControlButtons.Paste;
        }

        if (!string.IsNullOrEmpty(textBox.Text) && textBox.Text.Length > 0)
        {
            toAdd |= TextControlButtons.SelectAll;
        }

        return toAdd;
    }

    private bool IsButtonInPrimaryCommands(TextControlButtons button)
    {
        return PrimaryCommands.Contains(GetButton(button));
    }

    private void ExecuteCutCommand()
    {
        var target = Target;
        try
        {
            if (target is TextBox tb)
            {
                tb.Cut();
            }
        }
        catch
        {
            // TODO: probably should log the error if one is thrown, but don't fail b/c of it
            // Clipboard errors do happen
        }

        if (IsButtonInPrimaryCommands(TextControlButtons.Cut))
        {
            UpdateButtons();
        }
    }

    private async void ExecuteCopyCommand()
    {
        var target = Target;
        try
        {
            if (target is TextBox tb)
            {
                tb.Copy();
            }
            else if (target is TextBlock txtB)
            {
                await AvaloniaLocator.Current.GetService<IClipboard>().SetTextAsync(txtB.Text);
            }
        }
        catch
        {
            // TODO: probably should log the error if one is thrown, but don't fail b/c of it
            // Clipboard errors do happen
        }

        if (IsButtonInPrimaryCommands(TextControlButtons.Copy))
        {
            UpdateButtons();
        }
    }

    private async void ExecutePasteCommand()
    {
        var target = Target;
        try
        {
            if (target is TextBox tb)
            {
                tb.Paste();
            }
            else if (target is TextBlock txtB)
            {
                var txt = await AvaloniaLocator.Current.GetService<IClipboard>().GetTextAsync();
                if (txt != null)
                {
                    txtB.Text = txt;
                }
            }
        }
        catch
        {
            // TODO: probably should log the error if one is thrown, but don't fail b/c of it
            // Clipboard errors do happen
        }

        if (IsButtonInPrimaryCommands(TextControlButtons.Paste))
        {
            UpdateButtons();
        }
    }

    private void ExecuteBoldCommand()
    { }

    private void ExecuteItalicCommand()
    { }

    private void ExecuteUnderlineCommand()
    { }

    private void ExecuteUndoCommand()
    {
        // TextBox has no public way of calling Undo/Redo, so we'll just send the keys to it
        // TBH I have no idea if this will work. I can test on Windows and linux, but I'll 
        // never own a mac, but hopefully the key system is smart enough to work out what
        // Ctrl+z is cross platform that this will work.

        if (Target is TextBox tb)
        {
            // v2 - Avalonia decided PointerEventArgs and like shouldn't be publicly constructable so our way to get around
            //      this is drop down to the low-level event and make the input manager process it 
            //      Also now pulling the keys from the PlatformHotkeyConfiguration

            var keymap = AvaloniaLocator.Current.GetRequiredService<PlatformHotkeyConfiguration>();

            // Iterate backwards as this will take user specified key shortcuts first over the ones specified in the 
            // original platform initialization
            ulong ts = (ulong)DateTime.Now.Ticks;
            for (int i = keymap.Undo.Count - 1; i >= 0; i--)
            {
                var gesture = keymap.Undo[i];
                var args = new RawKeyEventArgs(KeyboardDevice.Instance, ts, (IInputRoot)tb.GetVisualRoot(),
                    RawKeyEventType.KeyDown, gesture.Key, GetRawMods(gesture.KeyModifiers));

                InputManager.Instance.ProcessInput(args);

                if (args.Handled)
                    break;
            }            
        }

        if (IsButtonInPrimaryCommands(TextControlButtons.Undo))
        {
            UpdateButtons();
        }
    }

    private void ExecuteRedoCommand()
    {
        // TextBox has no public way of calling Undo/Redo, so we'll just send the keys to it
        // TBH I have no idea if this will work. I can test on Windows and linux, but I'll 
        // never own a mac, but hopefully the key system is smart enough to work out what
        // Ctrl+z is cross platform that this will work.

        if (Target is TextBox tb)
        {
            // v2 - Avalonia decided PointerEventArgs and like shouldn't be publicly constructable so our way to get around
            //      this is drop down to the low-level event and make the input manager process it 
            //      Also now pulling the keys from the PlatformHotkeyConfiguration

            var keymap = AvaloniaLocator.Current.GetRequiredService<PlatformHotkeyConfiguration>();

            // Iterate backwards as this will take user specified key shortcuts first over the ones specified in the 
            // original platform initialization
            ulong ts = (ulong)DateTime.Now.Ticks;
            for (int i = keymap.Redo.Count - 1; i >= 0; i--)
            {
                var gesture = keymap.Redo[i];
                var args = new RawKeyEventArgs(KeyboardDevice.Instance, ts, (IInputRoot)tb.GetVisualRoot(), 
                    RawKeyEventType.KeyDown, gesture.Key, GetRawMods(gesture.KeyModifiers));

                InputManager.Instance.ProcessInput(args);

                if (args.Handled)
                    break;
            }
        }

        if (IsButtonInPrimaryCommands(TextControlButtons.Redo))
        {
            UpdateButtons();
        }
    }

    private static RawInputModifiers GetRawMods(KeyModifiers km)
    {
        RawInputModifiers rm = RawInputModifiers.None;

        if ((km & KeyModifiers.Control) == KeyModifiers.Control)
            rm |= RawInputModifiers.Control;

        if ((km & KeyModifiers.Shift) == KeyModifiers.Shift)
            rm |= RawInputModifiers.Shift;

        if ((km & KeyModifiers.Meta) == KeyModifiers.Meta)
            rm |= RawInputModifiers.Meta;

        if ((km & KeyModifiers.Alt) == KeyModifiers.Alt)
            rm |= RawInputModifiers.Alt;

        return rm;
    }

    private void ExecuteSelectAllCommand()
    {
        if (Target is TextBox tb)
        {
            tb.SelectAll();
        }

        if (IsButtonInPrimaryCommands(TextControlButtons.SelectAll))
        {
            UpdateButtons();
        }
    }

    private ICommandBarElement GetButton(TextControlButtons textControlButton)
    {
        if (_buttons == null)
            _buttons = new Dictionary<TextControlButtons, ICommandBarElement>();

        if (_buttons.ContainsKey(textControlButton))
        {
            return _buttons[textControlButton];
        }
        else
        {
            switch (textControlButton)
            {
                case TextControlButtons.Cut:
                    {
                        var button = new CommandBarButton();
                        InitializeButtonWithUICommand(button, new StandardUICommand(StandardUICommandKind.Cut), ExecuteCutCommand);
                        _buttons.Add(TextControlButtons.Cut, button);
                        return button;
                    }

                case TextControlButtons.Copy:
                    {
                        var button = new CommandBarButton();
                        InitializeButtonWithUICommand(button, new StandardUICommand(StandardUICommandKind.Copy), ExecuteCopyCommand);
                        _buttons.Add(TextControlButtons.Copy, button);
                        return button;
                    }

                case TextControlButtons.Paste:
                    {
                        var button = new CommandBarButton();
                        InitializeButtonWithUICommand(button, new StandardUICommand(StandardUICommandKind.Paste), ExecutePasteCommand);
                        _buttons.Add(TextControlButtons.Paste, button);
                        return button;
                    }

                // Skip Bold/Italic/Underline, since we don't have those right now

                case TextControlButtons.Bold:
                case TextControlButtons.Italic:
                case TextControlButtons.Underline:
                    return null;

                case TextControlButtons.Undo:
                    {
                        var button = new CommandBarButton();
                        InitializeButtonWithUICommand(button, new StandardUICommand(StandardUICommandKind.Undo), ExecuteUndoCommand);
                        _buttons.Add(TextControlButtons.Undo, button);
                        return button;
                    }

                case TextControlButtons.Redo:
                    {
                        var button = new CommandBarButton();
                        InitializeButtonWithUICommand(button, new StandardUICommand(StandardUICommandKind.Redo), ExecuteRedoCommand);
                        _buttons.Add(TextControlButtons.Redo, button);
                        return button;
                    }

                case TextControlButtons.SelectAll:
                    {
                        var button = new CommandBarButton();
                        InitializeButtonWithUICommand(button, new StandardUICommand(StandardUICommandKind.SelectAll), ExecuteSelectAllCommand);
                        _buttons.Add(TextControlButtons.SelectAll, button);
                        return button;
                    }

                default:
                    throw new NotSupportedException("Invalid TextControlButtons");
            }
        }
    }

    private Dictionary<TextControlButtons, ICommandBarElement> _buttons;
}
