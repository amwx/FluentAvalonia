using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls.Primitives
{
    public abstract class FlyoutBase : AvaloniaObject
    {
        #region Avalonia Properties 

        private static readonly DirectProperty<FlyoutBase, bool> IsOpenProperty =
           AvaloniaProperty.RegisterDirect<FlyoutBase, bool>("IsOpen",
               x => x.IsOpen, (x, v) => x.IsOpen = v);

        public static readonly DirectProperty<FlyoutBase, Control> TargetProperty =
            AvaloniaProperty.RegisterDirect<FlyoutBase, Control>("Target", x => x.Target);

        public static readonly DirectProperty<FlyoutBase, FlyoutPlacementMode> PlacementProperty =
            AvaloniaProperty.RegisterDirect<FlyoutBase, FlyoutPlacementMode>("Placement",
                x => x.Placement, (x,v) => x.Placement = v);

        public static readonly DirectProperty<FlyoutBase, bool> AreOpenCloseAnimationsEnabledProperty =
            AvaloniaProperty.RegisterDirect<FlyoutBase, bool>("AreOpenCloseAnimationsEnabled",
                x => x.AreOpenCloseAnimationsEnabled, (x, v) => x.AreOpenCloseAnimationsEnabled = v);

        public static readonly DirectProperty<FlyoutBase, bool> IsConstrainedToRootBoundsProperty =
            AvaloniaProperty.RegisterDirect<FlyoutBase, bool>("IsConstrainedToRootBounds",
                x => x.IsConstrainedToRootBounds, (x, v) => x.IsConstrainedToRootBounds = v);

        public static readonly DirectProperty<FlyoutBase, FlyoutShowMode> ShowModeProperty =
            AvaloniaProperty.RegisterDirect<FlyoutBase, FlyoutShowMode>("ShowMode",
                x => x.ShowMode, (x, v) => x.ShowMode = v);

        public static readonly DirectProperty<FlyoutBase, bool> IsThemeShadowDisabledProperty =
            AvaloniaProperty.RegisterDirect<FlyoutBase, bool>("IsThemeShadowDisabled",
                x => x.IsThemeShadowDisabled, (x, v) => x.IsThemeShadowDisabled = v);

        public static readonly StyledProperty<bool> OverlayDismissEventPassThroughProperty =
            AvaloniaProperty.Register<FlyoutBase, bool>(nameof(OverlayDismissEventPassThrough));

        public static readonly DirectProperty<FlyoutBase, IInputElement> OverlayInputPassThroughElementProperty =
            AvaloniaProperty.RegisterDirect<FlyoutBase, IInputElement>(
                nameof(OverlayInputPassThroughElement),
                o => o.OverlayInputPassThroughElement,
                (o, v) => o.OverlayInputPassThroughElement = v);

        #endregion

        #region CLR Properties

        //public string Name { get; set; }

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (_ignoreIsOpen)
                    return;
                if (SetAndRaise(IsOpenProperty, ref _isOpen, value))
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
            get => _target;
            protected set
            {
                if(_target != value)
                {
                    var old = _target;
                    _target = value;
                    RaisePropertyChanged(TargetProperty, old, value);
                }
            }
        }

        public FlyoutPlacementMode Placement
        {
            get => _placementMode;
            set => SetAndRaise(PlacementProperty, ref _placementMode, value);
        }

        //NOT IMPLEMENTED (ANIMATIONS DON'T WORK)
        public bool AreOpenCloseAnimationsEnabled
        {
            get => _areOpenCloseAnimationsEnabled;
            set => SetAndRaise(AreOpenCloseAnimationsEnabledProperty, ref _areOpenCloseAnimationsEnabled, value);
        }

        //NOT IMPLEMENTED
        public bool IsConstrainedToRootBounds
        {
            get => _isConstrainedToRootBounds;
            set => SetAndRaise(IsConstrainedToRootBoundsProperty, ref _isConstrainedToRootBounds, value);
        }

        public FlyoutShowMode ShowMode
        {
            get => _showMode;
            set => SetAndRaise(ShowModeProperty, ref _showMode, value);
        }

        public bool IsThemeShadowDisabled
        {
            get => _disableThemeShadow;
            set => SetAndRaise(IsThemeShadowDisabledProperty, ref _disableThemeShadow, value);
        }

        public bool OverlayDismissEventPassThrough
        {
            get => GetValue(OverlayDismissEventPassThroughProperty);
            set => SetValue(OverlayDismissEventPassThroughProperty, value);
        }

        public IInputElement OverlayInputPassThroughElement
        {
            get => _overlayInputPassThroughElement;
            set => SetAndRaise(OverlayInputPassThroughElementProperty, ref _overlayInputPassThroughElement, value);
        }

        #endregion

        #region AttachedProperties & related for AttachedFlyout

        public static readonly AttachedProperty<FlyoutBase> AttachedFlyoutProperty =
            AvaloniaProperty.RegisterAttached<FlyoutBase, Control, FlyoutBase>("AttachedPopup");

        public static FlyoutBase GetAttachedFlyout(Control obj)
        {
            return obj.GetValue(AttachedFlyoutProperty);
        }

        public static void SetAttachedFlyout(Control obj, FlyoutBase value)
        {
            obj.SetValue(AttachedFlyoutProperty, value);
        }

        public static void ShowAttachedFlyout(Control host)
        {
            GetAttachedFlyout(host).ShowAt(host);
        }

        public static void HideAttachedFlyout(Control host)
        {
            GetAttachedFlyout(host).Hide();
        }

        #endregion

        #region Events

        public event TypedEventHandler<FlyoutBase, object> Closed;
        public event TypedEventHandler<FlyoutBase, object> Opened;
        public event TypedEventHandler<FlyoutBase, object> Opening;
        public event TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs> Closing;

        #endregion

        #region Public Methods

        public void ShowAt(Control placementTarget)
        {
            ShowAtCore(placementTarget, null);

            //if (control == null)
            //    throw new ArgumentNullException();

            //if (_popup == null)
            //    InitPopup();

            //_popup.PlacementTarget = Target = control;

            //if (_popup.Child == null)
            //    _popup.Child = CreatePresenter();

            //OnOpening();

            ////TODO: implement remaining options...
            //switch (_placementMode)
            //{
            //    case FlyoutPlacementMode.Top:
            //        _popup.PlacementMode = PlacementMode.Top;
            //        break;
            //    case FlyoutPlacementMode.Right:
            //        _popup.PlacementMode = PlacementMode.Right;
            //        break;
            //    case FlyoutPlacementMode.Bottom:
            //        _popup.PlacementMode = PlacementMode.Bottom;
            //        break;
            //    case FlyoutPlacementMode.Left:
            //        _popup.PlacementMode = PlacementMode.Left;
            //        break;

            //    default:
            //        _popup.PlacementMode = PlacementMode.Top;
            //        break;
            //}

            //((ISetLogicalParent)_popup).SetParent(control);

            //_ignoreIsOpen = true;
            //IsOpen = _popup.IsOpen = true;
            //_ignoreIsOpen = false;
        }

        public void ShowAt(Control placementTarget, FlyoutShowOptions options)
        {
            ShowAtCore(placementTarget, options);
        }

        public void DisableLightDismiss(bool value)
        {
            //WinUI doesn't let Flyouts be non-light dismissable, but why limit.
            //But I'm rn, don't want full property for this either
            _disableLightDismiss = value;
            if (_popup != null)
            {
                _popup.IsLightDismissEnabled = !_disableLightDismiss;
            }
        }

        protected virtual void ShowAtCore(Control placementTarget, FlyoutShowOptions options)
        {
            if (_popup == null)
            {
                InitPopup();
            }
            
            if (_isOpen)
            {
                if (placementTarget == _target)
                {
                    return;
                }
                else //Close before opening a new one
                {
                    Hide(false /*canCancel*/);
                }
            }

            _popup.PlacementTarget = Target = placementTarget;
            _xamlRoot = Target.GetVisualRoot() as Control;

            if (_popup.Child == null)
            {
                //To enable drop shadow (mimic-ing Theme Shadow)
                //Window transparency MUST be enabled for this to work though
                //Since it's a fake shadow
                if (_disableThemeShadow)
                {
                    _popup.Child = CreatePresenter();
                }
                else
                {
                    _popup.Child = new Border
                    {
                        Child = new Border
                        {
                            Child = CreatePresenter(),
                            BoxShadow = new Avalonia.Media.BoxShadows(new Avalonia.Media.BoxShadow
                            {
                                Blur = 16,
                                Color = Color.Parse("#44000000"),
                                OffsetY = 8,
                                OffsetX = 0,
                                Spread = 2,
                            }),
                            //Make the margin 2x Blur, to prevent early cutoff
                            //but also so that small changes in Margin property
                            //set in FlyoutPresenterStyle can still occur
                            //The parent HWND is essentially full screen in WinUI/UWP so
                            //you can technically set a margin of -100 (top) and have it work
                            //Ths won't work here, but small adjustments should still be ok
                            Margin = new Thickness(_popupOffsetForShadow)
                        }
                    };
                }
            }
                


            //Required so LogicalTree & DataContext propagate into Popup & controls
            ((ISetLogicalParent)_popup).SetParent(placementTarget);
           
            OnOpening();

            //Placement = FlyoutPlacementMode.Right;
            if (options == null)
                options = new FlyoutShowOptions { Placement = Placement };

            PositionPopup(options);

            //Force popup to open so we get Layout
            //TODO: TEST WITH OVERLAY POPUPS
            //MAY NEED TO TRIGGER LAYOUT MANUALLY 
            _ignoreIsOpen = true;
            IsOpen = _popup.IsOpen = true;
            _ignoreIsOpen = false;

            OnOpened();
        }

        public void Hide(bool canCancel = true)
        {
            if (canCancel)
            {
                var args = new FlyoutBaseClosingEventArgs();
                OnClosing(args);
                if (args.Cancel)
                {
                    //Ensure we don't close anything
                    _ignoreIsOpen = true;
                    IsOpen = true;
                    _ignoreIsOpen = false;
                    return;
                }
            }
            
            _ignoreIsOpen = true;
            IsOpen = _popup.IsOpen = false;
            _ignoreIsOpen = false;

            OnClosed();
        }

        protected virtual void OnOpening()
        {
            Opening?.Invoke(this, null);
        }

        protected virtual void OnOpened()
        {
            Opened?.Invoke(this, null);
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
            throw new NotImplementedException("No base implementation for flyout presenter");
        }

        //This is kind of ugly, but it is what it is...
        protected internal Control GetPresenter => _disableThemeShadow ? _popup.Child : ((_popup?.Child as Border)?.Child as Border).Child as Control;

        #endregion

        #region Private Methods

        private void InitPopup()
        {
            _popup = new Popup();
            _popup[!Popup.IsOpenProperty] = this[!IsOpenProperty];
            _popup.WindowManagerAddShadowHint = false;
            _popup.IsLightDismissEnabled = !_disableLightDismiss;
            _popup.OverlayDismissEventPassThrough = OverlayDismissEventPassThrough;
            _popup.OverlayInputPassThroughElement = _overlayInputPassThroughElement;

            _popup.Opened += OnPopupOpened;
            _popup.Closed += OnPopupClosed;
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            Hide();
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            OnOpened();
        }

        private void PositionPopup(FlyoutShowOptions options)
        {
            //TODO:
            //FlyoutShowOptions.ExclusionRect
            //FlyoutShowOptions.Point
            //ShouldConstrainToRootBounds

            //Flyout isn't shown yet, so we manually measure the control
            var sz = LayoutHelper.MeasureChild(GetPresenter, Size.Infinity, new Thickness());
           
            var trgtBnds = Target.Bounds;
            //WinUI has a 4px margin around the popup for whatever reason
            //So any adjustment will also be offset by 4 along the adjacent edge
            double hAdjust = 0;
            double vAdjust = 0;

            _popup.PlacementMode = PlacementMode.AnchorAndGravity;

            _popup.PlacementConstraintAdjustment = Avalonia.Controls.Primitives.PopupPositioning.PopupPositionerConstraintAdjustment.SlideX | 
                Avalonia.Controls.Primitives.PopupPositioning.PopupPositionerConstraintAdjustment.SlideY;

            //PlacementRect MUST be within 0,0,wid-1,hei-1 of target or it will fail
            //I can't figure out why this is the case, but it just is...
            switch (options.Placement)
            {
                case FlyoutPlacementMode.Top: //Above & centered
                    _popup.PlacementRect = new Rect(-sz.Width / 2, 0, sz.Width, 1);
                    _popup.PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.Top;

                    vAdjust = _popupOffsetForShadow - 4;
                    break;

                case FlyoutPlacementMode.TopEdgeAlignedLeft:
                    _popup.PlacementRect = new Rect(0, 0, 0, 0);
                    _popup.PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.TopRight;
                    vAdjust = _popupOffsetForShadow - 4;
                    hAdjust = -_popupOffsetForShadow;
                    break;

                case FlyoutPlacementMode.TopEdgeAlignedRight:
                    _popup.PlacementRect = new Rect(trgtBnds.Width-1, 0, 10, 1);
                    _popup.PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.TopLeft;
                    vAdjust = _popupOffsetForShadow - 4;
                    hAdjust = _popupOffsetForShadow;
                    break;

                case FlyoutPlacementMode.RightEdgeAlignedTop:
                    _popup.PlacementRect = new Rect(trgtBnds.Width - 1, 0, 1, 1);
                    _popup.PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.BottomRight;
                    _popup.PlacementAnchor = Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.Right;

                    vAdjust = -_popupOffsetForShadow;
                    hAdjust = -_popupOffsetForShadow + 4;
                    break;

                case FlyoutPlacementMode.Right: //Right & centered
                    _popup.PlacementRect = new Rect(trgtBnds.Width-1, 0, 1, trgtBnds.Height);
                    _popup.PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.Right;
                    _popup.PlacementAnchor = Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.Right;
                                        
                    hAdjust = -_popupOffsetForShadow + 4;
                    break;

                case FlyoutPlacementMode.RightEdgeAlignedBottom:
                    _popup.PlacementRect = new Rect(trgtBnds.Width - 1, trgtBnds.Height-1, 1, 1);
                    _popup.PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.TopRight;
                    _popup.PlacementAnchor = Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.Right;
                    
                    vAdjust = _popupOffsetForShadow;
                    hAdjust = -_popupOffsetForShadow + 4;
                    break;

                case FlyoutPlacementMode.Bottom: //Below & centered
                    _popup.PlacementRect = new Rect(0, trgtBnds.Height - 1, trgtBnds.Width, 1);
                    _popup.PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.Bottom;
                    _popup.PlacementAnchor = Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.Bottom;


                    vAdjust = -_popupOffsetForShadow +4;
                    break;

                case FlyoutPlacementMode.BottomEdgeAlignedLeft:
                    _popup.PlacementRect = new Rect(0, trgtBnds.Height - 1, 1, 1);
                    _popup.PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.BottomRight;
                    _popup.PlacementAnchor = Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.Bottom;

                    vAdjust = -_popupOffsetForShadow + 4;
                    hAdjust = -_popupOffsetForShadow ;
                    break;

                case FlyoutPlacementMode.BottomEdgeAlignedRight:
                    _popup.PlacementRect = new Rect(trgtBnds.Width-1, trgtBnds.Height - 1, 1, 1);
                    _popup.PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.BottomLeft;
                    _popup.PlacementAnchor = Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.Bottom;

                    vAdjust = -_popupOffsetForShadow + 4;
                    hAdjust = _popupOffsetForShadow;
                    break;

                case FlyoutPlacementMode.LeftEdgeAlignedTop:
                    _popup.PlacementRect = new Rect(0,0, 1, 1);
                    _popup.PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.BottomLeft;
                    _popup.PlacementAnchor = Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.Left;

                    vAdjust = -_popupOffsetForShadow;
                    hAdjust = _popupOffsetForShadow - 4;
                    break;

                case FlyoutPlacementMode.Left: //Left & centered
                    _popup.PlacementRect = new Rect(0, 0, 1, trgtBnds.Height);
                    _popup.PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.Left;
                    _popup.PlacementAnchor = Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.Left;

                    hAdjust = _popupOffsetForShadow - 4;
                    break;

                case FlyoutPlacementMode.LeftEdgeAlignedBottom:
                    _popup.PlacementRect = new Rect(0, trgtBnds.Height-1, 1, 1);
                    _popup.PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.TopLeft;
                    _popup.PlacementAnchor = Avalonia.Controls.Primitives.PopupPositioning.PopupAnchor.BottomLeft;

                    vAdjust = _popupOffsetForShadow;
                    hAdjust = _popupOffsetForShadow - 4;
                    break;

                case FlyoutPlacementMode.Full:
                    //Not sure how the get this to work
                    //Popup should display at max size in the middle of the VisualRoot/Window of the Target
                    throw new NotSupportedException("FlyoutPlacementMode.Full is not supported at this time");
                    //break;

                //includes Auto (not sure what determines that)...
                default:
                    //This is just FlyoutPlacementMode.Top behavior (above & centered)
                    _popup.PlacementRect = new Rect(-sz.Width / 2, 0, sz.Width, 1);
                    _popup.PlacementGravity = Avalonia.Controls.Primitives.PopupPositioning.PopupGravity.Top;

                    vAdjust = _popupOffsetForShadow - 4;
                    break;
            }
            
            _popup.HorizontalOffset = !_disableThemeShadow ? hAdjust : 0;
            _popup.VerticalOffset = !_disableThemeShadow ? vAdjust : 0;

        }

        #endregion


        private bool _disableLightDismiss = false;
        private bool _areOpenCloseAnimationsEnabled = true; //Animations don't work
        private bool _isConstrainedToRootBounds = false;
        private FlyoutShowMode _showMode = FlyoutShowMode.Auto;

        private bool _disableThemeShadow = false;

        private bool _ignoreIsOpen;
        private bool _isOpen;
        private FlyoutPlacementMode _placementMode = FlyoutPlacementMode.Top;
        private Control _target;
        private IControl _xamlRoot;
        protected Popup _popup;
        private IInputElement _overlayInputPassThroughElement;

        internal static readonly int _popupOffsetForShadow = 32;
    }
}
