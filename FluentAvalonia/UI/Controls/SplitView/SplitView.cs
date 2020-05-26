//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml

using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using FluentAvalonia.Core;
using System;

namespace FluentAvalonia.UI.Controls
{
    public class SplitView : TemplatedControl
    {
        public SplitView()
        {
            //TemplateSettings = new SplitViewTemplateSettings(this);
            //SetTemplateSettings();
            //PanePlacement = SplitViewPanePlacement.Left;
            //DisplayMode = SplitViewDisplayMode.Inline;
            //PseudoClasses.Add(":inline");
            PseudoClasses.Add(":left");
            PseudoClasses.Add(":overlay");
            //PseudoClasses.Add(":closed");
            //his.GetObservable(BoundsProperty).Subscribe(OnBoundsChanged);
        }

        static SplitView()
        {
            PanePlacementProperty.Changed.AddClassHandler<SplitView>((x, e) => x.OnPanePlacementChanged(e));
            DisplayModeProperty.Changed.AddClassHandler<SplitView>((x, e) => x.OnDisplayModeChanged(e));
            OpenPaneLengthProperty.Changed.AddClassHandler<SplitView>((x, e) => x.OnOpenPaneLengthChanged(e));
            CompactPaneLengthProperty.Changed.AddClassHandler<SplitView>((x, e) => x.OnCompactPaneLengthChanged(e));
        }

        #region ContentAlignment Properties
        public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
            AvaloniaProperty.Register<SplitView, HorizontalAlignment>("HorizontalContentAlignment");
        public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
           AvaloniaProperty.Register<SplitView, VerticalAlignment>("VerticalContentAlignment");

        public HorizontalAlignment HorizontalContentAlignment
        {
            get => GetValue(HorizontalContentAlignmentProperty);
            set => SetValue(HorizontalContentAlignmentProperty, value);
        }
        public VerticalAlignment VerticalContentAlignment
        {
            get => GetValue(VerticalContentAlignmentProperty);
            set => SetValue(VerticalContentAlignmentProperty, value);
        }
        #endregion

        #region AvaloniaProperties

        public static readonly StyledProperty<Control> ContentProperty =
            AvaloniaProperty.Register<SplitView, Control>("Content");

        public static readonly StyledProperty<double> CompactPaneLengthProperty =
            AvaloniaProperty.Register<SplitView, double>("CompactPaneLength", defaultValue: 48);

        public static readonly StyledProperty<SplitViewDisplayMode> DisplayModeProperty =
            AvaloniaProperty.Register<SplitView, SplitViewDisplayMode>("DisplayMode", defaultValue: SplitViewDisplayMode.Overlay);

        public static readonly DirectProperty<SplitView,bool> IsPaneOpenProperty =
            AvaloniaProperty.RegisterDirect<SplitView, bool>("IsPaneOpen", 
                x=> x.IsPaneOpen, (x,v) => x.IsPaneOpen = v);

        public static readonly StyledProperty<double> OpenPaneLengthProperty =
            AvaloniaProperty.Register<SplitView, double>("OpenPaneLength", defaultValue: 320);

        public static readonly StyledProperty<IBrush> PaneBackgroundProperty =
            AvaloniaProperty.Register<SplitView, IBrush>("PaneBackground");

        public static readonly StyledProperty<SplitViewPanePlacement> PanePlacementProperty =
            AvaloniaProperty.Register<SplitView, SplitViewPanePlacement>("PanePlacement");

        public static readonly StyledProperty<Control> PaneProperty =
            AvaloniaProperty.Register<SplitView, Control>("Pane");

        public static readonly StyledProperty<LightDismissOverlayMode> LightDismissOverlayModeProperty =
            AvaloniaProperty.Register<SplitView, LightDismissOverlayMode>("LightDismissOverlay");


        #endregion

        #region CLR Properties

        [Content]
        public Control Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public double CompactPaneLength
        {
            get => GetValue(CompactPaneLengthProperty);
            set => SetValue(CompactPaneLengthProperty, value);
        }

        public SplitViewDisplayMode DisplayMode
        {
            get => GetValue(DisplayModeProperty);
            set => SetValue(DisplayModeProperty, value);
        }

