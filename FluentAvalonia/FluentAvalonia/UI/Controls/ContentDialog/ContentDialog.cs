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
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FluentAvalonia.UI.Controls
{
    
    /// <summary>
    /// Presents a asyncronous dialog to the user.
    /// Note, Tab navigation currently doesn't work
    /// </summary>
    public class ContentDialog : ContentControl
    {
        public ContentDialog()
        {
            PseudoClasses.Set(":hidden", true);
            IsVisible = false;
            AddHandler(Control.KeyUpEvent, OnDialogKeyUp, RoutingStrategies.Bubble);
        }

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

        public event TypedEventHandler<ContentDialog, ContentDialogClosedEventArgs> Closed;
        public event TypedEventHandler<ContentDialog, ContentDialogClosingEventArgs> Closing;
        //Change here, ContentDialogOpenedEventArgs is empty (at least publically), so just use null object
        public event TypedEventHandler<ContentDialog, object> Opened;
        public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> PrimaryButtonClick;
        public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> SecondaryButtonClick;
        public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> CloseButtonClick;

        #endregion

        #region Override Methods

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            PrimaryButton = e.NameScope.Find<Button>("PrimaryButton");
            SecondaryButton = e.NameScope.Find<Button>("SecondaryButton");
            CloseButton = e.NameScope.Find<Button>("CloseButton");
                       
            PrimaryButton.Click += Button_Click;
            SecondaryButton.Click += Button_Click;
            CloseButton.Click += Button_Click;
                       
            SetupDialog();
        }

        #endregion

        #region Public Methods

        public async Task<ContentDialogResult> ShowAsync()
        {
            return await ShowAsync(ContentDialogPlacement.Popup);
        }

        public async Task<ContentDialogResult> ShowAsync(ContentDialogPlacement placement)
        {
            //There are several steps to getting a ContentDialog to work properly
            //When using the "popup" placement, we need to find the active window of the application
            //or fallback to the main window. Once we have the host window, the ContentDialog
            //is injected into the xaml root. Basically, we'll walk down the visual tree until
            //we find a control that allows multiple children (i.e. Grid or Panel, etc.)
            //if using the default window template, a Panel is the first template child, 
            //Also, this will be above the VisualLayerManager, and will also be above any
            //adorners.
            //Running the dialog async will keep the window responsive (you can still resize, move,
            //process events in the background etc.) Part of the template is a semi-transparent background
            //that fills window & prevents the user from interacting with the content behind.
            //If user wants placement "InPlace", then the dialog is placed where the user placed it in
            //their xaml code. It still runs async, but the rest of the window isn't blocked
            
            //We also need check if a Parent exists, and we want popup mode. In which case we need to store the
            //original parent, move it to the top level, show it, then replace it when we close.

            tcs = new TaskCompletionSource<ContentDialogResult>();
            

            if(placement == ContentDialogPlacement.InPlace)
            {
                //We'll check if its InPlace mode first. We check to see if we have a 
                //Parent. If we do, great, we don't do anything, just show the dialog
                //If not, we fall back to popup mode
                if(Parent != null)
                {
                    //If we reuse the dialog, make sure we reset it
                    if (PrimaryButton != null)
                    {
                        SetupDialog();
                    }

                    PseudoClasses.Set(":hidden", false);
                    PseudoClasses.Set(":open", true);

                    OnOpened(null);

                    this.Focus();

                    return await tcs.Task;
                }
            }


            if(placement == ContentDialogPlacement.Popup)
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime al)
                {
                    //Find the active window
                    Window activeWindow = null;
                    foreach (var item in al.Windows)
                    {
                        if (item.IsActive)
                            activeWindow = item;
                        break;
                    }
                    //Assumes MainWindow != null
                    if (activeWindow == null)
                    {
                        activeWindow = al.MainWindow;
                    }
                    host = activeWindow;
                }
                else
                    throw new NotSupportedException("Only supports IClassicDesktopStyleApplicationLifetime");

                //Now we need to find the first container supporting multiple children in the window
                //We don't know the template of the Window, so we find the first Panel or Grid, since they
                //Allow multiple visual children, we need to do this AFTER the VisualLayerManager, so that
                //we can still draw focus adorners
                //The Window template provided with FluentAvalonia, wraps the window's ContentPresenter in a 
                //panel to allow us an injection point. 
                //If another template is used, we'll have to alter it to make it work:
                //>>>Check the VisualLayerManager's child
                //>>>If its a grid or panel, great, we'll just use that (same as our template)
                //>>>If its not, we'll create a Panel, and make a slight alteration to the template
                //NOTE: Custom window frames: This may also cover Caption Buttons & titlebar
                //If this happens, and you don't want it, you should be able to set the Top Margin of the dialog
                //to your titlebar height & prevent it from covering it:
                //<Style Selector="ui|ContentDialog">
                //  <Setter Property="Margin" Value="0 30 0 0" />
                //</Style>
                var vlm = (VisualLayerManager)host.GetVisualDescendants().FirstOrDefault(x => x is VisualLayerManager);
                var child = vlm.Child;

                //Before we add it, we need to make sure we don't already have a parent.
                if(Parent != null)
                {
                    //Since we don't know what the Parent Type is, we manually check & hope it works...
                    if(Parent is Panel prntP)
                    {
                        originalHostIndex = prntP.Children.IndexOf(this);
                        prntP.Children.Remove(this);
                        originalHost = prntP;
                    }
                    else if (Parent is Grid prntG)
                    {
                        originalHostIndex = prntG.Children.IndexOf(this);
                        prntG.Children.Remove(this);
                        originalHost = prntG;
                    }
                    else if(Parent is Decorator d)
                    {
                        d.Child = null;
                        originalHost = d;
                    }
                    else if(Parent is ContentControl cp)
                    {
                        cp.Content = null;
                        originalHost = cp;
                    }
                    else if (Parent is ContentPresenter cpp)
                    {
                        cpp.Content = null;
                        originalHost = cpp;
                    }
                }


                if(child is Panel p)
                {
                    RootPanel = p;
                    RootPanel.Children.Add(this);
                }
                else if (child is Grid g)
                {
                    RootGrid = g;
                    RootGrid.Children.Add(this);
                }
                else
                {
                    Panel cont = new Panel();
                    vlm.Child = cont;
                    cont.Children.Add(child);
                    RootPanel = cont;
                    RootPanel.Children.Add(this);
                }
                
                //If we reuse the dialog, make sure we reset it
                if(PrimaryButton != null)
                {
                    SetupDialog(); 
                }

                IsVisible = true;
                PseudoClasses.Set(":hidden", false);
                PseudoClasses.Set(":open", true);

                OnOpened(null);

                this.Focus();

                return await tcs.Task;

            }
            throw new Exception("Something went wrong with showing the Dialog");
        }

        #endregion

        #region protected methods

        protected virtual void OnOpened(object args)
        {
            Opened?.Invoke(this, args);
        }

        protected virtual void OnClosing(ContentDialogClosingEventArgs args)
        {
            Closing?.Invoke(this, args);
        }

        protected virtual void OnClosed(ContentDialogClosedEventArgs args)
        {
            Closed?.Invoke(this, args);
        }

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


        #endregion

        #region Private Methods

        private void SetupDialog()
        {
            if (PrimaryButton == null | SecondaryButton == null | CloseButton == null)
                throw new Exception("Can't find Dialog Buttons");

            Classes.Clear();//Clear any classes set, incase we're reusing it
            
            //Only show the button if its text has been set
            bool hasPrimary = true, hasSecondary = true, hasClose = true;
            if(PrimaryButtonText == "" || string.IsNullOrEmpty(PrimaryButtonText) || string.IsNullOrWhiteSpace(PrimaryButtonText))
            {
                hasPrimary = false;
            }
            if (SecondaryButtonText == "" || string.IsNullOrEmpty(SecondaryButtonText) || string.IsNullOrWhiteSpace(SecondaryButtonText))
            {
                hasSecondary = false;
            }
            if (CloseButtonText == "" || string.IsNullOrEmpty(CloseButtonText) || string.IsNullOrWhiteSpace(CloseButtonText))
            {
                hasClose = false;
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


            //The default button, we apply an Accent style to
            var defButton = DefaultButton;
            switch (defButton)
            {
                case ContentDialogButton.Primary:
                    if (!hasPrimary)
                        break;
                    PrimaryButton.Classes.Add("Accent");
                    SecondaryButton.Classes.Remove("Accent");
                    CloseButton.Classes.Remove("Accent");
                    break;
                case ContentDialogButton.Secondary:
                    if (!hasSecondary)
                        break;
                    PrimaryButton.Classes.Remove("Accent");
                    SecondaryButton.Classes.Add("Accent");
                    CloseButton.Classes.Remove("Accent");
                    break;
                case ContentDialogButton.Close:
                    if (!hasClose)
                        break;
                    PrimaryButton.Classes.Remove("Accent");
                    SecondaryButton.Classes.Remove("Accent");
                    CloseButton.Classes.Add("Accent");
                    break;
                default:
                    PrimaryButton.Classes.Remove("Accent");
                    SecondaryButton.Classes.Remove("Accent");
                    CloseButton.Classes.Remove("Accent");
                    break;
            }

            if (hasPrimary)
                PrimaryButton.Focus();
            else if (hasSecondary)
                SecondaryButton.Focus();
            else if (hasClose)
                CloseButton.Focus();
            else
                this.Focus();

        }

        /// <summary>
        /// Handles closing of the Dialog, usually through the buttons,
        /// but all exit points go through here
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == CloseButton)
            {
                result = ContentDialogResult.None;
            }
            else if (sender == PrimaryButton)
            {
                result = ContentDialogResult.Primary;
            }
            else if (sender == SecondaryButton)
            {
                result = ContentDialogResult.Secondary;
            }

            //Fire the ButtonClick events first
            //Note, The ContentDialogDeferral is not sourced,
            //so this is my own impl, based on observed behavior
            ContentDialogButtonClickEventArgs buttonArgs = new ContentDialogButtonClickEventArgs(false, this); 
            switch (result)
            {
                case ContentDialogResult.Primary:
                    PrimaryButtonClick?.Invoke(this, buttonArgs);
                    if (buttonArgs.Cancel)
                        return;

                    //Wait for the user to run their async code
                    while(deferral != null)
                    {
                        await Task.Delay(1);
                    }
                    break;
                case ContentDialogResult.Secondary:
                    SecondaryButtonClick?.Invoke(this, buttonArgs);
                    if (buttonArgs.Cancel)
                        return;

                    //Wait for the user to run their async code
                    while (deferral != null)
                    {
                        await Task.Delay(1);
                    }
                    break;
                case ContentDialogResult.None:
                    CloseButtonClick?.Invoke(this, buttonArgs);
                    if (buttonArgs.Cancel)
                        return;

                    //Wait for the user to run their async code
                    while (deferral != null)
                    {
                        await Task.Delay(1);
                    }
                    break;
            }

            //Now we can close the dialog
            ContentDialogClosingEventArgs args = new ContentDialogClosingEventArgs(false, result);
            //Alert that we're closing
            OnClosing(args);

            //If user canceled the closing, return
            if (args.Cancel)
                return;

            PseudoClasses.Set("open", false);
            PseudoClasses.Set(":hidden", true);

            //Finally set the result to return to normal...
            tcs.TrySetResult(result);

            //We await here to let any animations for fade out to complete
            //before removing the dialog from the parent container (if in Popup mode)
            //Animation is 0.5s in total
            await Task.Delay(500);

            //Remove the dialog from the grid
            if (RootPanel != null)
            {
                RootPanel.Children.Remove(this);
            }
            else if (RootGrid != null)
            {
                RootGrid.Children.Remove(this);
            }

            //If we originally had a parent, restore us back
            if(originalHost != null)
            {
                //Since we don't know what the Parent Type is, we manually check & hope it works...
                //Hopefully no changes to the UI were made while the dialog was showing other than
                //what we do to show the dialog, since we insert it back in its original index
                if (Parent is Panel prntP)
                {
                    prntP.Children.Insert(originalHostIndex, this);
                }
                else if (Parent is Grid prntG)
                {
                    prntG.Children.Insert(originalHostIndex, this);
                }
                else if (Parent is Decorator d)
                {
                    d.Child = this;
                }
                else if (Parent is ContentControl cp)
                {
                    cp.Content = this;
                }
                else if (Parent is ContentPresenter cpp)
                {
                    cpp.Content = this;
                }
                originalHost = null;
                originalHostIndex = -1;
                IsVisible = false;
            }

        }

        internal ContentDialogButtonClickDeferral GetButtonClickDeferral()
        {
            deferral = new ContentDialogButtonClickDeferral(this);
            IsEnabled = false; //Disable dialog when user has deferral
            return deferral;
        }

        internal void CompleteButtonClickDeferral()
        {
            deferral = null;
            IsEnabled = true;
        }

        private void OnFullSizedDesiredChanged(AvaloniaPropertyChangedEventArgs e)
        {
            bool newVal = (bool)e.NewValue;
            PseudoClasses.Set(":fullsize", newVal);
        }


        private void OnDialogKeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                //Cancel the dialog by using the close button,
                //We don't use the RoutedEventArgs param on Button_Click
                //So we can safely pass null here
                Button_Click(CloseButton, null);
                e.Handled = true;
            }
        }

        #endregion


        private ContentDialogButtonClickDeferral deferral;
        private ContentDialogResult result;
        private TaskCompletionSource<ContentDialogResult> tcs;
        private Grid RootGrid;
        private Panel RootPanel;
        private Button PrimaryButton;
        private Button SecondaryButton;
        private Button CloseButton;
        private Window host;
        private IControl originalHost;
        private int originalHostIndex = -1;
    }
}
