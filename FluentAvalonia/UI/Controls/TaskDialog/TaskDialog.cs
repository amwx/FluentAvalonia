using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls.Primitives;
using AvButton = Avalonia.Controls.Button;

namespace FluentAvalonia.UI.Controls
{
    [PseudoClasses(":hosted", ":hidden", ":open")]
    [PseudoClasses(":header", ":subheader", ":icon", ":footer", ":footerAuto", ":expanded")]
    [PseudoClasses(":progress", ":progressError", ":progressSuspend")]
    /// <summary>
    /// Represents and enhanced dialog with enhanced button, command, and progress support
    /// </summary>
    public partial class TaskDialog : ContentControl
    {
        public TaskDialog()
        {
            PseudoClasses.Add(":hidden");
            _buttons = new List<TaskDialogButton>();
            _commands = new List<TaskDialogCommand>();

            AddHandler(AvButton.ClickEvent, OnButtonClick, RoutingStrategies.Bubble, true);
            AddHandler(KeyDownEvent, OnKeyDownPreview, RoutingStrategies.Tunnel, true);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _buttonsHost = e.NameScope.Get<ItemsPresenter>("ButtonsHost");
            _commandsHost = e.NameScope.Get<ItemsPresenter>("CommandsHost");

            _moreDetailsButton = e.NameScope.Find<AvButton>("MoreDetailsButton");

            _progressBar = e.NameScope.Find<ProgressBar>("ProgressBar");

            if (_moreDetailsButton != null)
            {
                _moreDetailsButton.Click += MoreDetailsButtonClick;
            }

            SetButtons();
            SetCommands();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == FooterVisibilityProperty)
            {
                var val = change.GetNewValue<TaskDialogFooterVisibility>();

                PseudoClasses.Set(":footerAuto", val == TaskDialogFooterVisibility.Auto);
                PseudoClasses.Set(":footer", val != TaskDialogFooterVisibility.Never);
                PseudoClasses.Set(":expanded", val == TaskDialogFooterVisibility.Always);
            }
            else if (change.Property == IsFooterExpandedProperty)
            {
                if (FooterVisibility != TaskDialogFooterVisibility.Always)
                    PseudoClasses.Set(":expanded", change.GetNewValue<bool>());
            }
            else if (change.Property == ShowProgressBarProperty)
            {
                PseudoClasses.Set(":progress", change.GetNewValue<bool>());
            }
            else if (change.Property == IconSourceProperty)
            {
                PseudoClasses.Set(":icon", change.NewValue != null);
            }
            else if (change.Property == HeaderProperty)
            {
                PseudoClasses.Set(":header", change.NewValue != null);
            }
            else if (change.Property == SubHeaderProperty)
            {
                PseudoClasses.Set(":subheader", change.NewValue != null);
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
                if (_defaultButton != null && _defaultButton.DataContext is TaskDialogButton b)
                {
                    if (b.Command?.CanExecute(b.CommandParameter) == true)
                    {
                        b.Command.Execute(b.CommandParameter);
                    }

                    b.RaiseClick();

                    if (b is TaskDialogCommand com && !com.ClosesOnInvoked)
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
            bool declaredInXaml = ((IVisual)this).IsAttachedToVisualTree;
            if (!declaredInXaml && XamlRoot == null)
            {
                throw new InvalidOperationException("XamlRoot not set on TaskDialog. This should be set to the TopLevel that should own or host the dialog.");
            }

            OnOpening();

            var owner = XamlRoot ?? VisualRoot;

            void UnparentDialog()
            {
                _xamlOwner = Parent;
                if (_xamlOwner is Panel p)
                {
                    _xamlOwnerChildIndex = p.Children.IndexOf(this);
                    p.Children.RemoveAt(_xamlOwnerChildIndex);
                }
                else if (_xamlOwner is IContentControl icc)
                {
                    icc.Content = null;
                }
                else if (_xamlOwner is IContentPresenter icp)
                {
                    icp.Content = null;
                }
                else if (_xamlOwner is Decorator d)
                {
                    d.Child = null;
                }
            }

            object result = null;

            if (showHosted || !(owner is WindowBase))
            {
                // Hosted in OverlayLayer
                _tcs = new TaskCompletionSource<object>();
                if (declaredInXaml)
                {
                    UnparentDialog();
                }

                var host = new DialogHost
                {
                    Content = this
                };
                _host = host;

                var overlayLayer = OverlayLayer.GetOverlayLayer(owner);
                if (overlayLayer == null)
                    throw new InvalidOperationException("Unable to find OverlayLayer for hosting the TaskDialog");

                overlayLayer.Children.Add(host);
                PseudoClasses.Set(":hosted", true);
                IsVisible = true;

                OnOpened();

                PseudoClasses.Set(":open", true);                
                PseudoClasses.Set(":hidden", false);

                result = await _tcs.Task;
            }
            else
            {
                if (declaredInXaml)
                {
                    UnparentDialog();
                }

                PseudoClasses.Set(":hidden", false);
                var host = new TaskDialogWindowHost(this);
                host[!Window.TitleProperty] = this[!TitleProperty];
                host.Opened += (s, e) =>
                {
                    OnOpened();
                };

                _host = host;
                IsVisible = true;

                result = await host.ShowDialog<object>(owner as Window);
            }

            OnClosed();
            _host = null;
            return result ?? TaskDialogStandardResult.None;
        }

        /// <summary>
        /// Hides the TaskDialog with a <see cref="TaskDialogStandardResult.None"/> result
        /// </summary>
        public void Hide()
        {
            CloseCore(TaskDialogStandardResult.None);
        }

        /// <summary>
        /// Hides the dialog with the specified dialog result
        /// </summary>
        public void Hide(object result)
        {
            CloseCore(result);
        }

        internal void CompleteClosingDeferral(object result) 
        {
            IsEnabled = true;
            _hasDeferralActive = false;
            FinalCloseDialog(result);
        }

        protected virtual void OnOpening()
        {
            Opening?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnOpened()
        {
            Opened?.Invoke(this, EventArgs.Empty);

            _defaultButton?.Focus();
        }

        protected virtual void OnClosing(TaskDialogClosingEventArgs args)
        {
            Closing?.Invoke(this, args);
        }

        protected virtual void OnClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void CloseCore(object result)
        {
            var ea = new TaskDialogClosingEventArgs(this, result);
            OnClosing(ea);

            if (ea.Cancel)
                return;

            if (!ea.IsDeferred)
            {
                FinalCloseDialog(result);
            }
            else
            {
                // Diable interaction with the dialog while the deferral is active
                IsEnabled = false;
                _hasDeferralActive = true;
            }
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
                else if (_xamlOwner is IContentControl icc)
                {
                    icc.Content = this;
                }
                else if (_xamlOwner is IContentPresenter icp)
                {
                    icp.Content = this;
                }
            }

            if (_host is Window w)
            {
                // Hide the window, but don't close it while we shut down
                // Mainly so we can take out the dialog and put it back
                // if it was declared in Xaml and this doesn't show
                // on screen
                w.Hide();
                IsVisible = false;

                w.Content = null;
                ReturnDialogToParent();

                PseudoClasses.Set(":hosted", false);
                PseudoClasses.Set(":hidden", true);

                // Fully close the dialog sending the result back
                w.Close(result);
            }
            else if (_host is DialogHost dh)
            {
                IsHitTestVisible = false;

                Focus();

                PseudoClasses.Set(":open", false);
                PseudoClasses.Set(":hidden", true);

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
            // TaskDialogCommandHost is a TaskDialogButtonHost, this captures everything
            if (e.Source is IVisual v && v.FindAncestorOfType<TaskDialogButtonHost>(true) is TaskDialogButtonHost b)
            {
                // DataContext for the hosts are the user defined buttons/commands, get the dialog from that
                if (b.DataContext is TaskDialogControl tdb)
                {
                    if (tdb is TaskDialogCommand com && !com.ClosesOnInvoked)
                        return;

                    Hide(tdb.DialogResult);
                }
            }
        }

        public void SetProgressBarState(double value, TaskDialogProgressState state)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_progressBar != null)
                {
                    _progressBar.Value = value;

                    _progressBar.IsIndeterminate = (state & TaskDialogProgressState.Indeterminate) == TaskDialogProgressState.Indeterminate;

                    if (_currentProgressState != state)
                    {
                        _currentProgressState = state;

                        PseudoClasses.Set(":progressError", (state & TaskDialogProgressState.Error) == TaskDialogProgressState.Error);
                        PseudoClasses.Set(":progressSuspend", (state & TaskDialogProgressState.Suspended) == TaskDialogProgressState.Suspended);
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
            List<TaskDialogButtonHost> buttons = new List<TaskDialogButtonHost>();
            bool foundDefault = false;
            for (int i = 0; i < _buttons.Count; i++)
            {
                var b = new TaskDialogButtonHost
                {
                    [!ContentProperty] = _buttons[i][!TaskDialogControl.TextProperty],
                    [!TaskDialogButtonHost.IconSourceProperty] = _buttons[i][!TaskDialogButton.IconSourceProperty],
                    DataContext = _buttons[i],
                    [!IsEnabledProperty] = _buttons[i][!TaskDialogControl.IsEnabledProperty],
                    [!AvButton.CommandParameterProperty] = _buttons[i][!TaskDialogButton.CommandParameterProperty],
                    [!AvButton.CommandProperty] = _buttons[i][!TaskDialogButton.CommandProperty]
                };

                if (_buttons[i].IsDefault)
                {
                    if (foundDefault)
                        throw new InvalidOperationException("Cannot set 'IsDefault' property on more than one item in a TaskDialog");

                    foundDefault = true;
                    b.Classes.Add("accent");
                    _defaultButton = b;
                }
                buttons.Add(b);
            }
            _buttonsHost.Items = buttons;            
        }

        private void SetCommands()
        {
            List<Control> commands = new List<Control>();

            bool foundDefault = _defaultButton != null;
            for (int i = 0; i < _commands.Count; i++)
            {
                if (_commands[i] is TaskDialogCheckBox tdcb)
                {
                    var com = new CheckBox
                    {
                        [!ContentProperty] = tdcb[!TaskDialogControl.TextProperty],
                        DataContext = tdcb,
                        [!IsEnabledProperty] = tdcb[!TaskDialogControl.IsEnabledProperty],
                        [!CheckBox.IsCheckedProperty] = tdcb[!TaskDialogRadioButton.IsCheckedProperty]
                    };

                    com.Classes.Add("FA_TaskDialogCommand");

                    commands.Add(com);
                }
                else if (_commands[i] is TaskDialogRadioButton tdrb)
                {
                    var com = new RadioButton
                    {
                        [!ContentProperty] = tdrb[!TaskDialogControl.TextProperty],
                        DataContext = tdrb,
                        [!IsEnabledProperty] = tdrb[!TaskDialogControl.IsEnabledProperty],
                        [!RadioButton.IsCheckedProperty] = tdrb[!TaskDialogRadioButton.IsCheckedProperty]
                    };

                    com.Classes.Add("FA_TaskDialogCommand");

                    commands.Add(com);
                }
                else if (_commands[i] is TaskDialogCommand tdc)
                {
                    var com = new TaskDialogCommandHost
                    {
                        [!ContentProperty] = tdc[!TaskDialogControl.TextProperty],
                        DataContext = tdc,
                        [!IsEnabledProperty] = tdc[!TaskDialogControl.IsEnabledProperty],
                        [!AvButton.CommandParameterProperty] = tdc[!TaskDialogButton.CommandParameterProperty],
                        [!AvButton.CommandProperty] = tdc[!TaskDialogButton.CommandProperty],
                        [!TaskDialogButtonHost.IconSourceProperty] = tdc[!TaskDialogButton.IconSourceProperty]
                    };

                    if (tdc.IsDefault)
                    {
                        if (foundDefault)
                            throw new InvalidOperationException("Cannot set 'IsDefault' property on more than one item in a TaskDialog");

                        foundDefault = true;
                        com.Classes.Add("accent");
                        _defaultButton = com;
                    }

                    commands.Add(com);
                }
            }

            _commandsHost.Items = commands;
        }

        private ItemsPresenter _buttonsHost;
        private ItemsPresenter _commandsHost;
        private ProgressBar _progressBar;
        private AvButton _moreDetailsButton;

        private TaskDialogProgressState _currentProgressState = TaskDialogProgressState.Normal;

        private AvButton _defaultButton;

        public IControl _xamlOwner;
        private int _xamlOwnerChildIndex;
        private IControl _host;
        private TaskCompletionSource<object> _tcs;
        internal bool _hasDeferralActive = false;
    }

    internal class TaskDialogWindowHost : CoreWindow
    {
        public TaskDialogWindowHost(TaskDialog dialog)
        {
            CanResize = false;
            SizeToContent = SizeToContent.WidthAndHeight;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ShowAsDialog = true;

            Content = dialog;
            MinWidth = 100;
            MinHeight = 100;

#if DEBUG
            this.AttachDevTools();
#endif
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // Don't allow closing the window when a Deferral is active
            // Otherwise the deferral task will continue to run, but the 
            // window will be dismissed
            if (Content is TaskDialog td && td._hasDeferralActive)
                e.Cancel = true;
        }
    }
}