        public bool IsPaneOpen
        {
            get => _IsPaneOpen;
            set
            {
                if (value == _IsPaneOpen)
                    return;

                if (value)
                {
                    OnPaneOpening(this, null);
                    SetAndRaise(IsPaneOpenProperty, ref _IsPaneOpen, value);
                    SetStateOnOpenChanged(value);
                    OnPaneOpened(this, null);
                }
                else
                {
                    SplitViewPaneClosingEventArgs args = new SplitViewPaneClosingEventArgs(false);
                    OnPaneClosing(this, args);
                    if (!args.Cancel)
                    {
                        SetAndRaise(IsPaneOpenProperty, ref _IsPaneOpen, value);
                        SetStateOnOpenChanged(value);
                        OnPaneClosed(this, null);
                    }
                }
            }
        }

        public double OpenPaneLength
        {
            get => GetValue(OpenPaneLengthProperty);
            set => SetValue(OpenPaneLengthProperty, value);
        }

        public IBrush PaneBackground
        {
            get => GetValue(PaneBackgroundProperty);
            set => SetValue(PaneBackgroundProperty, value);
        }

        public SplitViewPanePlacement PanePlacement
        {
            get => GetValue(PanePlacementProperty);
            set => SetValue(PanePlacementProperty, value);
        }

        public Control Pane
        {
            get => GetValue(PaneProperty);
            set => SetValue(PaneProperty, value);
        }

        public LightDismissOverlayMode LightDismissOverlayMode
        {
            get => GetValue(LightDismissOverlayModeProperty);
            set => SetValue(LightDismissOverlayModeProperty, value);
        }

        #endregion

        #region Events

        public event TypedEventHandler<SplitView, object> PaneClosed;
        public event TypedEventHandler<SplitView, SplitViewPaneClosingEventArgs> PaneClosing;
        public event TypedEventHandler<SplitView, object> PaneOpened;
        public event TypedEventHandler<SplitView, object> PaneOpening;

        #endregion

        #region PropertyChanged EventHandlers

        private void OnCompactPaneLengthChanged(AvaloniaPropertyChangedEventArgs e)
        {
            GoToState(GetStateNameFromSettings(), false);
        }

        private void OnOpenPaneLengthChanged(AvaloniaPropertyChangedEventArgs e)
        {
            GoToState(GetStateNameFromSettings(), false);
        }

        private void OnPanePlacementChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var newV = (SplitViewPanePlacement)e.NewValue;
            SetPlacementState(newV);
        }

