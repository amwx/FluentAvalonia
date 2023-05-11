using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Input;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a button control that can be displayed in a CommandBar
/// </summary>
public partial class CommandBarButton : Button, ICommandBarElement, IStyleable
{
    public CommandBarButton()
    {
        TemplateSettings = new CommandBarButtonTemplateSettings();
    }

    Type IStyleable.StyleKey => typeof(CommandBarButton);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconSourceProperty)
        {
            PseudoClasses.Set(SharedPseudoclasses.s_pcIcon, change.NewValue != null);
            TemplateSettings.Icon = IconHelpers.CreateFromUnknown(change.GetNewValue<IconSource>());
        }
        else if (change.Property == LabelProperty)
        {
            PseudoClasses.Set(SharedPseudoclasses.s_pcLabel, change.NewValue != null);
        }
        else if (change.Property == FlyoutProperty)
        {
            if (change.OldValue is FlyoutBase oldFB)
            {
                oldFB.Closed -= OnFlyoutClosed;
                oldFB.Opened -= OnFlyoutOpened;
            }

            if (change.NewValue is FlyoutBase newFB)
            {
                newFB.Closed += OnFlyoutClosed;
                newFB.Opened += OnFlyoutOpened;

                PseudoClasses.Set(SharedPseudoclasses.s_pcFlyout, true);
                PseudoClasses.Set(s_pcSubmenuOpen, newFB.IsOpen);
            }
            else
            {
                PseudoClasses.Set(SharedPseudoclasses.s_pcFlyout, false);
                PseudoClasses.Set(s_pcSubmenuOpen, false);
            }
        }
        else if (change.Property == HotKeyProperty)
        {
            PseudoClasses.Set(SharedPseudoclasses.s_pcHotkey, change.NewValue != null);
        }
        else if (change.Property == IsCompactProperty)
        {
            PseudoClasses.Set(SharedPseudoclasses.s_pcCompact, change.GetNewValue<bool>());
        }
        else if (change.Property == CommandProperty)
        {
            if (change.OldValue is XamlUICommand xamlComOld)
            {
                if (Label == xamlComOld.Label)
                {
                    Label = null;
                }

                if (HotKey == xamlComOld.HotKey)
                {
                    HotKey = null;
                }

                if (ToolTip.GetTip(this).ToString() == xamlComOld.Description)
                {
                    ToolTip.SetTip(this, null);
                }
            }

            if (change.NewValue is XamlUICommand xamlCom)
            {
                if (string.IsNullOrEmpty(Label))
                {
                    Label = xamlCom.Label;
                }

                IconSource = xamlCom.IconSource;

                if (HotKey == null)
                {
                    HotKey = xamlCom.HotKey;
                }

                if (ToolTip.GetTip(this) == null)
                {
                    ToolTip.SetTip(this, xamlCom.Description);
                }
            }
        }
    }

    protected override void OnClick()
    {
        base.OnClick();
        if (IsInOverflow)
        {
            var cb = this.FindLogicalAncestorOfType<CommandBar>();
            if (cb != null)
            {
                cb.IsOpen = false;
            }
        }
    }

    protected override bool RegisterContentPresenter(IContentPresenter presenter)
    {
        if (presenter.Name == "ContentPresenter")
            return true;

        return base.RegisterContentPresenter(presenter);
    }

    private void OnFlyoutOpened(object sender, EventArgs e)
    {
        PseudoClasses.Set(s_pcSubmenuOpen, true);
    }

    private void OnFlyoutClosed(object sender, EventArgs e)
    {
        PseudoClasses.Set(s_pcSubmenuOpen, false);
    }
}
