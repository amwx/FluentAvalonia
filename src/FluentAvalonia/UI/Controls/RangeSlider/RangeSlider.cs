using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.VisualTree;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

public partial class RangeSlider : TemplatedControl
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == RangeStartProperty)
        {
            _minSet = true;

            if (!_valuesAssigned)
                return;

            var newV = change.GetNewValue<double>();
            RangeMinToStepFrequency();

            if (_valuesAssigned)
            {
                if (newV < Minimum)
                    RangeStart = Minimum;
                else if (newV > Maximum)
                    RangeStart = Maximum;

                SyncActiveRectangle();

                if (newV > RangeEnd)
                    RangeEnd = newV;
            }

            SyncThumbs();
        }
        else if (change.Property == RangeEndProperty)
        {
            _maxSet = true;

            if (!_valuesAssigned)
                return;

            var newV = change.GetNewValue<double>();
            RangeMaxToStepFrequency();

            if (_valuesAssigned)
            {
                if (newV < Minimum)
                    RangeEnd = Minimum;
                else if (newV > Maximum)
                    RangeEnd = Maximum;

                SyncActiveRectangle();

                if (newV < RangeStart)
                    RangeStart = newV;
            }

            SyncThumbs();
        }
        else if (change.Property == MinimumProperty)
        {
            if (!_valuesAssigned)
                return;

            var (oldV, newV) = change.GetOldAndNewValue<double>();

            if (Maximum < newV)
                Maximum = newV + Epsilon;

            if (RangeStart < newV)
                RangeStart = newV;

            if (RangeEnd < newV)
                RangeEnd = newV;

            if (newV != oldV)
                SyncThumbs();
        }
        else if (change.Property == MaximumProperty)
        {
            if (!_valuesAssigned)
                return;

            var (oldV, newV) = change.GetOldAndNewValue<double>();

            if (Minimum > newV)
                Maximum = newV + Epsilon;

            if (RangeEnd > newV)
                RangeEnd = newV;

            if (RangeStart > newV)
                RangeStart = newV;

            if (newV != oldV)
                SyncThumbs();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        VerifyValues();
        _valuesAssigned = true;

        _activeRectangle = e.NameScope.Get<Rectangle>("ActiveRectangle");
        _minThumb = e.NameScope.Get<Thumb>("MinThumb");
        _maxThumb = e.NameScope.Get<Thumb>("MaxThumb");
        _containerCanvas = e.NameScope.Get<Canvas>("ContainerCanvas");
        _toolTip = e.NameScope.Find<Control>("ToolTip");
        _toolTipText = e.NameScope.Find<TextBlock>("ToolTipText");

        if (_toolTip != null)
        {
            if (_toolTip.Parent is Panel p)
                p.Children.Remove(_toolTip);
        }

        _minThumb.DragCompleted += HandleThumbDragCompleted;
        _minThumb.DragDelta += MinThumbDragDelta;
        _minThumb.DragStarted += MinThumbDragStarted;
        _minThumb.KeyDown += MinThumbKeyDown;
        _minThumb.KeyUp += ThumbKeyUp;

        _maxThumb.DragCompleted += ThumbDragCompleted;
        _maxThumb.DragDelta += MaxThumbDragDelta;
        _maxThumb.DragStarted += MaxThumbDragStarted;
        _maxThumb.KeyDown += MaxThumbKeyDown;
        _maxThumb.KeyUp += ThumbKeyUp;

        _containerCanvas.SizeChanged += ContainerCanvasSizeChanged;
        _containerCanvas.PointerPressed += ContainerCanvasPointerPressed;
        _containerCanvas.PointerMoved += ContainerCanvasPointerMoved;
        _containerCanvas.PointerReleased += ContainerCanvasPointerReleased;
        _containerCanvas.PointerExited += ContainerCanvasPointerExited;
    }

    protected virtual void OnThumbDragStarted(VectorEventArgs e)
    {
        ThumbDragStarted?.Invoke(this, e);
    }

    protected virtual void OnThumbDragCompleted(VectorEventArgs e)
    {
        ThumbDragCompleted?.Invoke(this, e);
    }

    protected virtual void OnValueChanged(RangeChangedEventArgs e)
    {
        ValueChanged?.Invoke(this, e);
    }

    private void MinThumbDragDelta(object sender, VectorEventArgs e)
    {
        _absolutePosition += e.Vector.X;

        RangeStart = DragThumb(_minThumb, 0, Canvas.GetLeft(_maxThumb), _absolutePosition);

        if (_toolTipText != null)
        {
            UpdateToolTipText(RangeStart);
        }
    }

    private void MaxThumbDragDelta(object sender, VectorEventArgs e)
    {
        _absolutePosition += e.Vector.X;

        RangeEnd = DragThumb(_maxThumb, Canvas.GetLeft(_minThumb), DragWidth, _absolutePosition);

        if (_toolTipText != null)
        {
            UpdateToolTipText(RangeEnd);
        }
    }

    private void MinThumbDragStarted(object sender, VectorEventArgs e)
    {
        _isDragging = true;
        OnThumbDragStarted(e);
        HandleThumbDragStarted(_minThumb);
    }

    private void MaxThumbDragStarted(object sender, VectorEventArgs e)
    {
        _isDragging = true;
        OnThumbDragStarted(e);
        HandleThumbDragStarted(_maxThumb);
    }

    private void HandleThumbDragCompleted(object sender, VectorEventArgs e)
    {
        _isDragging = false;
        OnThumbDragCompleted(e);
        OnValueChanged(sender.Equals(_minThumb) ? 
            new RangeChangedEventArgs(_oldValue, RangeStart, RangeSelectorProperty.RangeStartValue) : 
            new RangeChangedEventArgs(_oldValue, RangeEnd, RangeSelectorProperty.RangeEndValue));
        SyncThumbs();

        if (_toolTip != null)
        {
            ToolTip.SetIsOpen(sender as Control, false);
            UnParentToolTip(_toolTip);
            ToolTip.SetTip(sender as Control, null);
        }
    }

    private double DragThumb(Thumb thumb, double min, double max, double nextPos)
    {
        nextPos = Math.Max(min, nextPos);
        nextPos = Math.Min(max, nextPos);

        Canvas.SetLeft(thumb, nextPos);

        return Minimum + ((nextPos / DragWidth) * (Maximum - Minimum));
    }

    private void HandleThumbDragStarted(Thumb thumb)
    {
        var useMin = thumb == _minThumb;
        var otherThumb = useMin ? _maxThumb : _minThumb;

        _absolutePosition = Canvas.GetLeft(thumb);
        thumb.ZIndex = 10;
        otherThumb.ZIndex = 0;
        _oldValue = RangeStart;

        if (_toolTipText != null && _toolTip != null)
        {
            if (!ToolTip.GetIsOpen(thumb))
            {
                UnParentToolTip(_toolTip);
                ToolTip.SetTip(thumb, _toolTip);
                ToolTip.SetIsOpen(thumb, true);
            }

            UpdateToolTipText(useMin ? RangeStart : RangeEnd);
        }
    }

    private void MinThumbKeyDown(object sender, KeyEventArgs e)
    {
        //switch (e.Key)
        //{
        //    case VirtualKey.Left:
        //        RangeStart -= StepFrequency;
        //        SyncThumbs(fromMinKeyDown: true);
        //        if (_toolTip != null)
        //        {
        //            _toolTip.Visibility = Visibility.Visible;
        //        }

        //        e.Handled = true;
        //        break;
        //    case VirtualKey.Right:
        //        RangeStart += StepFrequency;
        //        SyncThumbs(fromMinKeyDown: true);
        //        if (_toolTip != null)
        //        {
        //            _toolTip.Visibility = Visibility.Visible;
        //        }

        //        e.Handled = true;
        //        break;
        //}
    }

    private void MaxThumbKeyDown(object sender, KeyEventArgs e)
    {
        //switch (e.Key)
        //{
        //    case VirtualKey.Left:
        //        RangeEnd -= StepFrequency;
        //        SyncThumbs(fromMaxKeyDown: true);
        //        if (_toolTip != null)
        //        {
        //            _toolTip.Visibility = Visibility.Visible;
        //        }

        //        e.Handled = true;
        //        break;
        //    case VirtualKey.Right:
        //        RangeEnd += StepFrequency;
        //        SyncThumbs(fromMaxKeyDown: true);
        //        if (_toolTip != null)
        //        {
        //            _toolTip.Visibility = Visibility.Visible;
        //        }

        //        e.Handled = true;
        //        break;
        //}
    }

    private void ThumbKeyUp(object sender, KeyEventArgs e)
    {
        //switch (e.Key)
        //{
        //    case VirtualKey.Left:
        //    case VirtualKey.Right:
        //        if (_toolTip != null)
        //        {
        //            keyDebounceTimer.Debounce(
        //                () => _toolTip.Visibility = Visibility.Collapsed,
        //                TimeToHideToolTipOnKeyUp);
        //        }

        //        e.Handled = true;
        //        break;
        //}
    }

    private void ContainerCanvasPointerExited(object sender, PointerEventArgs e)
    {
        var position = e.GetCurrentPoint(_containerCanvas).Position.X;
        var normalizedPosition = ((position / DragWidth) * (Maximum - Minimum)) + Minimum;

        if (_pointerManipulatingMin)
        {
            _pointerManipulatingMin = false;
            _containerCanvas.IsHitTestVisible = true;
            OnValueChanged(new RangeChangedEventArgs(RangeStart, normalizedPosition, RangeSelectorProperty.RangeStartValue));
        }
        else if (_pointerManipulatingMax)
        {
            _pointerManipulatingMax = false;
            _containerCanvas.IsHitTestVisible = true;
            OnValueChanged(new RangeChangedEventArgs(RangeEnd, normalizedPosition, RangeSelectorProperty.RangeEndValue));
        }
    }

    private void ContainerCanvasPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        var position = e.GetCurrentPoint(_containerCanvas).Position.X;
        var normalizedPosition = ((position / DragWidth) * (Maximum - Minimum)) + Minimum;

        if (_pointerManipulatingMin)
        {
            _pointerManipulatingMin = false;
            _containerCanvas.IsHitTestVisible = true;
            OnValueChanged(new RangeChangedEventArgs(RangeStart, normalizedPosition, RangeSelectorProperty.RangeStartValue));
        }
        else if (_pointerManipulatingMax)
        {
            _pointerManipulatingMax = false;
            _containerCanvas.IsHitTestVisible = true;
            OnValueChanged(new RangeChangedEventArgs(RangeEnd, normalizedPosition, RangeSelectorProperty.RangeEndValue));
        }

        SyncThumbs();
    }

    private void ContainerCanvasPointerMoved(object sender, PointerEventArgs e)
    {
        var position = e.GetCurrentPoint(_containerCanvas).Position.X;
        var normalizedPosition = ((position / DragWidth) * (Maximum - Minimum)) + Minimum;

        if (_pointerManipulatingMin && normalizedPosition < RangeEnd)
        {
            RangeStart = DragThumb(_minThumb, 0, Canvas.GetLeft(_maxThumb), position);
            UpdateToolTipText(RangeStart);
        }
        else if (_pointerManipulatingMax && normalizedPosition > RangeStart)
        {
            RangeEnd = DragThumb(_maxThumb, Canvas.GetLeft(_minThumb), DragWidth, position);
            UpdateToolTipText(RangeEnd);
        }
    }

    private void ContainerCanvasPointerPressed(object sender, PointerPressedEventArgs e)
    {
        var position = e.GetCurrentPoint(_containerCanvas).Position.X;
        var normalizedPosition = position * Math.Abs(Maximum - Minimum) / DragWidth;
        double upperValueDiff = Math.Abs(RangeEnd - normalizedPosition);
        double lowerValueDiff = Math.Abs(RangeStart - normalizedPosition);

        if (upperValueDiff < lowerValueDiff)
        {
            RangeEnd = normalizedPosition;
            _pointerManipulatingMax = true;
            HandleThumbDragStarted(_maxThumb);
        }
        else
        {
            RangeStart = normalizedPosition;
            _pointerManipulatingMin = true;
            HandleThumbDragStarted(_minThumb);
        }

        SyncThumbs();
    }

    private void UpdateToolTipText(double newValue)
    {
        if (_toolTipText != null)
        {
            _toolTipText.Text = newValue.ToString("0.##");
        }
    }

    private void VerifyValues()
    {
        if (Minimum > Maximum)
        {
            Minimum = Maximum;
            Maximum = Maximum;
        }

        if (Minimum == Maximum)
        {
            Maximum += Epsilon;
        }

        if (!_maxSet)
        {
            RangeEnd = Maximum;
        }

        if (!_minSet)
        {
            RangeStart = Minimum;
        }

        if (RangeStart < Minimum)
        {
            RangeStart = Minimum;
        }

        if (RangeEnd < Minimum)
        {
            RangeEnd = Minimum;
        }

        if (RangeStart > Maximum)
        {
            RangeStart = Maximum;
        }

        if (RangeEnd > Maximum)
        {
            RangeEnd = Maximum;
        }

        if (RangeEnd < RangeStart)
        {
            RangeStart = RangeEnd;
        }
    }

    private void RangeMinToStepFrequency()
    {
        RangeStart = MoveToStepFrequency(RangeStart);
    }

    private void RangeMaxToStepFrequency()
    {
        RangeEnd = MoveToStepFrequency(RangeEnd);
    }

    private double MoveToStepFrequency(double rangeValue)
    {
        double newValue = Minimum + (((int)Math.Round((rangeValue - Minimum) / StepFrequency)) * StepFrequency);

        if (newValue < Minimum)
        {
            return Minimum;
        }
        else if (newValue > Maximum || Maximum - newValue < StepFrequency)
        {
            return Maximum;
        }
        else
        {
            return newValue;
        }
    }

    private void SyncThumbs(bool fromMinKeyDown = false, bool fromMaxKeyDown = false)
    {
        if (_containerCanvas == null || _isDragging)
        {
            return;
        }

        var relativeLeft = ((RangeStart - Minimum) / (Maximum - Minimum)) * DragWidth;
        var relativeRight = ((RangeEnd - Minimum) / (Maximum - Minimum)) * DragWidth;

        Canvas.SetLeft(_minThumb, relativeLeft);
        Canvas.SetLeft(_maxThumb, relativeRight);

        var y = _containerCanvas.Bounds.Height / 2 - _minThumb.Bounds.Height / 2;
        Canvas.SetTop(_minThumb, y);
        Canvas.SetTop(_maxThumb, y);

        if (fromMinKeyDown || fromMaxKeyDown)
        {
            DragThumb(
                fromMinKeyDown ? _minThumb : _maxThumb,
                fromMinKeyDown ? 0 : Canvas.GetLeft(_minThumb),
                fromMinKeyDown ? Canvas.GetLeft(_maxThumb) : DragWidth,
                fromMinKeyDown ? relativeLeft : relativeRight);
            
            if (_toolTipText != null)
            {
                UpdateToolTipText(fromMinKeyDown ? RangeStart : RangeEnd);
            }
        }

        SyncActiveRectangle();
    }

    private void SyncActiveRectangle()
    {
        if (_containerCanvas == null || _minThumb == null || _maxThumb == null)
            return;

        var relativeLeft = Canvas.GetLeft(_minThumb);
        Canvas.SetLeft(_activeRectangle, relativeLeft);
        Canvas.SetTop(_activeRectangle, (_containerCanvas.Bounds.Height - _activeRectangle.Bounds.Height) / 2);
        _activeRectangle.Width = Math.Max(0, Canvas.GetLeft(_maxThumb) - Canvas.GetLeft(_minThumb));
    }

    private void ContainerCanvasSizeChanged(object sender, SizeChangedEventArgs e)
    {
        SyncThumbs();
    }

    private static void UnParentToolTip(Control c)
    {
        if (c.Parent is Panel p)
        {
            p.Children.Remove(c);
        }
        else if (c.Parent is ContentControl cc)
        {
            cc.Content = null;
        }
        else if (c.Parent is Decorator d)
        {
            d.Child = null;
        }
    }


    private Rectangle _activeRectangle;
    private Thumb _minThumb;
    private Thumb _maxThumb;
    private Canvas _containerCanvas;
    private double _oldValue;
    private bool _valuesAssigned;
    private bool _minSet;
    private bool _maxSet;
    private bool _pointerManipulatingMin;
    private bool _pointerManipulatingMax;
    private double _absolutePosition;
    private Control _toolTip;
    private TextBlock _toolTipText;
    private const double Epsilon = 0.01;
    private bool _isDragging;
}