        private void OnDisplayModeChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var oldVal = (SplitViewDisplayMode)e.OldValue;
            var newVal = (SplitViewDisplayMode)e.NewValue;
            SetDisplayModeState(oldVal, newVal);
        }

        #endregion

        #region override methods

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            //Use Get<>() so an exception is thrown if these objects aren't found
            _Container = e.NameScope.Get<Grid>("Container");
            _PaneRoot = e.NameScope.Get<Grid>("PaneRoot");
            _ContentRoot = e.NameScope.Get<Grid>("ContentRoot");
            _HCPaneBorder = e.NameScope.Get<Rectangle>("HCPaneBorder");

            GoToState(GetStateNameFromSettings(), false); //Don't animate on first load
            //_PaneClipRect = _PaneRoot.Clip as RectangleGeometry;

            
            _DismissLayer = e.NameScope.Get<Rectangle>("LightDismissLayer");
            _DismissLayer.PointerReleased += _DismissLayer_PointerReleased;
            
        }

        #endregion

        #region virtual methods

        protected virtual void OnPaneOpening(SplitView sender, object args)
        {
            PaneOpening?.Invoke(sender, args);
        }
        protected virtual void OnPaneOpened(SplitView sender, object args)
        {
            PaneOpened?.Invoke(sender, args);
        }
        protected virtual void OnPaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            PaneClosing?.Invoke(sender, args);
        }
        protected virtual void OnPaneClosed(SplitView sender, object args)
        {
            PaneClosed?.Invoke(sender, args);
        }

        #endregion

        #region private methods

        private void _DismissLayer_PointerReleased(object sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            IsPaneOpen = false;
        }

        private void SetDismissLayer(bool value)
        {
            if (_DismissLayer == null)
                return;
            //If the user doesn't want to use LightDismissOverlayMode, ensure it's false & return
            if (value && LightDismissOverlayMode == LightDismissOverlayMode.Off)
            {
                _DismissLayer.IsVisible = false;
                return;
            }
            _DismissLayer.IsVisible = value;
        }

        private void SetStateOnOpenChanged(bool newV)
        {
            GoToState(GetStateNameFromSettings(), true); //CHANGE TO TRUE
            //if(PanePlacement == SplitViewPanePlacement.Left)
            //{
            //    var displayMode = DisplayMode;
            //    if(displayMode == SplitViewDisplayMode.Overlay)
            //    {
            //        //Classes.Clear();
            //        PseudoClasses.Add(!newV ? ":openToClosedOverlayLeft" : ":closedToOpenOverlayLeft");
            //        PseudoClasses.Remove(newV ? ":openToClosedOverlayLeft" : ":closedToOpenOverlayLeft");
            //    }
            //    else if(displayMode == SplitViewDisplayMode.CompactOverlay)
            //    {
            //        //Classes.Clear();
            //        PseudoClasses.Add(!newV ? ":openToClosedCompactOverlayLeft" : ":closedToOpenCompactOverlayLeft");
            //        PseudoClasses.Remove(newV ? ":openToClosedCompactOverlayLeft" : ":closedToOpenCompactOverlayLeft");
            //    }
            //}
            //else //Right
            //{
            //    var displayMode = DisplayMode;
            //    if (displayMode == SplitViewDisplayMode.Overlay)
            //    {
            //        //Classes.Clear();
            //        PseudoClasses.Add(!newV ? ":openToClosedOverlayRight" : ":closedToOpenOverlayRight");
            //        PseudoClasses.Remove(newV ? ":openToClosedOverlayRight" : ":closedToOpenOverlayRight");
            //    }
            //    else if (displayMode == SplitViewDisplayMode.CompactOverlay)
            //    {
            //        //Classes.Clear();
            //        PseudoClasses.Add(!newV ? ":openToClosedCompactOverlayRight" : ":closedToOpenCompactOverlayRight");
            //        PseudoClasses.Remove(newV ? ":openToClosedCompactOverlayRight" : ":closedToOpenCompactOverlayRight");
            //    }
            //}
           // PseudoClasses.Set(":open", newV);
        }

        private void SetPlacementState(SplitViewPanePlacement newV)
        {
            //No transition if changing placement, just do it =D
            GoToState(GetStateNameFromSettings(), false);
            //Closed (default state
            //ClosedCompactLeft
            //ClosedCompactRight
            //OpenOverlayLeft
            //OpenoverlayRight
            //OpenInlineLeft
            //OpenInlineRight
            //OpenCompactOverlayLeft
            //OpenCompactOverlayRight
            //Change how the SplitView template is laid out, but don't animate
            //PseudoClasses.Add(newV == SplitViewPanePlacement.Left ? ":left" : ":right");
            //PseudoClasses.Remove(newV != SplitViewPanePlacement.Left ? ":left" : ":right");
        }

        private void SetDisplayModeState(SplitViewDisplayMode oldV, SplitViewDisplayMode newV)
        {
            //No Transition if chaning DisplayMode, just do it =D
            GoToState(GetStateNameFromSettings(), false);
        }

        #endregion


        #region pseudo-VisualStateManager

        private string GetStateNameFromSettings()
        {
            /*
             *   Valid VisualStates
                    >Closed (default)
                    >ClosedCompactLeft
                    >ClosedCompactRight
                    >OpenOverlayLeft
                    >OpenOverlayRight
                    >OpenInlineLeft
                    >OpenInlineRight
                    >OpenCompactOverlayLeft
                    >OpenCompactOverlayRight
             */

            var placementString = PanePlacement.ToString();//== SplitViewPanePlacement.Left ? "Left" : "Right";
            var dm = DisplayMode.ToString();
            bool isOpen = IsPaneOpen;

            if (!isOpen)
            {
                //If we're closed, we only have Closed & ClosedCompact
                if (dm.Contains("Compact"))
                    return $"ClosedCompact{placementString}";
                else
                    return "Closed";
            }
            else
            {
                //When open, we have OpenOverlay, OpenCompact, and OpenInline
                //OpenCompact works for both Overlay and Inline mode
                if (dm.Contains("Inline"))
                    return $"OpenInline{placementString}";
                else if (dm.Contains("Compact"))
                    return $"OpenCompactOverlay{placementString}";
                else
                    return $"OpenOverlay{placementString}";
            }
        }

        private void GoToState(string stateName, bool useTransitions)
        {
            if (_Container == null) //Haven't init-d yet
                return;
            /*  
             *  Valid VisualStates
            >Closed (default)
            >ClosedCompactLeft
            >ClosedCompactRight
            >OpenOverlayLeft
            >OpenOverlayRight
            >OpenInlineLeft
            >OpenInlineRight
            >OpenCompactOverlayLeft
            >OpenCompactOverlayRight

             *  Valid Transitions
            > Closed --> OpenOverlayLeft
            > Closed --> OpenOverlayRight
            > ClosedCompactLeft --> OpenCompactOverlayLeft
            > ClosedCompactRight --> OpenCompactOverlayRight
            > OpenOverlayLeft --> Closed
            > OpenOverlayRight --> Closed
            > OpenCompactOverlayLeft --> ClosedCompactLeft
            > OpenCompactOverlayRight --> ClosedCompactRight
            > OpenInlineLeft --> Closed
            > Closed --> OpenInlineLeft
            > ClosedCompactLeft --> OpenInlineLeft
            > OpenInlineLeft --> ClosedCompactLeft             
             */

            //If we haven't init-ed yet, set the state
            if (string.IsNullOrEmpty(previousState))
            {
                previousState = "Closed";//Always start closed
            }

            //System.Diagnostics.Debug.WriteLine($"STATES >> {previousState} {stateName}");

            if (useTransitions)
            {
                switch (stateName)
                {
                    case "OpenOverlayLeft":
                        TransitionToOpenOverlayLeft();
                        break;
                    case "OpenOverlayRight":
                        TransitionToOpenOverlayRight();
                        break;
                    case "OpenCompactOverlayLeft":
                        TransitionToOpenCompactOverlayLeft();
                        break;
                    case "OpenCompactOverlayRight":
                        TransitionToOpenCompactOverlayRight();
                        break;
                    case "Closed":
                        TransitionToClosed();
                        break;
                    case "ClosedCompactLeft":
                        TransitionToClosedCompactLeft();
                        break;
                    case "ClosedCompactRight":
                        TransitionToClosedCompactRight();
                        break;
                    case "OpenInlineLeft":
                        TransitionToOpenInlineLeft();
                        break;
                    case "OpenInlineRight":
                        TransitionToOpenInlineRight();
                        break;
                }

            }
            else
            {
                //     > Closed(default)
                //> ClosedCompactLeft
                //> ClosedCompactRight
                //> OpenOverlayLeft
                //> OpenOverlayRight
                //> OpenInlineLeft
                //> OpenInlineRight
                //> OpenCompactOverlayLeft
                //> OpenCompactOverlayRight


                if (stateName == "Closed")
                {
                    SetClosed();
                }
                else if (stateName == "ClosedCompactLeft")
                {
                    SetClosedCompact();
                }
                else if (stateName == "ClosedCompactRight")
                {
                    SetClosedCompact(false);
                }
                else if (stateName == "OpenOverlayLeft")
                {
                    SetOpenOverlay();
                }
                else if (stateName == "OpenOverlayRight")
                {
                    SetOpenOverlay(false);
                }
                else if (stateName == "OpenInlineLeft")
                {
                    SetOpenInline();
                }
                else if (stateName == "OpenInlineRight")
                {
                    SetOpenInline(false);
                }
                else if (stateName == "OpenCompactOverlayLeft")
                {
                    SetOpenCompactOverlay();
                }
                else if (stateName == "OpenCompactOverlayRight")
                {
                    SetOpenCompactOverlay(false);
                }
            }

            previousState = stateName;

        }

        #region No Transition State setters

        private void SetClosed()
        {
            _Container.ColumnDefinitions[0].Width = new GridLength(OpenPaneLength);
            _Container.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);

            Grid.SetColumn(_ContentRoot, 0);
            Grid.SetColumnSpan(_ContentRoot, 2);
            Grid.SetColumn(_PaneRoot, 0);
            Grid.SetColumnSpan(_PaneRoot, 2);
            
            _PaneRoot.Width = 0;
            _PaneRoot.HorizontalAlignment = PanePlacement == SplitViewPanePlacement.Left ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            _PaneRoot.IsVisible = false;

            SetDismissLayer(false);
        }

        private void SetClosedCompact(bool left = true)
        {
            SetDismissLayer(false);
            if (left)
            {
                _Container.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                _Container.ColumnDefinitions[0].Width = new GridLength(CompactPaneLength);
                Grid.SetColumn(_ContentRoot, 1);
                Grid.SetColumnSpan(_ContentRoot, 1);

                Grid.SetColumn(_PaneRoot, 0);
                Grid.SetColumnSpan(_PaneRoot, 1);

                _PaneRoot.IsVisible = true;
                _PaneRoot.Width = CompactPaneLength;
            }
            else
            {
                _Container.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                _Container.ColumnDefinitions[1].Width = new GridLength(CompactPaneLength);

                Grid.SetColumnSpan(_ContentRoot, 1);
                Grid.SetColumn(_ContentRoot, 0);

                _PaneRoot.IsVisible = true;
                Grid.SetColumn(_PaneRoot, 1);
                Grid.SetColumnSpan(_PaneRoot, 1);
                _PaneRoot.HorizontalAlignment = HorizontalAlignment.Right;
                _PaneRoot.Width = CompactPaneLength;
            }
        }

        private void SetOpenOverlay(bool left = true)
        {
            
            if (left)
            {
                _Container.ColumnDefinitions[0].Width = new GridLength(OpenPaneLength);
                _Container.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                _Container.ColumnDefinitions[1].Width = new GridLength(OpenPaneLength);
                _Container.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            }
            

            Grid.SetColumn(_ContentRoot, 0);
            Grid.SetColumnSpan(_ContentRoot, 2);
            Grid.SetColumn(_PaneRoot, 0);
            Grid.SetColumnSpan(_PaneRoot, 2);

            _PaneRoot.Width = OpenPaneLength;
            _PaneRoot.HorizontalAlignment = left ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            _PaneRoot.IsVisible = true;

            SetDismissLayer(true);
        }

        private void SetOpenInline(bool left = true)
        {
            SetDismissLayer(false);
            if (left)
            {
                //Set the pane column to auto, allowing for animation
                _Container.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Auto);
                _Container.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);

                Grid.SetColumn(_ContentRoot, 1);
                Grid.SetColumnSpan(_ContentRoot, 1);
                Grid.SetColumn(_PaneRoot, 0);
                Grid.SetColumnSpan(_PaneRoot, 1);

                _PaneRoot.Width = OpenPaneLength;
                _PaneRoot.HorizontalAlignment = HorizontalAlignment.Left;
                _PaneRoot.IsVisible = true;

            }
            else
            {
                //Set the pane column to auto, allowing for animation
                _Container.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Auto);
                _Container.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);

                Grid.SetColumn(_ContentRoot, 0);
                Grid.SetColumnSpan(_ContentRoot, 1);
                Grid.SetColumn(_PaneRoot, 1);
                Grid.SetColumnSpan(_PaneRoot, 1);

                _PaneRoot.Width = OpenPaneLength;
                _PaneRoot.HorizontalAlignment = HorizontalAlignment.Right;
                _PaneRoot.IsVisible = true;
            }
        }

        private void SetOpenCompactOverlay(bool left = true)
        {
            if (left)
            {
                _Container.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                _Container.ColumnDefinitions[0].Width = new GridLength(CompactPaneLength);
                Grid.SetColumn(_ContentRoot, 1);
                Grid.SetColumnSpan(_ContentRoot, 1);

                Grid.SetColumn(_PaneRoot, 0);
                Grid.SetColumnSpan(_PaneRoot, 2); //Not in WinUI, but ??
                _PaneRoot.Width = OpenPaneLength;

                _PaneRoot.IsVisible = true;
                _HCPaneBorder.IsVisible = true; //Needed?
                //_PaneRoot.Clip.Transform = new TranslateTransform(0, 0);
                //(_PaneRoot.Clip.Transform as TranslateTransform).X = 0;
            }
            else
            {
                _Container.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                _Container.ColumnDefinitions[1].Width = new GridLength(CompactPaneLength);
                Grid.SetColumnSpan(_ContentRoot, 1);
                Grid.SetColumn(_ContentRoot, 0);

                Grid.SetColumnSpan(_PaneRoot, 2); //Not in WinUI, but ??
                Grid.SetColumn(_PaneRoot, 1);
                _PaneRoot.Width = OpenPaneLength;

                _PaneRoot.IsVisible = true;
                _PaneRoot.HorizontalAlignment = HorizontalAlignment.Right;
                _HCPaneBorder.HorizontalAlignment = HorizontalAlignment.Left;
                _HCPaneBorder.IsVisible = true; //Needed?
               // _PaneRoot.Clip.Transform = new TranslateTransform(0, 0);
            }
            SetDismissLayer(true);

        }

        #endregion

        #region Transition setters

        private async void TransitionToClosed()
        {
            _Container.ColumnDefinitions[0].Width = new GridLength(OpenPaneLength);
            _Container.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);

            Grid.SetColumn(_ContentRoot, 0);
            Grid.SetColumnSpan(_ContentRoot, 2);
            Grid.SetColumn(_PaneRoot, 0);
            Grid.SetColumnSpan(_PaneRoot, 2);
            _PaneRoot.HorizontalAlignment = PanePlacement == SplitViewPanePlacement.Left ? HorizontalAlignment.Left : HorizontalAlignment.Right;

            Animation ani = new Animation();
            ani.FillMode = FillMode.Forward;
            ani.Duration = TimeSpan.FromSeconds(0.1);

            KeyFrame kf = new KeyFrame();
            kf.Cue = new Cue(1d);
            kf.KeySpline = new KeySpline(0.1, 0.9, 0.2, 1.0); //MS's default spline
            kf.Setters.Add(new Setter(Control.WidthProperty, 0.0));

            ani.Children.Add(kf);

            await ani.RunAsync(_PaneRoot);

            //Ensure value is persisted & hide the Grid
            _PaneRoot.IsVisible = false;
            // _PaneRoot.Width = 0;

            SetDismissLayer(false);
        }

        private async void TransitionToOpenOverlayLeft()
        {
            _Container.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
            _Container.ColumnDefinitions[0].Width = new GridLength(CompactPaneLength);

            Grid.SetColumn(_ContentRoot, 0);
            Grid.SetColumnSpan(_ContentRoot, 2);

            Grid.SetColumn(_PaneRoot, 0);
            Grid.SetColumnSpan(_PaneRoot, 2); //Not in WinUI, but ??

            _PaneRoot.IsVisible = true;
            _HCPaneBorder.IsVisible = true; //Needed?

            Animation ani = new Animation();
            ani.FillMode = FillMode.Forward;
            ani.Duration = TimeSpan.FromSeconds(0.2); //SplitViewPaneAnimationOpenDuration

            KeyFrame kf = new KeyFrame();
            kf.Cue = new Cue(1d);
            kf.Setters.Add(new Setter(Grid.WidthProperty, OpenPaneLength));
            kf.KeySpline = new KeySpline(0.1, 0.9, 0.2, 1.0); //MS's default spline
            ani.Children.Add(kf);

            await ani.RunAsync(_PaneRoot);

            //Ensure final ani value is persisted
            //_PaneRoot.Width = OpenPaneLength;
            SetDismissLayer(true);
        }
        private async void TransitionToOpenOverlayRight()
        {
            //Ensure the base state is set...
            _Container.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
            _Container.ColumnDefinitions[0].Width = new GridLength(CompactPaneLength);

            Grid.SetColumn(_ContentRoot, 0);
            Grid.SetColumnSpan(_ContentRoot, 2);

            Grid.SetColumnSpan(_PaneRoot, 2); //Not in WinUI, but ??
            Grid.SetColumn(_PaneRoot, 1);

            _PaneRoot.IsVisible = true;
            _PaneRoot.HorizontalAlignment = HorizontalAlignment.Right;
            _HCPaneBorder.HorizontalAlignment = HorizontalAlignment.Left;
            _HCPaneBorder.IsVisible = true; //Needed?

            Animation ani = new Animation();
            ani.FillMode = FillMode.Forward;
            ani.Duration = TimeSpan.FromSeconds(0.2); //SplitViewPaneAnimationOpenDuration

            KeyFrame kf = new KeyFrame();
            kf.Cue = new Cue(1d);
            kf.Setters.Add(new Setter(Grid.WidthProperty, OpenPaneLength));
            kf.KeySpline = new KeySpline(0.1, 0.9, 0.2, 1.0); //MS's default spline
            ani.Children.Add(kf);

            await ani.RunAsync(_PaneRoot);

            //Ensure final ani value is persisted
            // _PaneRoot.Width = OpenPaneLength;
            SetDismissLayer(true);
        }

        private async void TransitionToOpenCompactOverlayLeft()
        {
            //Base State
            _Container.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
            _Container.ColumnDefinitions[0].Width = new GridLength(CompactPaneLength);
            Grid.SetColumn(_ContentRoot, 1);
            Grid.SetColumnSpan(_ContentRoot, 1);

            Grid.SetColumn(_PaneRoot, 0);
            Grid.SetColumnSpan(_PaneRoot, 2); //Not in WinUI, but ??
            _PaneRoot.IsVisible = true;
            _HCPaneBorder.IsVisible = true; //Needed?

            Animation ani = new Animation();
            ani.FillMode = FillMode.Forward;
            ani.Duration = TimeSpan.FromSeconds(0.2); //SplitViewPaneAnimationOpenDuration

            KeyFrame kf = new KeyFrame();
            kf.Cue = new Cue(1d);
            kf.Setters.Add(new Setter(Grid.WidthProperty, OpenPaneLength));
            kf.KeySpline = new KeySpline(0.1, 0.9, 0.2, 1.0); //MS's default spline
            ani.Children.Add(kf);

            await ani.RunAsync(_PaneRoot);

            //Persist value            
            // _PaneRoot.Width = OpenPaneLength;
            SetDismissLayer(true);
        }
        private async void TransitionToOpenCompactOverlayRight()
        {
            _Container.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            _Container.ColumnDefinitions[1].Width = new GridLength(CompactPaneLength);
            Grid.SetColumnSpan(_ContentRoot, 1);
            Grid.SetColumn(_ContentRoot, 0);

            Grid.SetColumnSpan(_PaneRoot, 2); //Not in WinUI, but ??
            Grid.SetColumn(_PaneRoot, 1);

            _PaneRoot.IsVisible = true;
            _PaneRoot.HorizontalAlignment = HorizontalAlignment.Right;
            _HCPaneBorder.HorizontalAlignment = HorizontalAlignment.Left;
            _HCPaneBorder.IsVisible = true;

            Animation ani = new Animation();
            ani.FillMode = FillMode.Forward;
            ani.Duration = TimeSpan.FromSeconds(0.2); //SplitViewPaneAnimationOpenDuration

            KeyFrame kf = new KeyFrame();
            kf.Cue = new Cue(1d);
            kf.Setters.Add(new Setter(Grid.WidthProperty, OpenPaneLength));
            kf.KeySpline = new KeySpline(0.1, 0.9, 0.2, 1.0); //MS's default spline
            ani.Children.Add(kf);

            await ani.RunAsync(_PaneRoot);

            //Persist
            // _PaneRoot.Width = OpenPaneLength;
            SetDismissLayer(true);
        }

        private async void TransitionToClosedCompactLeft()
        {
            _Container.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
            _Container.ColumnDefinitions[0].Width = new GridLength(CompactPaneLength);
            Grid.SetColumn(_ContentRoot, 1);
            Grid.SetColumnSpan(_ContentRoot, 1);

            Grid.SetColumn(_PaneRoot, 0);
            Grid.SetColumnSpan(_PaneRoot, 1);

            Animation ani = new Animation();
            ani.FillMode = FillMode.Forward;
            ani.Duration = TimeSpan.FromSeconds(0.1);

            KeyFrame kf = new KeyFrame();
            kf.Cue = new Cue(1d);
            kf.KeySpline = new KeySpline(0.1, 0.9, 0.2, 1.0); //MS's default spline
            kf.Setters.Add(new Setter(Control.WidthProperty, CompactPaneLength));

            ani.Children.Add(kf);

            await ani.RunAsync(_PaneRoot);

            //Persist
            _PaneRoot.IsVisible = true;
            //_PaneRoot.Width = CompactPaneLength;
            SetDismissLayer(false);
        }
        private async void TransitionToClosedCompactRight()
        {
            _Container.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            _Container.ColumnDefinitions[1].Width = new GridLength(CompactPaneLength);

            Grid.SetColumnSpan(_ContentRoot, 1);
            Grid.SetColumn(_ContentRoot, 0);

            _PaneRoot.IsVisible = true;
            Grid.SetColumn(_PaneRoot, 1);
            Grid.SetColumnSpan(_PaneRoot, 1);
            _PaneRoot.HorizontalAlignment = HorizontalAlignment.Right;

            Animation ani = new Animation();
            ani.FillMode = FillMode.Forward;
            ani.Duration = TimeSpan.FromSeconds(0.1);

            KeyFrame kf = new KeyFrame();
            kf.Cue = new Cue(1d);
            kf.KeySpline = new KeySpline(0.1, 0.9, 0.2, 1.0); //MS's default spline
            kf.Setters.Add(new Setter(Control.WidthProperty, CompactPaneLength));

            ani.Children.Add(kf);

            await ani.RunAsync(_PaneRoot);

            //_PaneRoot.Width = CompactPaneLength;
            SetDismissLayer(false);
        }

        private async void TransitionToOpenInlineLeft()
        {
            _Container.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Auto);
            _Container.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);

            Grid.SetColumn(_ContentRoot, 1);
            Grid.SetColumnSpan(_ContentRoot, 1);
            Grid.SetColumn(_PaneRoot, 0);
            Grid.SetColumnSpan(_PaneRoot, 1);

            _PaneRoot.HorizontalAlignment = HorizontalAlignment.Left;
            _PaneRoot.IsVisible = true;

            Animation ani = new Animation();
            ani.FillMode = FillMode.Forward;
            ani.Duration = TimeSpan.FromSeconds(0.2);

            KeyFrame kf = new KeyFrame();
            kf.Cue = new Cue(1d);
            kf.KeySpline = new KeySpline(0.1, 0.9, 0.2, 1.0); //MS's default spline
            kf.Setters.Add(new Setter(Control.WidthProperty, OpenPaneLength));

            ani.Children.Add(kf);

            await ani.RunAsync(_PaneRoot);

            //_PaneRoot.Width = OpenPaneLength;
            SetDismissLayer(false);
        }
        private async void TransitionToOpenInlineRight()
        {
            //Base State
            _Container.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Auto);
            _Container.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);

            Grid.SetColumn(_ContentRoot, 0);
            Grid.SetColumnSpan(_ContentRoot, 1);
            Grid.SetColumn(_PaneRoot, 1);
            Grid.SetColumnSpan(_PaneRoot, 1);
                        
            _PaneRoot.HorizontalAlignment = HorizontalAlignment.Right;
            _PaneRoot.IsVisible = true;

            Animation ani = new Animation();
            ani.FillMode = FillMode.Forward;
            ani.Duration = TimeSpan.FromSeconds(0.2);

            KeyFrame kf = new KeyFrame();
            kf.Cue = new Cue(1d);
            kf.KeySpline = new KeySpline(0.1, 0.9, 0.2, 1.0); //MS's default spline
            kf.Setters.Add(new Setter(Control.WidthProperty, OpenPaneLength));

            ani.Children.Add(kf);

            await ani.RunAsync(_PaneRoot);

            // _PaneRoot.Width = OpenPaneLength;
            SetDismissLayer(false);
        }

        #endregion

        #endregion

        private string previousState = null;
        private bool _IsPaneOpen;
        //Template Parts
        private Grid _Container;
        private Grid _PaneRoot;
        private Grid _ContentRoot;
        private Rectangle _HCPaneBorder;
        private Rectangle _DismissLayer;
    }

    
    

    
}
