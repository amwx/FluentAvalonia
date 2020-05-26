//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml

using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.Core;
using System;

namespace FluentAvalonia.UI.Controls.Primitives
{
    /// <summary>
    /// Base class for all Flyout objects. Note, FlyoutBase is not
    /// included in WinUI, so this is my own implementation,
    /// expected behavior may not 100% be the same
    /// </summary>
    public class FlyoutBase : AvaloniaObject
    {
        public FlyoutBase()
        {
            InitPopup();
        }
        static FlyoutBase()
        {

        }

        #region AvaloniaProperties
        private static readonly DirectProperty<FlyoutBase, bool> IsOpenProperty =
           AvaloniaProperty.RegisterDirect<FlyoutBase, bool>("IsOpen",
               x => x.IsOpen, (x, v) => x.IsOpen = v);
        public static readonly StyledProperty<Control> TargetProperty =
            AvaloniaProperty.Register<FlyoutBase, Control>("Target");
        public static readonly StyledProperty<FlyoutPlacementMode> FlyoutPlacementProperty =
            AvaloniaProperty.Register<FlyoutBase, FlyoutPlacementMode>("FlyoutPlacement");
        #endregion

        #region CLRProperties

        public bool IsOpen
        {
            get => _IsOpen;
            set
            {
                if (setOpenFromMethod)
                    return;
                if(SetAndRaise(IsOpenProperty, ref _IsOpen, value))
                {
                    if (value)
                        ShowAt(Target);
                    else
                        Hide();
                }
            }
        }

        public Control Target
        {
            get => GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        public FlyoutPlacementMode FlyoutPlacement
        {
            get => GetValue(FlyoutPlacementProperty);
            set => SetValue(FlyoutPlacementProperty, value);
        }

        #endregion

        #region Events

        public event EventHandler<object> Closed;
        public event EventHandler<object> Opened;
        public event EventHandler<object> Opening;
        public event TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs> Closing;

        #endregion

        #region Public Methods

        public void ShowAt(Control control)
        {
            if (control == null && Target == null)
                throw new ArgumentNullException("No target");

            if (FlyoutRoot == null)
                InitPopup();
            OnOpening();

            //TO DO, finish FlyoutPlacement
            switch (FlyoutPlacement)
            {
                case FlyoutPlacementMode.Top:
                    FlyoutRoot.PlacementMode = PlacementMode.Top;
                    break;
                case FlyoutPlacementMode.Right:
                    FlyoutRoot.PlacementMode = PlacementMode.Right;
                    break;
                case FlyoutPlacementMode.Bottom:
                    FlyoutRoot.PlacementMode = PlacementMode.Bottom;
                    break;
                default:
                    FlyoutRoot.PlacementMode = PlacementMode.Top;
                    break;
            }

            FlyoutRoot.PlacementTarget = control == null ? Target : control;

            ((ISetLogicalParent)FlyoutRoot).SetParent(FlyoutRoot.PlacementTarget);

            setOpenFromMethod = true;
            IsOpen = true;
            setOpenFromMethod = false;
            FlyoutRoot.IsOpen = true;

            
        }

        public void Hide()
        {
            FlyoutBaseClosingEventArgs args = new FlyoutBaseClosingEventArgs(false);
            OnClosing(args);
            if (args.Cancel)
                return;
            setOpenFromMethod = true;
            IsOpen = false;
            FlyoutRoot.IsOpen = false;
            setOpenFromMethod = false;
            
        }

        #endregion

        #region Protected Methods

        protected virtual void OnOpened()
        {
            Opened?.Invoke(this, null);
        }
        protected virtual void OnOpening()
        {
            Opening?.Invoke(this, null);
        }
        protected virtual void OnClosing(FlyoutBaseClosingEventArgs args)
        {
            Closing?.Invoke(this, args);
        }
        protected virtual void OnClosed()
        {
            Closed?.Invoke(this, null);
        }

        protected virtual Control CreatePresenter()
        {
            throw new NotImplementedException("FlyoutBase doesn't create a default presenter");
        }

        #endregion

        #region Private Methods

        private void InitPopup()
        {
            FlyoutRoot = new Popup();
            FlyoutRoot.StaysOpen = false;

            FlyoutRoot.Opened += FlyoutRoot_Opened;
            FlyoutRoot.Closed += FlyoutRoot_Closed;
        }

        private void FlyoutRoot_Closed(object sender, Avalonia.Controls.Primitives.PopupClosedEventArgs e)
        {
            //Note if the Popup closes because of lost focus (stayopen=false),
            //This is the only event raised, as there's no current impl for
            //Popup.Closing
            IsOpen = false; //Insurance
            OnClosed();
        }

        private void FlyoutRoot_Opened(object sender, EventArgs e)
        {
            OnOpened();
        }

        #endregion



        private bool setOpenFromMethod;
        private bool _IsOpen;
        protected Popup FlyoutRoot;
        protected Control XamlRoot;
    }
}
