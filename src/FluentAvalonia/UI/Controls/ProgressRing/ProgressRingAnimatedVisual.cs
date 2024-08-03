using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using Avalonia.VisualTree;
using SkiaSharp;

namespace FluentAvalonia.UI.Controls.Primitives;

/// <summary>
/// Represents the animated visual source for a <see cref="ProgressRing"/>
/// </summary>
/// <remarks>
/// This class is only public for Xaml support in the control template of the ProgressRing
/// </remarks>
public sealed class ProgressRingAnimatedVisual : Control
{
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var parent = this.FindAncestorOfType<ProgressRing>();

        bool indeterminate = parent.IsIndeterminate;
        _handler = new CustomCompHandler(parent.Minimum, parent.Maximum, parent.Value,
            parent.IsActive, parent.Background, parent.Foreground);

        if (_sfc == null)
        {
            var vis = ElementComposition.GetElementVisual(this);
            var comp = vis.Compositor;

            _sfc = comp.CreateCustomVisual(_handler);
            // The WinUI Animated Visual is 80x80
            _sfc.Size = new Vector(80, 80);

            ElementComposition.SetElementChildVisual(this, _sfc);
        }

        _sfc.SendHandlerMessage(new HandlerMessage(HandlerMessageType.Indeterminate, indeterminate));
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        // The progress ring's aspect ratio is preserved, so we constrain to the smallest dimension we have
        var minSize = Math.Min(e.NewSize.Width, e.NewSize.Height);
        // The animated visual is 80x80, we scale the composition visual to scale up or down accordingly
        _sfc.Scale = new Vector3D(minSize / 80, minSize / 80, 1);
    }

    internal void SetMinimum(double min)
    {
        _sfc?.SendHandlerMessage(new HandlerMessage(HandlerMessageType.Min, (float)min));
    }

    internal void SetMaximum(double max)
    {
        _sfc?.SendHandlerMessage(new HandlerMessage(HandlerMessageType.Max, (float)max));
    }

    internal void SetValue(double val)
    {
        _sfc?.SendHandlerMessage(new HandlerMessage(HandlerMessageType.Value, (float)val));
    }

    internal void SetActive(bool active)
    {
        _sfc?.SendHandlerMessage(new HandlerMessage(HandlerMessageType.Active, active));
    }

    internal void SetIndeterminate(bool indeterminate)
    {
        _sfc?.SendHandlerMessage(new HandlerMessage(HandlerMessageType.Indeterminate, indeterminate));
    }

    internal void SetBackground(IBrush brush)
    {
        if (brush is ISolidColorBrush scb)
        {
            _sfc?.SendHandlerMessage(new HandlerMessage(HandlerMessageType.Background, scb.Color.ToSKColor()));
        }
        else
        {
            _sfc?.SendHandlerMessage(new HandlerMessage(HandlerMessageType.Background, null));
        }
    }

    internal void SetForeground(IBrush brush)
    {
        if (brush is ISolidColorBrush scb)
        {
            _sfc?.SendHandlerMessage(new HandlerMessage(HandlerMessageType.Foreground, scb.Color.ToSKColor()));
        }
        else
        {
            _sfc?.SendHandlerMessage(new HandlerMessage(HandlerMessageType.Foreground, SKColors.Transparent));
        }
    }

    private CustomCompHandler _handler;
    private CompositionCustomVisual _sfc;

    private enum HandlerMessageType
    {
        Background,
        Foreground,
        Min,
        Max,
        Value,
        Active,
        Indeterminate
    }

    private class HandlerMessage
    {
        public HandlerMessage(HandlerMessageType type, object data)
        {
            MessageType = type;
            Data = data;
        }

        public HandlerMessageType MessageType { get; }

        public object Data { get; }
    }

    private class CustomCompHandler : CompositionCustomVisualHandler
    {
        public CustomCompHandler(double minimum, double maximum, double value, bool isActive,
            IBrush background, IBrush foreground)
        {
            _min = (float)minimum;
            _max = (float)maximum;
            _value = (float)value;
            _active = isActive;
            
            if (background is ISolidColorBrush scb)
            {
                _background = scb.Color.ToSKColor();
            }

            if (foreground is ISolidColorBrush scbF)
            {
                _foreground = scbF.Color.ToSKColor();
            }

            _paint = new SKPaint
            {
                IsAntialias = true,
                IsStroke = true,
                StrokeWidth = 4f,
                StrokeCap = SKStrokeCap.Round
            };

            _path = new SKPath();
        }

        public override void OnRender(ImmediateDrawingContext drawingContext)
        {
            if (!_active)
                return;

            var feat = drawingContext.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            using var lease = feat.Lease();

            var dc = lease.SkCanvas;

            if (_background.HasValue)
            {
                _paint.Color = _background.Value;
                dc.DrawArc(_visualBounds, 0, 360, false, _paint);
            }
            
            _paint.Color = _foreground;
            dc.DrawPath(_path, _paint);
        }

        public override void OnAnimationFrameUpdate()
        {
            Invalidate();
            Update();

            if (_active && (_indeterminate || _isAnimatingToValue))
                RegisterForNextAnimationFrameUpdate();
        }

        private void Update()
        {
            if (_indeterminate)
            {
                // This is entirely determined by sight and reverse engineering
                var now = CompositionNow;
                if (!_lastTime.HasValue)
                    _lastTime = now;
                var elapsed = now - _lastTime.Value;
                var seconds = elapsed.TotalSeconds;

                if (seconds > _duration)
                {
                    while (seconds > _duration)
                    {
                        seconds -= _duration;
                    }

                    _lastTime = now - TimeSpan.FromSeconds(seconds);
                }

                // Size:
                // 0% - 0
                // 25% - 180
                // 75% - 180
                // 100% - 0

                var progress = (float)(seconds / _duration);

                float size = 0, size2 = 0, position = 0;
                if (progress < 0.25)
                {
                    size = 180 * (progress / 0.25f);
                }
                else if (progress >= 0.75)
                {
                    size = 180 * ((1 - progress) / 0.25f);
                }
                else
                {
                    size = 180;
                }

                size2 = size / 2;

                // 3 full rotations complete the animation, 360 * 3 = 1080
                position = 1080 * progress;

                _path.Reset();
                _path.MoveTo(40, 10);
                _path.AddArc(_visualBounds, -90 + (position - size2), size);
            }
            else if (_isAnimatingToValue)
            {
                var now = CompositionNow;
                if (!_lastTime.HasValue)
                    _lastTime = now;
                var elapsed = now - _lastTime.Value;
                var seconds = elapsed.TotalSeconds;

                var progress = (float)(seconds / _duration);
                if (progress >= 1)
                {
                    _isAnimatingToValue = false;
                    _lastTime = null;
                    progress = 1;
                }

                var dV = _value - _lastValue;
                var size = _lastValue + (dV * progress);
                _path.Reset();
                _path.MoveTo(40, 10);
                _path.AddArc(_visualBounds, -90, 360 * (size - _min) / (_max - _min));
            }
            else
            {                
                _path.Reset();
                _path.MoveTo(40, 10);
                _path.AddArc(_visualBounds, -90, 360 * (_value - _min) / (_max - _min));
            }
        }

        public override void OnMessage(object message)
        {
            if (message is HandlerMessage hm)
            {
                switch (hm.MessageType)
                {
                    case HandlerMessageType.Min:
                        _min = (float)hm.Data;
                        break;

                    case HandlerMessageType.Max:
                        _max = (float)hm.Data;
                        break;

                    case HandlerMessageType.Value:
                        {
                            var next = (float)hm.Data;
                            _lastValue = _value;

                            // No animation if we drop the value
                            if (next <= _value)
                            {
                                _value = next;
                                _isAnimatingToValue = false;
                            }
                            else
                            {
                                // Increasing, animate to new value
                                _value = next;
                                _isAnimatingToValue = true;
                                RegisterForNextAnimationFrameUpdate();
                                return;
                            }
                        }                        
                        break;

                    case HandlerMessageType.Active:
                        _active = (bool)hm.Data;
                        if (_active && _indeterminate)
                        {
                            RegisterForNextAnimationFrameUpdate();
                            return;
                        }
                        else
                        {
                            _lastTime = null;
                        }
                        break;

                    case HandlerMessageType.Indeterminate:
                        _indeterminate = (bool)hm.Data;
                        if (_indeterminate && _active)
                        {
                            RegisterForNextAnimationFrameUpdate();
                            return;
                        }
                        else
                        {
                            _lastTime = null;
                        }
                        break;

                    case HandlerMessageType.Background:
                        {
                            if (hm.Data is SKColor c)
                            {
                                _background = c;
                            }
                            else
                            {
                                _background = null;
                            }
                        }
                        break;

                    case HandlerMessageType.Foreground:
                        {
                            if (hm.Data is SKColor c)
                            {
                                _foreground = c;
                            }
                            else
                            {
                                _foreground = SKColors.Transparent;
                            }
                        }
                        break;
                }

                Update();
                Invalidate();
            }
        }

        private TimeSpan? _lastTime;
        private float _duration = 2;
        private readonly SKPaint _paint;
        private readonly SKPath _path;
        private readonly SKRect _visualBounds = new SKRect(10, 10, 70, 70);

        private SKColor? _background;
        private SKColor _foreground;
        private float _min, _max, _value;
        private bool _indeterminate;
        private bool _active;
        private bool _isAnimatingToValue;
        private float _lastValue;
    }
}
