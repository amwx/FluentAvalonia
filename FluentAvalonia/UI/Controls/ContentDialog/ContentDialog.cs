using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Logging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls;

[PseudoClasses(s_pcHidden, s_pcOpen)]
[PseudoClasses(s_pcPrimary, s_pcSecondary, s_pcClose)]
[PseudoClasses(s_pcFullSize)]
[TemplatePart(s_tpPrimaryButton, typeof(Button))]
[TemplatePart(s_tpSecondaryButton, typeof(Button))]
[TemplatePart(s_tpCloseButton, typeof(Button))]
/// <summary>
/// Presents a asyncronous dialog to the user.
/// </summary>
public partial class ContentDialog : ContentControl, ICustomKeyboardNavigation
{
    public ContentDialog()
    {
        PseudoClasses.Add(s_pcHidden);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_primaryButton != null)
            _primaryButton.Click -= OnButtonClick;
        if (_secondaryButton != null)
            _secondaryButton.Click -= OnButtonClick;
        if (_closeButton != null)
            _closeButton.Click -= OnButtonClick;

        base.OnApplyTemplate(e);

        _primaryButton = e.NameScope.Get<Button>(s_tpPrimaryButton);
        _primaryButton.Click += OnButtonClick;
        _secondaryButton = e.NameScope.Get<Button>(s_tpSecondaryButton);
        _secondaryButton.Click += OnButtonClick;
        _closeButton = e.NameScope.Get<Button>(s_tpCloseButton);
        _closeButton.Click += OnButtonClick;

        // v2- Removed this as I don't think its necessary anymore (called from ShowAsync)
        //SetupDialog();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FullSizeDesiredProperty)
        {
            OnFullSizedDesiredChanged(change);
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Handled)
        {
            base.OnKeyUp(e);
            return;
        }

