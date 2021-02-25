//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using FluentAvalonia.Core;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FluentAvalonia.UI.Controls
{
    
    /// <summary>
    /// Presents a asyncronous dialog to the user.
    /// Note, Tab navigation currently doesn't work
    /// </summary>
    public class ContentDialog : ContentControl//, INavigableContainer
    {
        static ContentDialog()
        {
            FullSizeDesiredProperty.Changed.AddClassHandler<ContentDialog>((x, v) => x.OnFullSizedDesiredChanged(v));
        }

        #region AvaloniaProperties

        public static readonly StyledProperty<ICommand> CloseButtonCommandProperty =
            AvaloniaProperty.Register<ContentDialog, ICommand>("CloseButtonCommand");

        public static readonly StyledProperty<object> CloseButtonCommandParameterProperty =
            AvaloniaProperty.Register<ContentDialog, object>("CloseButtonCommandParameter");

        public static readonly StyledProperty<string> CloseButtonTextProperty =
            AvaloniaProperty.Register<ContentDialog, string>("CloseButtonText");

        public static readonly StyledProperty<ContentDialogButton> DefaultButtonProperty =
            AvaloniaProperty.Register<ContentDialog, ContentDialogButton>("DefaultButton", ContentDialogButton.None);

        public static readonly StyledProperty<bool> IsPrimaryButtonEnabledProperty =
            AvaloniaProperty.Register<ContentDialog, bool>("IsPrimaryButtonEnabled", true);

        public static readonly StyledProperty<bool> IsSecondaryButtonEnabledProperty =
            AvaloniaProperty.Register<ContentDialog, bool>("IsSecondaryButtonEnabled", true);

        public static readonly StyledProperty<ICommand> PrimaryButtonCommandProperty =
            AvaloniaProperty.Register<ContentDialog, ICommand>("PrimaryButtonCommand");

        public static readonly StyledProperty<object> PrimaryButtonCommandParameterProperty =
            AvaloniaProperty.Register<ContentDialog, object>("PrimaryButtonCommandParameter");

        public static readonly StyledProperty<string> PrimaryButtonTextProperty =
            AvaloniaProperty.Register<ContentDialog, string>("PrimaryButtonText");

        public static readonly StyledProperty<ICommand> SecondaryButtonCommandProperty =
            AvaloniaProperty.Register<ContentDialog, ICommand>("SecondaryButtonCommand");

        public static readonly StyledProperty<object> SecondaryButtonCommandParameterProperty =
            AvaloniaProperty.Register<ContentDialog, object>("SecondaryButtonCommandParameter");

        public static readonly StyledProperty<string> SecondaryButtonTextProperty =
            AvaloniaProperty.Register<ContentDialog, string>("SecondaryButtonText");

        public static readonly StyledProperty<object> TitleProperty =
            AvaloniaProperty.Register<ContentDialog, object>("Title", "");

        public static readonly StyledProperty<IDataTemplate> TitleTemplateProperty =
            AvaloniaProperty.Register<ContentDialog, IDataTemplate>("TitleTemplate");

        public static readonly StyledProperty<bool> FullSizeDesiredProperty =
            AvaloniaProperty.Register<ContentDialog, bool>("FullSizeDesired");

        #endregion

        #region CLR Properties

        /// <summary>
        /// Command to execute when the close button is clicked
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
        /// CommandParameter for the close button
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
        /// Close Button Text
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
        /// Sets which button is the Default, closing the dialog when enter/space is pressed
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
        /// Gets or sets primary button enabled
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
        /// Gets or sets secondary button enabled
        /// </summary>
        public bool IsSecondaryButtonEnabled
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
        /// Command for the Primary Button
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
        /// CommandParamter for the PrimaryButton
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
        /// Primary Button Text
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
        /// Command for the secondary button
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
        /// CommandParameter for the Secondary button
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
        /// SecondaryButtonText
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
        /// Gets or sets the Dialog Title
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
        /// Gets or sets the template for the Title
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

        #endregion

        #region Events 

        public event TypedEventHandler<ContentDialog, object> Opening;
        public event TypedEventHandler<ContentDialog, object> Opened;
        public event TypedEventHandler<ContentDialog, ContentDialogClosingEventArgs> Closing;
        public event TypedEventHandler<ContentDialog, ContentDialogClosedEventArgs> Closed;
        public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> PrimaryButtonClick;
        public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> SecondaryButtonClick;
        public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> CloseButtonClick;

        #endregion

        #region Override Methods

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _primaryButton = e.NameScope.Get<Button>("PrimaryButton");
            _primaryButton.Click += OnButtonClick;
            _secondaryButton = e.NameScope.Get<Button>("SecondaryButton");
            _secondaryButton.Click += OnButtonClick;
            _closeButton = e.NameScope.Get<Button>("CloseButton");
            _closeButton.Click += OnButtonClick;
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
                    var curFocus = FocusManager.Instance?.Current;
                    if (curFocus != null)
                    {
                        if (curFocus == _primaryButton)
                        {
                            OnButtonClick(_primaryButton, null);
                        }
                        else if (curFocus == _secondaryButton)
                        {
                            OnButtonClick(_secondaryButton, null);
                        }
                        else if (curFocus == _closeButton)
                        {
                            OnButtonClick(_closeButton, null);
                        }
                        else if (Content is IControl c && c.Focusable && c.IsFocused)
                        {
                            //Assume primary button is "OK"
                            OnButtonClick(_primaryButton, null);
                        }
                        e.Handled = true;
                    }
                    break;
            }
            base.OnKeyUp(e);
        }

        #endregion

        #region Public Methods

        public async Task<ContentDialogResult> ShowAsync(ContentDialogPlacement placement = ContentDialogPlacement.Popup)
        {
            tcs = new TaskCompletionSource<ContentDialogResult>();

            OnOpening();

            //Fall back to popup mode if not in tree when launched
            if(placement == ContentDialogPlacement.InPlace && Parent != null)
            {
                IsVisible = true;
            }
            else
            {
                if(Parent != null)
                {
                    //If declared in xaml, we'll need to temporarily remove the dialog from the tree
                    //to show it, then place it back...
                    _originalHost = Parent;
                    if(_originalHost is Panel p)
                    {
                        _originalHostIndex = p.Children.IndexOf(this);
                        p.Children.Remove(this);
                    }
                    else if(_originalHost is Decorator d)
                    {
                        d.Child = null;
                    }
                    else if(_originalHost is IContentControl cc)
                    {
                        cc.Content = null;
                    }
                }

                if(Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime al)
                {
                    Window activeWindow = null;
                    foreach(var item in al.Windows)
                    {
                        if (item.IsActive)
                        {
                            activeWindow = item;
                            break;
                        }
                    }

                    //Fallback, just in case
                    if (activeWindow == null)
                        activeWindow = al.MainWindow;

                    //Because of the differences between UWP/WinUI/WinRT ContentDialog needs to function
                    //and be built slighly differently to accomodate
                    //WinUI's PopupRoot is built into the Window & has a more direct connection thus allowing
                    //the ContentDialog to host the dialog & the "smoke screen" background that dims the 
                    //window contents
                    //Avalonia does not have this, so we need to adapt (INVESTIGATING)
                    //1 - ContentDialog will launch in its own window (was going to use a popup, but you can't
                    //    easily manipulate size/location of Popup, we we'll use its own Window
                    //    The downside, is that the main window will still deactivate when focus is placed in the 
                    //    Dialog, but the main window will still be interactable in terms of moving & resizing, like WinUI
                    //2 - We'll subscribe to the owner window so that as it resizes/moves around/minimizes/etc, the dialog
                    //    will also follow suit

                   
                    Window w = new Window();
                    w.Background = null; //Don't waste by drawing background
                    w.SystemDecorations = SystemDecorations.None;
                    w.ShowInTaskbar = false;
                    w.TransparencyLevelHint = WindowTransparencyLevel.Transparent;
                    w.Content = this;
                    this.IsVisible = true;
                    w.Topmost = true;
                    w.ShowActivated = false;
                    

                    
                    //Make it an owned window, but don't show as dialog to keep this async
                    w.Show(activeWindow);

                    SetupDialog();

                    w.Position = activeWindow.PointToScreen(new Point(0, 0));
                    w.Width = activeWindow.Width;
                    w.Height = activeWindow.Height;

                    w.KeyDown += (s, e) =>
                    {
                        if (e.Key == Key.Escape && ((e.KeyModifiers & KeyModifiers.Alt) == KeyModifiers.Alt))
                        {
                            e.Handled = false;
                        }
                    };

                    //TODO: dispose these...
                    activeWindow.PlatformImpl.PositionChanged += (pt) =>
                    {
                        w.Position = activeWindow.PointToScreen(new Point(0, 0)); 
                    };                    
                    activeWindow.GetObservable(BoundsProperty).Subscribe(s =>
                    {
                        w.Width = s.Width;
                        w.Height = s.Height;
                        w.Position = activeWindow.PointToScreen(new Point(0, 0));
                    });
                    activeWindow.Closed += (s, e) =>
                    {
                        w?.Close();
                    };

                }
                else if(Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime sl)
                {
                    //Have MainView property here, need to figure out how to work with that...
                    throw new NotSupportedException("Only supports classic desktop mode right now");
                }
                else if (Application.Current.ApplicationLifetime is IControlledApplicationLifetime cl)
                {
                    //Not sure how this works...
                    throw new NotSupportedException("Only supports classic desktop mode right now");
                }
            }

            ShowCore();

            return await tcs.Task;
        }

        internal void CompleteButtonClickDeferral()
        {
            IsEnabled = true;
            HideCore();
        }

        internal void CompleteClosingDeferral()
        {
            IsEnabled = true;
            HideCore();
        }

        #endregion

        #region Protected Methods

        protected virtual void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
        {
            PrimaryButtonClick?.Invoke(this, args);
        }

        protected virtual void OnSecondaryButtonClick(ContentDialogButtonClickEventArgs args)
        {
            SecondaryButtonClick?.Invoke(this, args);
        }

        protected virtual void OnCloseButtonClick(ContentDialogButtonClickEventArgs args)
        {
            CloseButtonClick?.Invoke(this, args);
        }

        protected virtual void OnOpening()
        {
            Opening?.Invoke(this, null);
        }

        protected virtual void OnOpened() 
        {
            Opened?.Invoke(this, null);
        }

        protected virtual void OnClosing(ContentDialogClosingEventArgs args)
        {
            Closing?.Invoke(this, args);
        }

        protected virtual void OnClosed(ContentDialogClosedEventArgs args)
        {
            Closed?.Invoke(this, args);
        }

        #endregion

        #region Private Methods

        private void ShowCore()
        {
            IsVisible = true;
            PseudoClasses.Set(":hidden", false);
            PseudoClasses.Set(":open", true);

            OnOpened();
        }

        private void HideCore()
        {
            var ea = new ContentDialogClosingEventArgs(this, result);
            OnClosing(ea);

            if (ea.Cancel)
                return;

            if (!ea.IsDeferred)
            {
                FinalCloseDialog();
            }
            else
            {
                IsEnabled = false;
            }
        }


        private void SetupDialog()
        {
            if (_primaryButton == null)
                ApplyTemplate();

            Classes.Clear();

            bool hasPrimary = false, hasSecondary = false, hasClose = false;
            if(!string.IsNullOrEmpty(PrimaryButtonText))
            {
                hasPrimary = true;
            }
            if (!string.IsNullOrEmpty(SecondaryButtonText))
            {
                hasSecondary = true;
            }
            if (!string.IsNullOrEmpty(CloseButtonText))
            {
                hasClose = true;
            }

            if (hasPrimary && hasSecondary && hasClose)
            {
                PseudoClasses.Set(":primarysecondaryclose", true);
            }
            else if (!hasPrimary && !hasSecondary && !hasClose)
            {
                PseudoClasses.Set(":nobuttons", true);
            }
            else if (hasPrimary && hasSecondary && !hasClose)
            {
                PseudoClasses.Set(":primarysecondary", true);
            }
            else if (hasPrimary && hasClose && !hasSecondary)
            {
                PseudoClasses.Set(":primaryclose", true);
            }
            else if (hasSecondary && hasClose && !hasPrimary)
            {
                PseudoClasses.Set(":secondaryclose", true);
            }
            else if (hasPrimary)
            {
                PseudoClasses.Set(":primaryonly", true);
            }
            else if (hasSecondary)
            {
                PseudoClasses.Set(":secondaryonly", true);
            }
            else if (hasClose)
            {
                PseudoClasses.Set(":closeonly", true);
            }


            PseudoClasses.Set(":nobuttons", !hasPrimary && !hasSecondary && !hasClose);


            switch (DefaultButton)
            {
                case ContentDialogButton.Primary:
                    if (!hasPrimary)
                        break;

                    _primaryButton.Classes.Add("accent");
                    _secondaryButton.Classes.Remove("accent");
                    _closeButton.Classes.Remove("accent");
                    if (Content is IControl cp && cp.Focusable)
                    {
                        cp.Focus();
                    }
                    else
                    {
                        _primaryButton.Focus();
                    }
                    
                    break;

                case ContentDialogButton.Secondary:
                    if (!hasSecondary)
                        break;

                    _secondaryButton.Classes.Add("accent");
                    _primaryButton.Classes.Remove("accent");
                    _closeButton.Classes.Remove("accent");
                    if (Content is IControl cs && cs.Focusable)
                    {
                        cs.Focus();
                    }
                    else
                    {
                        _secondaryButton.Focus();
                    }
                    
                    break;

                case ContentDialogButton.Close:
                    if (!hasClose)
                        break;

                    _closeButton.Classes.Add("accent");
                    _primaryButton.Classes.Remove("accent");
                    _secondaryButton.Classes.Remove("accent");
                    if (Content is IControl cc && cc.Focusable)
                    {
                        cc.Focus();
                    }
                    else
                    {
                        _closeButton.Focus();
                    }
                    
                    break;

                default:
                    _closeButton.Classes.Remove("accent");
                    _primaryButton.Classes.Remove("accent");
                    _secondaryButton.Classes.Remove("accent");

                    if (Content is IControl cd && cd.Focusable)
                    {
                        cd.Focus();
                    }
                    else if (hasPrimary)
                    {
                        _primaryButton.Focus();
                    }
                    else if (hasSecondary)
                    {
                        _secondaryButton.Focus();
                    }
                    else if(hasClose)
                    {
                        _closeButton.Focus();
                    }
                    else
                    {
                        Focus();
                    }

                    break;
            }

        }

        //This is the exit point for the ContentDialog
        //This method MUST be called to finalize everything
        private async void FinalCloseDialog()
        {
            PseudoClasses.Set(":hidden", true);
            PseudoClasses.Set(":open", false);

            //Let the close animation finish
            await Task.Delay(500);

            IsVisible = false;

            //Close the host window
            if(this.VisualRoot is Window w && w.Content == this)
            {
                w.Close();
                w.Content = null;
            }

            if(_originalHost != null)
            {
                if(_originalHost is Panel p)
                {
                    p.Children.Insert(_originalHostIndex, this);
                }
                else if(_originalHost is Decorator d)
                {
                    d.Child = this;
                }
                else if(_originalHost is IContentControl cc)
                {
                    cc.Content = this;
                }
            }

            OnClosed(new ContentDialogClosedEventArgs(result));
            tcs.TrySetResult(result);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if(sender == _primaryButton)
            {
                HandlePrimaryClick();
            }
            else if(sender == _secondaryButton)
            {
                HandleSecondaryClick();
            }
            else if(sender == _closeButton)
            {
                HandleCloseClick();
            }
        }

        private void HandlePrimaryClick() 
        {
            var ea = new ContentDialogButtonClickEventArgs(this);
            OnPrimaryButtonClick(ea);

            if (ea.Cancel)
                return;

            result = ContentDialogResult.Primary;
            if (!ea.IsDeferred)
            {
                if(PrimaryButtonCommand != null && PrimaryButtonCommand.CanExecute(PrimaryButtonCommandParameter))
                {
                    PrimaryButtonCommand.Execute(PrimaryButtonCommandParameter);                    
                }
                HideCore();
            }
            else
            {
                IsEnabled = false;
            }
        }

        private void HandleSecondaryClick() 
        {
            var ea = new ContentDialogButtonClickEventArgs(this);
            OnSecondaryButtonClick(ea);

            if (ea.Cancel)
                return;

            result = ContentDialogResult.Secondary;
            if (!ea.IsDeferred)
            {
                if (SecondaryButtonCommand != null && SecondaryButtonCommand.CanExecute(SecondaryButtonCommandParameter))
                {
                    SecondaryButtonCommand.Execute(SecondaryButtonCommandParameter);
                }
                HideCore();
            }
            else
            {
                IsEnabled = false;
            }
        }

        private void HandleCloseClick() 
        {
            var ea = new ContentDialogButtonClickEventArgs(this);
            OnCloseButtonClick(ea);

            if (ea.Cancel)
                return;

            result = ContentDialogResult.None;
            if (!ea.IsDeferred)
            {
                if (CloseButtonCommand != null && CloseButtonCommand.CanExecute(CloseButtonCommandParameter))
                {
                    CloseButtonCommand.Execute(CloseButtonCommandParameter);
                }
                HideCore();
            }
            else
            {
                IsEnabled = false;
            }
        }

        private void OnFullSizedDesiredChanged(AvaloniaPropertyChangedEventArgs e)
        {
            bool newVal = (bool)e.NewValue;
            PseudoClasses.Set(":fullsize", newVal);
        }

        #endregion

        //private Window _activeWindow;
        private IControl _originalHost;
        private int _originalHostIndex;
        //private Popup _host;
        private ContentDialogResult result;
        private TaskCompletionSource<ContentDialogResult> tcs;
        private Button _primaryButton;
        private Button _secondaryButton;
        private Button _closeButton;
    }

    //internal class ContentDialogManager
    //{
    //    public static ContentDialogManager Instance { get; private set; }

    //    private Dictionary<IControl, Queue<ContentDialog>> ActiveDialogs { get; } = new Dictionary<IControl, Queue<ContentDialog>>();

    //    static ContentDialogManager()
    //    {
    //        Instance = new ContentDialogManager();
    //    }

    //    public void RegisterAndShowDialog(IControl topLevelHost, ContentDialog dialog)
    //    {

    //    }

    //    public void AddDialog(IControl topLevelHost, ContentDialog dialog)
    //    {
    //        if (ActiveDialogs.ContainsKey(topLevelHost))
    //        {
    //            ActiveDialogs[topLevelHost].Enqueue(dialog);
    //        }
    //        else
    //        {
    //            ActiveDialogs.Add(topLevelHost, new Queue<ContentDialog>());
    //            ActiveDialogs[topLevelHost].Enqueue(dialog);
    //        }
    //    }

    //    public void ClearDialog(IControl topLevelHost)
    //    {

    //    }
    //}
}
