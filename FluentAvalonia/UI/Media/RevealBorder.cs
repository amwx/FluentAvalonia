//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml

using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using ReactiveUI;
using System;
using System.Collections.Specialized;
using System.Diagnostics;

namespace FluentAvalonia.UI.Media
{
    /// <summary>
    /// Psuedo-reveal border for controls, just add this to any control, and any container
    /// control implement ISupportReveal. Note, this is only meant to be included as an
    /// item in a ControlTemplate, and not as a standalone thing
    /// </summary>
    public class RevealBorder : Decorator
    {
        //These technically refer to the samething
        private ISupportReveal RevealHost { get; set; }
        private Control RevealHostControl { get; set; }

        //While border thickness can be set normally, we assume it's uniform,
        //and will only pull the LEFT thickness and apply it to all sides
        public RevealBorder()
        {
            

        }

        static RevealBorder()
        {
            RevealGradientStopsProperty.Changed.AddClassHandler<RevealBorder>((x,v) => x.OnRevealGradientChanged(v));
            AffectsRender<RevealBorder>(CornerRadiusProperty, RevealRadiusProperty);
        }

        #region AvaloniaProperties

        public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
            AvaloniaProperty.Register<RevealBorder, CornerRadius>("CornerRadius");

        public static readonly StyledProperty<Thickness> BorderThicknessProperty =
            AvaloniaProperty.Register<RevealBorder, Thickness>("BorderThickness");

        public static readonly StyledProperty<GradientStops> RevealGradientStopsProperty =
            AvaloniaProperty.Register<RevealBorder, GradientStops>("RevealGradientStops");

        public static readonly StyledProperty<double> RevealRadiusProperty =
            AvaloniaProperty.Register<RevealBorder, double>("RevealRadius", 0.75);

        public static readonly StyledProperty<IBrush> PointerOverBorderBrushProperty =
            AvaloniaProperty.Register<RevealBorder, IBrush>("PointerOverBorderBrush");

        #endregion


        #region CLRProperties
        /// <summary>
        /// Defines the CornerRadius property
        /// </summary>
        public CornerRadius CornerRadius
        {
            get => GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// The gradient stops used to define the reveal brush. Default Gradient stops are 
        /// defined in the ThemeStyles[Mode] files, but you can supply your own if you desire.
        /// </summary>
        public GradientStops RevealGradientStops
        {
            get => GetValue(RevealGradientStopsProperty);
            set => SetValue(RevealGradientStopsProperty, value);
        }

        /// <summary>
        /// Radius of reveal. Avalonia only supports uniform RGB, (no x/y radius), and
        /// its in relative coordinates.
        /// </summary>
        public double RevealRadius
        {
            get => GetValue(RevealRadiusProperty);
            set => SetValue(RevealRadiusProperty, value);
        }

        /// <summary>
        /// Defines the BorderThickness property
        /// </summary>
        public Thickness BorderThickness
        {
            get { return GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        #endregion

     

        #region protected methods

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);

            var item = e.Parent;
            while (item != null && !(item is ISupportReveal))
            {
                item = item.LogicalParent;
            }
            if (item == null)
                return;

            Debug.Assert(item is ISupportReveal);
            RevealHost = item as ISupportReveal;

            RevealHost.WhenAnyValue(x => x.IsRevealActive);

            RevealHostControl = RevealHost as Control;

            RevealHostControl.PointerMoved += RevealHostControl_PointerMoved;
            RevealHostControl.PointerLeave += RevealHostControl_PointerLeave;
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);
            if(RevealHostControl != null)
            {
                RevealHostControl.PointerMoved -= RevealHostControl_PointerMoved;
                RevealHostControl.PointerLeave -= RevealHostControl_PointerLeave;
            }
            

        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            if(TemplatedParent != null)
            {
                pointerOverOwnerTracker = TemplatedParent.GetObservable(IsPointerOverProperty).Subscribe(OnOwnerPointerOverChanged);
            }
        }
        
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            pointerOverOwnerTracker?.Dispose();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (PointerOver)
            {
                return; //Let Styles handle interaction with parent control
            }
            if (RevealHost != null && RevealHost.IsRevealActive)
            {
                Pen p = new Pen(_internalBrush, BorderThickness.Left);

                RoundedRect rr = new RoundedRect(new Rect(Bounds.Size).Inflate(-BorderThickness.Left / 2.0), CornerRadius.TopLeft, CornerRadius.TopLeft,
                    CornerRadius.BottomRight, CornerRadius.BottomLeft);

                context.PlatformImpl.DrawRectangle(null, p, rr);
            }
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the moving the gradient center when the mouse leaves over the ISupportReveal Host
        /// </summary>
        private void RevealHostControl_PointerLeave(object sender, Avalonia.Input.PointerEventArgs e)
        {
            //When the pointer leaves the screen, move the center to (minval,minval)
            //So it doesn't show up. This is needed for when the cursor leaves the window
            //Otherwise, since the reveal would remain if its against the border of the window
            if (RevealHost.IsRevealActive && _internalBrush != null)
            {
                var pt = e.GetCurrentPoint(this);
                _internalBrush.Center = new RelativePoint(double.MinValue, double.MinValue, RelativeUnit.Absolute);
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Handles the moving the gradient center when the mouse moves over the ISupportReveal Host
        /// </summary>
        private void RevealHostControl_PointerMoved(object sender, Avalonia.Input.PointerEventArgs e)
        {
            //When pointer is over the container control, we want to keep the GradientBrush
            //location updated and in contact with the Poitner, but use coordinates relative to this
            if(RevealHost.IsRevealActive && _internalBrush != null)
            {
                var pt = e.GetCurrentPoint(this);
                _internalBrush.Center = new RelativePoint(pt.Position.X, pt.Position.Y, RelativeUnit.Absolute);
                InvalidateVisual();
            }
        }


        /// <summary>
        /// Recreates the GradientBrush when properties change
        /// </summary>
        private void RecreateBrush()
        {
            var oldCX = double.MinValue;
            var oldCY = double.MinValue;
            var oldUnit = RelativeUnit.Absolute;
            if (_internalBrush != null)
            {
                oldCX = _internalBrush.Center.Point.X;
                oldCY = _internalBrush.Center.Point.Y;
                oldUnit = _internalBrush.Center.Unit;
                _internalBrush = null;
            }
            if (RevealGradientStops != null && RevealGradientStops.Count > 0)
            {
                _internalBrush = new RadialGradientBrush();
                _internalBrush.Center = new RelativePoint(oldCX, oldCY, oldUnit);
                _internalBrush.GradientStops = RevealGradientStops;
                _internalBrush.Radius = RevealRadius;
            }
        }

        private void OnRevealGradientChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                ((GradientStops)e.OldValue).CollectionChanged -= OnGradientStopsChanged;
            }

            if (e.NewValue != null)
            {
                ((GradientStops)e.NewValue).CollectionChanged += OnGradientStopsChanged;
            }
            RecreateBrush();
            InvalidateVisual();
        }

        private void OnGradientStopsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RecreateBrush();
            InvalidateVisual();
        }

        private void OnOwnerPointerOverChanged(bool obj)
        {
            PointerOver = obj;
            InvalidateVisual();
        }

        #endregion

        private RadialGradientBrush _internalBrush;
        private bool PointerOver = false;
        IDisposable pointerOverOwnerTracker;
    }
}