        switch (e.Key)
        {
            case Key.Escape:
                HideCore();
                e.Handled = true;
                break;

            case Key.Enter:
                var defButton = DefaultButton;

                // v2 - Only handle 'Enter' if the default button is set
                //      Otherwise, we'll let the event go as normal - if focus is currently
                //      on a button, 'Enter' should invoke that button
                if (defButton != ContentDialogButton.None)
                {
                    switch (defButton)
                    {
                        case ContentDialogButton.Primary:
                            OnButtonClick(_primaryButton, null);
                            break;

                        case ContentDialogButton.Secondary:
                            OnButtonClick(_secondaryButton, null);
                            break;

                        case ContentDialogButton.Close:
                            OnButtonClick(_closeButton, null);
                            break;
                    }
                    e.Handled = true;
                }

                break;
        }
        base.OnKeyUp(e);
    }

    public async Task<ContentDialogResult> ShowAsync() => await ShowAsyncCore(null);
    public async Task<ContentDialogResult> ShowAsync(Window w) => await ShowAsyncCore(w);

    /// <summary>
    /// Shows the content dialog on the specified window asynchronously.
    /// </summary>
    /// <remarks>
    /// Note that the placement parameter is not implemented and only accepts <see cref="ContentDialogPlacement.Popup"/>
    /// </remarks>
    private async Task<ContentDialogResult> ShowAsyncCore(Window window, ContentDialogPlacement placement = ContentDialogPlacement.Popup)
    {
        if (placement == ContentDialogPlacement.InPlace)
            throw new NotImplementedException("InPlace not implemented yet");
        _tcs = new TaskCompletionSource<ContentDialogResult>();

        OnOpening();

        if (Parent != null)
        {
            _originalHost = Parent;
            switch (_originalHost)
            {
                case Panel p:
                    _originalHostIndex = p.Children.IndexOf(this);
                    p.Children.Remove(this);
                    break;
                case Decorator d:
                    d.Child = null;
                    break;
                case IContentControl cc:
                    cc.Content = null;
                    break;
                case IContentPresenter cp:
                    cp.Content = null;
                    break;
            }
        }

        _host ??= new DialogHost();

        _host.Content = this;

        _lastFocus = FocusManager.Instance.Current;

        OverlayLayer ol = null;

        if (window != null)
        {
            ol = OverlayLayer.GetOverlayLayer(window);
        }
        else
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime al)
            {
                foreach (var item in al.Windows)
                {
                    if (item.IsActive)
                    {
                        window = item;
                        break;
                    }
                }

                //Fallback, just in case
                window ??= al.MainWindow;

                ol = OverlayLayer.GetOverlayLayer(window);
            }
            else if (Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime sl)
            {
                ol = OverlayLayer.GetOverlayLayer(sl.MainView);
            }
        }

        if (ol == null)
            throw new InvalidOperationException();

        ol.Children.Add(_host);

        // v2 - Added this so dialog materializes in the Visual Tree now since for some reason
        //      items in the OverlayLayer materialize at the absolute last moment making init
        //      a very difficult task to do
        (ol.GetVisualRoot() as ILayoutRoot).LayoutManager.ExecuteInitialLayoutPass();

        IsVisible = true;
        ShowCore();
        SetupDialog();
        return await _tcs.Task;
    }

    /// <summary>
    /// Closes the current <see cref="ContentDialog"/> without a result (<see cref="ContentDialogResult"/>.<see cref="ContentDialogResult.None"/>)
    /// </summary>
    public void Hide() => Hide(ContentDialogResult.None);

    /// <summary>
    /// Closes the current <see cref="ContentDialog"/> with the given <see cref="ContentDialogResult"/> <para>ddd</para>
    /// </summary>
    /// <param name="dialogResult">The <see cref="ContentDialogResult"/> to return</param>
    public void Hide(ContentDialogResult dialogResult)
    {
        _result = dialogResult;
        HideCore();
    }

    /// <summary>
    /// Called when the primary button is invoked
    /// </summary>
    protected virtual void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
    {
        PrimaryButtonClick?.Invoke(this, args);
    }

    /// <summary>
    /// Called when the secondary button is invoked
    /// </summary>
    protected virtual void OnSecondaryButtonClick(ContentDialogButtonClickEventArgs args)
    {
        SecondaryButtonClick?.Invoke(this, args);
    }

    /// <summary>
    /// Called when the close button is invoked
    /// </summary>
    protected virtual void OnCloseButtonClick(ContentDialogButtonClickEventArgs args)
    {
        CloseButtonClick?.Invoke(this, args);
    }

    /// <summary>
    /// Called when the ContentDialog is requested to be opened
    /// </summary>
    protected virtual void OnOpening()
    {
        Opening?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called after the ContentDialog is initialized but just before its presented on screen
    /// </summary>
    protected virtual void OnOpened()
    {
        Opened?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the ContentDialog has been requested to close, but before it actually closes
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnClosing(ContentDialogClosingEventArgs args)
    {
        Closing?.Invoke(this, args);
    }

    /// <summary>
    /// Called when the ContentDialog has been closed and removed from the tree
    /// </summary>
    protected virtual void OnClosed(ContentDialogClosedEventArgs args)
    {
        Closed?.Invoke(this, args);
    }

    private void ShowCore()
    {
        IsVisible = true;
        PseudoClasses.Set(s_pcHidden, false);
        PseudoClasses.Set(s_pcOpen, true);

        OnOpened();
    }

    private void HideCore()
    {
        // v2 - No longer disabling the dialog during a deferral so we need to make sure that if
        //      multiple requests to close come in, we don't handle them
        if (_hasDeferralActive)
            return;

        // v2- Changed to match logic in TeachingTip for deferral, fixing #239 where cancel
        //     was being handled before the deferral.
        var args = new ContentDialogClosingEventArgs(_result);

        var deferral = new Deferral(() =>
        {
            Dispatcher.UIThread.VerifyAccess();
            _hasDeferralActive = false;

            if (!args.Cancel)
            {
                FinalCloseDialog();
            }
        });

        args.SetDeferral(deferral);
        _hasDeferralActive = true;

        args.IncrementDeferralCount();
        OnClosing(args);
        args.DecrementDeferralCount();
    }

    // Internal only for UnitTests
    internal void SetupDialog()
    {
        if (_primaryButton == null)
            ApplyTemplate();

        PseudoClasses.Set(s_pcPrimary, !string.IsNullOrEmpty(PrimaryButtonText));
        PseudoClasses.Set(s_pcSecondary, !string.IsNullOrEmpty(SecondaryButtonText));
        PseudoClasses.Set(s_pcClose, !string.IsNullOrEmpty(CloseButtonText));

        var curFocus = FocusManager.Instance.Current;
        bool setFocus = false;
        if (curFocus.FindAncestorOfType<ContentDialog>() == null)
        {
            // Only set the focus if user didn't handle doing that in Opened handler,
            // since this is called after
            setFocus = true;
        }

        var p = Presenter;
        switch (DefaultButton)
        {
            case ContentDialogButton.Primary:
                if (!_primaryButton.IsVisible)
                    break;

                _primaryButton.Classes.Add(s_cAccent);
                _secondaryButton.Classes.Remove(s_cAccent);
                _closeButton.Classes.Remove(s_cAccent);

                if (setFocus)
                {
                    FocusManager.Instance.Focus(_primaryButton);
#if DEBUG
                    Logger.TryGet(LogEventLevel.Debug, "ContentDialog")?.Log("SetupDialog", "Set initial focus to PrimaryButton");
#endif
                }

                break;

            case ContentDialogButton.Secondary:
                if (!_secondaryButton.IsVisible)
                    break;

                _secondaryButton.Classes.Add(s_cAccent);
                _primaryButton.Classes.Remove(s_cAccent);
                _closeButton.Classes.Remove(s_cAccent);

                if (setFocus)
                {
                    FocusManager.Instance.Focus(_secondaryButton);
#if DEBUG
                    Logger.TryGet(LogEventLevel.Debug, "ContentDialog")?.Log("SetupDialog", "Set initial focus to SecondaryButton");
#endif
                }

                break;

            case ContentDialogButton.Close:
                if (!_closeButton.IsVisible)
                    break;

                _closeButton.Classes.Add(s_cAccent);
                _primaryButton.Classes.Remove(s_cAccent);
                _secondaryButton.Classes.Remove(s_cAccent);

                if (setFocus)
                {
                    FocusManager.Instance.Focus(_closeButton);
#if DEBUG
                    Logger.TryGet(LogEventLevel.Debug, "ContentDialog")?.Log("SetupDialog", "Set initial focus to CloseButton");
#endif
                }

                break;

            default:
                _closeButton.Classes.Remove(s_cAccent);
                _primaryButton.Classes.Remove(s_cAccent);
                _secondaryButton.Classes.Remove(s_cAccent);

                if (setFocus)
                {
                    var next = KeyboardNavigationHandler.GetNext(this, NavigationDirection.Next);
                    if (next != null)
                    {
                        FocusManager.Instance.Focus(next);
                    }
                    else
                    {
                        FocusManager.Instance.Focus(this);
                    }

#if DEBUG
                    Logger.TryGet(LogEventLevel.Debug, "ContentDialog")?.Log("SetupDialog", "Set initial focus to {next}", next);
#endif
                }

                break;
        }
    }

    // This is the exit point for the ContentDialog
    // This method MUST be called to finalize everything
    private async void FinalCloseDialog()
    {
        // Prevent interaction when closing...double/mutliple clicking on the buttons to close
        // the dialog was calling this multiple times, which would cause the OverlayLayer check
        // below to fail (as this would be removed from the tree). This is a simple workaround
        // to make sure we don't error out
        IsHitTestVisible = false;

        // For a better experience when animating closed, we need to make sure the
        // focus adorner is not showing (if using keyboard) otherwise that will hang
        // around and not fade out and it just looks weird. So focus this to force the
        // adorner to hide, then continue forward.
        Focus();

        PseudoClasses.Set(s_pcHidden, true);
        PseudoClasses.Set(s_pcOpen, false);

        // Let the close animation finish (now 0.167s in new WinUI update...)
        // We'll wait just a touch longer to be sure
        await Task.Delay(200);

        OnClosed(new ContentDialogClosedEventArgs(_result));

        if (_lastFocus != null)
        {
            FocusManager.Instance.Focus(_lastFocus, NavigationMethod.Unspecified);
            _lastFocus = null;
        }

        var ol = OverlayLayer.GetOverlayLayer(_host);
        // If OverlayLayer isn't found here, this may be a reentrant call (hit ESC multiple times quickly, etc)
        // Don't fail, and return. If this isn't reentrant, there's bigger issues...
        if (ol == null)
            return;

        ol.Children.Remove(_host);

        _host.Content = null;

        if (_originalHost != null)
        {
            if (_originalHost is Panel p)
            {
                p.Children.Insert(_originalHostIndex, this);
            }
            else if (_originalHost is Decorator d)
            {
                d.Child = this;
            }
            else if (_originalHost is IContentControl cc)
            {
                cc.Content = this;
            }
            else if (_originalHost is IContentPresenter cp)
            {
                cp.Content = this;
            }
        }

        _tcs.TrySetResult(_result);
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        // v2 - No longer disabling the dialog during a deferral so we need to make sure that if
        //      multiple requests to close come in, we don't handle them
        if (_hasDeferralActive)
            return;

        var args = new ContentDialogButtonClickEventArgs();

        var deferral = new Deferral(() =>
        {
            Dispatcher.UIThread.VerifyAccess();
            _hasDeferralActive = false;

            if (args.Cancel)
                return;

            if (sender == _primaryButton)
            {
                if (PrimaryButtonCommand != null && PrimaryButtonCommand.CanExecute(PrimaryButtonCommandParameter))
                {
                    PrimaryButtonCommand.Execute(PrimaryButtonCommandParameter);
                }
                _result = ContentDialogResult.Primary;
            }
            else if (sender == _secondaryButton)
            {
                if (SecondaryButtonCommand != null && SecondaryButtonCommand.CanExecute(SecondaryButtonCommandParameter))
                {
                    SecondaryButtonCommand.Execute(SecondaryButtonCommandParameter);
                }
                _result = ContentDialogResult.Secondary;
            }
            else if (sender == _closeButton)
            {
                if (CloseButtonCommand != null && CloseButtonCommand.CanExecute(CloseButtonCommandParameter))
                {
                    CloseButtonCommand.Execute(CloseButtonCommandParameter);
                }
                _result = ContentDialogResult.None;
            }

            HideCore();
        });

        args.SetDeferral(deferral);
        _hasDeferralActive = true;

        args.IncrementDeferralCount();
        if (sender == _primaryButton)
        {
            OnPrimaryButtonClick(args);
        }
        else if (sender == _secondaryButton)
        {
            OnSecondaryButtonClick(args);
        }
        else if (sender == _closeButton)
        {
            OnCloseButtonClick(args);
        }
        args.DecrementDeferralCount();
    }

    private void OnFullSizedDesiredChanged(AvaloniaPropertyChangedEventArgs e)
    {
        bool newVal = (bool)e.NewValue;
        PseudoClasses.Set(s_pcFullSize, newVal);
    }

    public (bool handled, IInputElement next) GetNext(IInputElement element, NavigationDirection direction)
    {
        var children = this.GetVisualDescendants().OfType<IInputElement>()
            .Where(x => KeyboardNavigation.GetIsTabStop((InputElement)x) && x.Focusable &&
            x.IsEffectivelyVisible && IsEffectivelyEnabled).ToList();

        if (children.Count == 0)
            return (false, null);

        var current = FocusManager.Instance?.Current;
        if (current == null)
            return (false, null);

        if (direction == NavigationDirection.Next)
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == current)
                {
                    if (i == children.Count - 1)
                    {
                        return (true, children[0]);
                    }
                    else
                    {
                        return (true, children[i + 1]);
                    }
                }
            }
        }
        else if (direction == NavigationDirection.Previous)
        {
            for (int i = children.Count - 1; i >= 0; i--)
            {
                if (children[i] == current)
                {
                    if (i == 0)
                    {
                        return (true, children[children.Count - 1]);
                    }
                    else
                    {
                        return (true, children[i - 1]);
                    }
                }
            }
        }

        return (false, null);
    }


    // Store the last element focused before showing the dialog, so we can
    // restore it when it closes
    private IInputElement _lastFocus;
    private IControl _originalHost;
    private int _originalHostIndex;
    private DialogHost _host;
    private ContentDialogResult _result;
    private TaskCompletionSource<ContentDialogResult> _tcs;
    private Button _primaryButton;
    private Button _secondaryButton;
    private Button _closeButton;
    private bool _hasDeferralActive;

    private const string s_tpPrimaryButton = "PrimaryButton";
    private const string s_tpSecondaryButton = "SecondaryButton";
    private const string s_tpCloseButton = "CloseButton";

    internal const string s_cAccent = "accent"; // Internal for TaskDialog
    private const string s_pcOpen = ":open";
    private const string s_pcHidden = ":hidden";

    private const string s_pcPrimary = ":primary";
    private const string s_pcSecondary = ":secondary";
    private const string s_pcClose = ":close";
    private const string s_pcFullSize = ":fullsize";
}
