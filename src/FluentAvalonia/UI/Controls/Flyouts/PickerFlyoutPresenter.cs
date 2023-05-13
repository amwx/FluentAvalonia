using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls.Primitives;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// The FlyoutPresenter that is used within a <see cref="PickerFlyoutBase"/>
/// </summary>
[PseudoClasses(s_pcAcceptDismiss)]
[TemplatePart(s_tpAcceptButton, typeof(Button))]
[TemplatePart(s_tpDismissButton, typeof(Button))]
public class PickerFlyoutPresenter : ContentControl
{
    public PickerFlyoutPresenter()
    {
        PseudoClasses.Add(s_pcAcceptDismiss);
    }

    /// <summary>
    /// Raised when the Confirmed button is tapped indicating the new Color should be applied
    /// </summary>
    public event TypedEventHandler<PickerFlyoutPresenter, EventArgs> Confirmed;

    /// <summary>
    /// Raised when the Dismiss button is tapped, indicating the new color should not be applied
    /// </summary>
    public event TypedEventHandler<PickerFlyoutPresenter, EventArgs> Dismissed;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_acceptButton != null)
        {
            _acceptButton.Click -= OnAcceptClick;
        }
        if (_dismissButton != null)
        {
            _dismissButton.Click -= OnDismissClick;
        }

        base.OnApplyTemplate(e);

        _acceptButton = e.NameScope.Find<Button>(s_tpAcceptButton);
        if (_acceptButton != null)
        {
            _acceptButton.Click += OnAcceptClick;
        }
        _dismissButton = e.NameScope.Find<Button>(s_tpDismissButton);
        if (_dismissButton != null)
        {
            _dismissButton.Click += OnDismissClick;
        }
    }

    protected override bool RegisterContentPresenter(IContentPresenter presenter)
    {
        if (presenter.Name == "ContentPresenter")
            return true;

        return base.RegisterContentPresenter(presenter);
    }

    private void OnDismissClick(object sender, RoutedEventArgs e)
    {
        Dismissed?.Invoke(this, EventArgs.Empty);
    }

    private void OnAcceptClick(object sender, RoutedEventArgs e)
    {
        Confirmed?.Invoke(this, EventArgs.Empty);
    }

    internal void ShowHideButtons(bool show)
    {
        PseudoClasses.Set(s_pcAcceptDismiss, show);
    }

    private Button _acceptButton;
    private Button _dismissButton;

    private const string s_pcAcceptDismiss = ":acceptdismiss";
    private const string s_tpAcceptButton = "AcceptButton";
    private const string s_tpDismissButton = "DismissButton";
}
