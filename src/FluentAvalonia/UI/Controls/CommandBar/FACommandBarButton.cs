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
/// Represents a templated button control to be displayed in an <see cref="FACommandBar"/>.
/// </summary>
public partial class FACommandBarButton : Button, IFACommandBarElement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FACommandBarButton"/> class.
    /// </summary>
    public FACommandBarButton()
    {
        TemplateSettings = new FACommandBarButtonTemplateSettings();
    }

    protected override Type StyleKeyOverride => typeof(FACommandBarButton);

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconSourceProperty)
        {
            PseudoClasses.Set(FASharedPseudoclasses.s_pcIcon, change.NewValue != null);
            TemplateSettings.Icon = FAIconHelpers.CreateFromUnknown(change.GetNewValue<FAIconSource>());
        }
        else if (change.Property == LabelProperty)
        {
            PseudoClasses.Set(FASharedPseudoclasses.s_pcLabel, change.NewValue != null);
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

                PseudoClasses.Set(FASharedPseudoclasses.s_pcFlyout, true);
                PseudoClasses.Set(s_pcSubmenuOpen, newFB.IsOpen);
            }
            else
            {
                PseudoClasses.Set(FASharedPseudoclasses.s_pcFlyout, false);
                PseudoClasses.Set(s_pcSubmenuOpen, false);
            }
        }
        else if (change.Property == HotKeyProperty)
        {
            PseudoClasses.Set(FASharedPseudoclasses.s_pcHotkey, change.NewValue != null);
        }
        else if (change.Property == IsCompactProperty)
        {
            PseudoClasses.Set(FASharedPseudoclasses.s_pcCompact, change.GetNewValue<bool>());
        }
        else if (change.Property == CommandProperty)
        {
            if (change.OldValue is FAXamlUICommand xamlComOld)
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

            if (change.NewValue is FAXamlUICommand xamlCom)
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
            var cb = this.FindLogicalAncestorOfType<FACommandBar>();
            if (cb != null)
            {
                cb.IsOpen = false;
            }
        }
    }

    protected override bool RegisterContentPresenter(ContentPresenter presenter)
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
