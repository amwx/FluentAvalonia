using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls.Primitives;
using FluentAvalonia.UI.Windowing;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents and enhanced dialog with enhanced button, command, and progress support
/// </summary>
public partial class FATaskDialog : ContentControl
{
    public FATaskDialog()
    {
        PseudoClasses.Add(s_pcHidden);
        _buttons = new List<FATaskDialogButton>();
        _commands = new List<FATaskDialogCommand>();

        AddHandler(Button.ClickEvent, OnButtonClick, RoutingStrategies.Bubble, true);
        AddHandler(KeyDownEvent, OnKeyDownPreview, RoutingStrategies.Tunnel, true);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_moreDetailsButton != null)
        {
            _moreDetailsButton.Click -= MoreDetailsButtonClick;
        }

        base.OnApplyTemplate(e);

        _buttonsHost = e.NameScope.Get<ItemsControl>(s_tpButtonsHost);
        _commandsHost = e.NameScope.Get<ItemsControl>(s_tpCommandsHost);

        _moreDetailsButton = e.NameScope.Find<Button>(s_tpMoreDetailsButton);

        _progressBar = e.NameScope.Find<ProgressBar>(s_tpProgressBar);

        if (_moreDetailsButton != null)
        {
            _moreDetailsButton.Click += MoreDetailsButtonClick;
        }        
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FooterVisibilityProperty)
        {
            var val = change.GetNewValue<FATaskDialogFooterVisibility>();

            PseudoClasses.Set(s_pcFooterAuto, val == FATaskDialogFooterVisibility.Auto);
            PseudoClasses.Set(s_pcFooter, val != FATaskDialogFooterVisibility.Never);
            PseudoClasses.Set(s_pcExpanded, val == FATaskDialogFooterVisibility.Always);
        }
        else if (change.Property == IsFooterExpandedProperty)
        {
            if (FooterVisibility != FATaskDialogFooterVisibility.Always)
                PseudoClasses.Set(s_pcExpanded, change.GetNewValue<bool>());
        }
        else if (change.Property == ShowProgressBarProperty)
        {
            PseudoClasses.Set(s_pcProgress, change.GetNewValue<bool>());
        }
        else if (change.Property == IconSourceProperty)
        {
            PseudoClasses.Set(FASharedPseudoclasses.s_pcIcon, change.NewValue != null);
        }
        else if (change.Property == HeaderProperty)
        {
            PseudoClasses.Set(FASharedPseudoclasses.s_pcHeader, change.NewValue != null);
        }
        else if (change.Property == SubHeaderProperty)
        {
            PseudoClasses.Set(s_pcSubheader, change.NewValue != null);
        }
        else if (change.Property == HeaderForegroundProperty)
        {
            PseudoClasses.Set(s_pcHeaderForeground, change.NewValue != null);
        }
        else if (change.Property == IconForegroundProperty)
        {
            PseudoClasses.Set(s_pcIconForeground, change.NewValue != null);
        }
    }
    
    protected override bool RegisterContentPresenter(ContentPresenter presenter)
    {
        if (presenter.Name == "ContentPresenter")
            return true;

        return base.RegisterContentPresenter(presenter);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // GH #539
        // If TaskDialog is declared in Xaml, OnLoaded will get called when the Xaml is loaded, which is not 
        // desirable (particularly the focus part). Only run this code and set focus if we're opening
        if (_isOpening)
        {
            SetButtons();
            SetCommands();
            TrySetInitialFocus();
        }       
    }

    private void OnKeyDownPreview(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Hide();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            if (_defaultButton != null && _defaultButton.DataContext is FATaskDialogButton b)
            {
                if (b.Command?.CanExecute(b.CommandParameter) == true)
                {
                    b.Command.Execute(b.CommandParameter);
                }

                b.RaiseClick();

                if (b is FATaskDialogCommand com && !com.ClosesOnInvoked)
                    return;

                CloseCore(b.DialogResult);
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Shows the TaskDialog
    /// </summary>
    /// <param name="showHosted">Optional parameter that specifies whether this dialog should show in the OverlayLayer even on windowing platforms. Defaults to false</param>
    /// <returns>The TaskDialog result corresponding to the command/button used to close the dialog</returns>
    /// <remarks>
    /// Before calling this method, you MUST set <see cref="XamlRoot"/> property to the TopLevel/Window that should
    /// own or host this Dialog. If you declare the dialog in Xaml, this is done automatically since 
    /// the dialog is already attached to the visual tree
    /// </remarks>
    public async Task<object> ShowAsync(bool showHosted = false)
    {
        bool declaredInXaml = this.IsAttachedToVisualTree();
        if (!declaredInXaml && XamlRoot == null)
        {
            throw new InvalidOperationException("XamlRoot not set on TaskDialog. This should be set to the TopLevel that should own or host the dialog.");
        }

        // See OnLoaded
        _isOpening = true;

        OnOpening();
        
        var owner = XamlRoot ?? VisualRoot as Visual;

        void UnparentDialog()
        {
            _xamlOwner = (Control)Parent;
            if (_xamlOwner is Panel p)
            {
                _xamlOwnerChildIndex = p.Children.IndexOf(this);
                p.Children.RemoveAt(_xamlOwnerChildIndex);
            }
            else if (_xamlOwner is ContentControl icc)
            {
                icc.Content = null;
            }
            else if (_xamlOwner is ContentPresenter icp)
            {
                icp.Content = null;
            }
            else if (_xamlOwner is Decorator d)
            {
                d.Child = null;
            }
        }

        object result = null;
        _previousFocus = TopLevel.GetTopLevel(owner).FocusManager.GetFocusedElement();

        if (showHosted || !(owner is WindowBase))
        {
            // Hosted in OverlayLayer
            _tcs = new TaskCompletionSource<object>();
            if (declaredInXaml)
            {
                UnparentDialog();
            }

            var host = new FADialogHost
            {
                Content = this
            };
            _host = host;

            var overlayLayer = OverlayLayer.GetOverlayLayer(owner);
            if (overlayLayer == null)
                throw new InvalidOperationException("Unable to find OverlayLayer for hosting the TaskDialog");

            overlayLayer.Children.Add(host);
            PseudoClasses.Set(s_pcHosted, true);
            IsVisible = true;

            // v2 - Added this so dialog materializes in the Visual Tree now since for some reason
            //      items in the OverlayLayer materialize at the absolute last moment making init
            //      a very difficult task to do
            // v2-preview6: This doesn't appear necessary anymore...will preserve this for now
            // but has to be removed to solve GH#315
            //(overlayLayer.GetVisualRoot() as ILayoutRoot).LayoutManager.ExecuteInitialLayoutPass();

            OnOpened();

            PseudoClasses.Set(FASharedPseudoclasses.s_pcOpen, true);
            PseudoClasses.Set(s_pcHidden, false);

            result = await _tcs.Task;
        }
        else
        {
            if (declaredInXaml)
            {
                UnparentDialog();
            }

            PseudoClasses.Set(s_pcHidden, false);
            PseudoClasses.Set(s_pcHosted, false);

            var host = new AppWindow()
            {
                CanResize = false,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowAsDialog = true,
                Content = this,
                MinWidth = 100,
                MinHeight = 100
            };

            if (_host == null)
            {
                host[!Window.TitleProperty] = this[!TitleProperty];
                host.Opened += (s, e) =>
                {
                    OnOpened();

                    TrySetInitialFocus();
                };
                host.Closing += (s, e) =>
                {
                    if (_ignoreWindowClosingEvent)
                        return;

                    // Cancel the window event now, and we'll use our normal closing logic to determine
                    // if we should actually cancel
                    e.Cancel = true;
                    CloseCore(FATaskDialogStandardResult.None);
                };
            }            
            
            _host = host;
            IsVisible = true;

            result = await host.ShowDialog<object>(owner as Window);
        }

        _isOpening = false;
        OnClosed();
        _host = null;

        _previousFocus?.Focus();

        return result ?? FATaskDialogStandardResult.None;
    }

    /// <summary>
    /// Hides the TaskDialog with a <see cref="FATaskDialogStandardResult.None"/> result
    /// </summary>
    public void Hide()
    {
        CloseCore(FATaskDialogStandardResult.None);
    }

    /// <summary>
    /// Hides the dialog with the specified dialog result
    /// </summary>
    public void Hide(object result)
    {
        CloseCore(result);
    }

    protected virtual void OnOpening()
    {
        Opening?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnOpened()
    {
        Opened?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnClosing(FATaskDialogClosingEventArgs args)
    {
        Closing?.Invoke(this, args);
    }

    protected virtual void OnClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }

    private void CloseCore(object result)
    {
        if (_hasDeferralActive)
            return;

        var args = new FATaskDialogClosingEventArgs(result);

        var deferral = new FADeferral(() =>
        {
            Dispatcher.UIThread.VerifyAccess();
            _hasDeferralActive = false;
            if (args.Cancel)
                return;

            FinalCloseDialog(result);
        });

        args.SetDeferral(deferral);

        _hasDeferralActive = true;
        args.IncrementDeferralCount();
        OnClosing(args);
        args.DecrementDeferralCount();
    }

    private async void FinalCloseDialog(object result)
    {
        void ReturnDialogToParent()
        {
            if (_xamlOwner == null)
                return;

            if (_xamlOwner is Panel p)
            {
                p.Children.Insert(_xamlOwnerChildIndex, this);
            }
            else if (_xamlOwner is Decorator d)
            {
                d.Child = this;
            }
            else if (_xamlOwner is ContentControl icc)
            {
                icc.Content = this;
            }
            else if (_xamlOwner is ContentPresenter icp)
            {
                icp.Content = this;
            }
        }

        if (_host is Window w)
        {            
            _ignoreWindowClosingEvent = true;

            w.Close(result);
            IsVisible = false;

            w.Content = null;
            ReturnDialogToParent();

            PseudoClasses.Set(FASharedPseudoclasses.s_pcOpen, false);
            PseudoClasses.Set(s_pcHidden, true);

            _ignoreWindowClosingEvent = false;
        }
        else if (_host is FADialogHost dh)
        {
            IsHitTestVisible = false;

            Focus();

            PseudoClasses.Set(FASharedPseudoclasses.s_pcOpen, false);
            PseudoClasses.Set(s_pcHidden, true);

            // Let the close animation finish (now 0.167s in new WinUI update...)
            // We'll wait just a touch longer to be sure
            await Task.Delay(200);

            IsHitTestVisible = true;
            IsVisible = false;

            dh.Content = null;
            ReturnDialogToParent();

            var overlayLayer = OverlayLayer.GetOverlayLayer(dh);
            // If OverlayLayer isn't found here, this may be a reentrant call (hit ESC multiple times quickly, etc)
            // Don't fail, and return. If this isn't reentrant, there's bigger issues...
            if (overlayLayer == null)
                return;

            overlayLayer.Children.Remove(dh);

            _tcs.TrySetResult(result);
        }
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        if (_hasDeferralActive)
            return;

        // TaskDialogCommandHost is a TaskDialogButtonHost, this captures everything
        if (e.Source is Visual v && v.FindAncestorOfType<FATaskDialogButtonHost>(true) is FATaskDialogButtonHost b)
        {
            // DataContext for the hosts are the user defined buttons/commands, get the dialog from that
            if (b.DataContext is FATaskDialogControl tdb)
            {
                if (tdb is FATaskDialogCommand com && !com.ClosesOnInvoked)
                    return;

                Hide(tdb.DialogResult);
            }
        }
    }

    public void SetProgressBarState(double value, FATaskDialogProgressState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_progressBar != null)
            {
                _progressBar.Value = value;

                _progressBar.IsIndeterminate = (state & FATaskDialogProgressState.Indeterminate) == FATaskDialogProgressState.Indeterminate;

                if (_currentProgressState != state)
                {
                    _currentProgressState = state;

                    PseudoClasses.Set(s_pcProgressError, (state & FATaskDialogProgressState.Error) == FATaskDialogProgressState.Error);
                    PseudoClasses.Set(s_pcProgressSuspend, (state & FATaskDialogProgressState.Suspended) == FATaskDialogProgressState.Suspended);
                }
            }
        });
    }

    private void MoreDetailsButtonClick(object sender, RoutedEventArgs e)
    {
        IsFooterExpanded = !IsFooterExpanded;
    }

    private void SetButtons()
    {
        if (_buttons == null)
            return;

        List<FATaskDialogButtonHost> buttons = new List<FATaskDialogButtonHost>();
        bool foundDefault = false;
        for (int i = 0; i < _buttons.Count; i++)
        {
            var button = _buttons[i];
            var b = new FATaskDialogButtonHost
            {
                [!ContentProperty] = _buttons[i][!FATaskDialogControl.TextProperty],
                [!FATaskDialogButtonHost.IconSourceProperty] = button[!FATaskDialogButton.IconSourceProperty],
                DataContext = button,
                [!IsEnabledProperty] = button[!FATaskDialogControl.IsEnabledProperty],
                [!Button.CommandParameterProperty] = button[!FATaskDialogButton.CommandParameterProperty],
                [!Button.CommandProperty] = button[!FATaskDialogButton.CommandProperty]
            };

            if (button.IsDefault)
            {
                if (foundDefault)
                    throw new InvalidOperationException("Cannot set 'IsDefault' property on more than one item in a TaskDialog");

                foundDefault = true;
                b.Classes.Add(FASharedPseudoclasses.s_cAccent);
                _defaultButton = b;
            }
            buttons.Add(b);
        }

        _buttonsHost.ItemsSource = buttons;
    }

    private void SetCommands()
    {
        if (_commands == null)
            return;

        List<Control> commands = new List<Control>();

        bool foundDefault = _defaultButton != null;
        int iconCount = 0;
        int normalCommandCount = 0;
        for (int i = 0; i < _commands.Count; i++)
        {
            if (_commands[i] is FATaskDialogCheckBox tdcb)
            {
                var com = new CheckBox
                {
                    [!ContentProperty] = tdcb[!FATaskDialogControl.TextProperty],
                    DataContext = tdcb,
                    [!IsEnabledProperty] = tdcb[!FATaskDialogControl.IsEnabledProperty],
                    [!ToggleButton.IsCheckedProperty] = tdcb[!FATaskDialogRadioButton.IsCheckedProperty]
                };

                com.Classes.Add(s_cFATDCom);

                commands.Add(com);                
            }
            else if (_commands[i] is FATaskDialogRadioButton tdrb)
            {
                var com = new RadioButton
                {
                    [!ContentProperty] = tdrb[!FATaskDialogControl.TextProperty],
                    DataContext = tdrb,
                    [!IsEnabledProperty] = tdrb[!FATaskDialogControl.IsEnabledProperty],
                    [!ToggleButton.IsCheckedProperty] = tdrb[!FATaskDialogRadioButton.IsCheckedProperty]
                };

                com.Classes.Add(s_cFATDCom);

                commands.Add(com);
            }
            else if (_commands[i] is FATaskDialogCommand tdc)
            {
                var com = new FATaskDialogCommandHost
                {
                    [!ContentProperty] = tdc[!FATaskDialogControl.TextProperty],
                    DataContext = tdc,
                    [!IsEnabledProperty] = tdc[!FATaskDialogControl.IsEnabledProperty],
                    [!Button.CommandParameterProperty] = tdc[!FATaskDialogButton.CommandParameterProperty],
                    [!Button.CommandProperty] = tdc[!FATaskDialogButton.CommandProperty],
                    [!FATaskDialogButtonHost.IconSourceProperty] = tdc[!FATaskDialogButton.IconSourceProperty]
                };

                if (tdc.IsDefault)
                {
                    if (foundDefault)
                        throw new InvalidOperationException("Cannot set 'IsDefault' property on more than one item in a TaskDialog");

                    foundDefault = true;
                    com.Classes.Add(FASharedPseudoclasses.s_cAccent);
                    _defaultButton = com;
                }

                commands.Add(com);

                // Icons are only supported on "normal" TaskDialogCommands
                if (tdc.IconSource != null)
                    iconCount++;
                normalCommandCount++;
            }
        }

        if (iconCount != normalCommandCount)
        {
            // We have an item with no icon - force it to display as if one
            // was present so that its aligned with the others
            for (int i = 0; i < commands.Count; i++)
            {
                (commands[i].Classes as IPseudoClasses).Set(FASharedPseudoclasses.s_pcIcon, true);
            }
        }

        _commandsHost.ItemsSource = commands;
    }

    private void TrySetInitialFocus()
    {
        var curFocus = TopLevel.GetTopLevel(this).FocusManager.GetFocusedElement() as Control;
        bool setFocus = false;
        if (curFocus?.FindAncestorOfType<FATaskDialog>() == null)
        {
            setFocus = true;
        }

        // User requested something to be focused, don't override their choice
        if (!setFocus)
            return;

        // Default button gets priority focus
        if (_defaultButton != null)
        {
            _defaultButton.Focus();
#if DEBUG
            Logger.TryGet(LogEventLevel.Debug, "TaskDialog")?.Log("TrySetInitialFocus", "Set initial focus to requested DefaultButton");
#endif
        }
        else
        {
            var fm = TopLevel.GetTopLevel(this).FocusManager;
            // TODO: v3 - does this work?
            var next = FocusManager.FindFirstFocusableElement(this);
            if (next != null)
            {
                next.Focus();
            }
            else
            {
                this.Focus();
            }

#if DEBUG
            Logger.TryGet(LogEventLevel.Debug, "TaskDialog")?.Log("TrySetInitialFocus", "Set initial focus to {next}", next);
#endif
        }
    }

    private ItemsControl _buttonsHost;
    private ItemsControl _commandsHost;
    private ProgressBar _progressBar;
    private Button _moreDetailsButton;

    private FATaskDialogProgressState _currentProgressState = FATaskDialogProgressState.Normal;

    private Button _defaultButton;

    public Control _xamlOwner;
    private int _xamlOwnerChildIndex;
    private Control _host;
    private TaskCompletionSource<object> _tcs;
    internal bool _hasDeferralActive = false;

    private IInputElement _previousFocus;
    private bool _ignoreWindowClosingEvent;
    private bool _isOpening;
}
