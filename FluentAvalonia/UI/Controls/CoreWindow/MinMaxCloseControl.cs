using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System;
using Avalonia.Rendering;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace FluentAvalonia.UI.Controls.Primitives;

public class MinMaxCloseControl : TemplatedControl
{
    public MinMaxCloseControl()
    {
        KeyboardNavigation.SetIsTabStop(this, false);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_minimizeButton != null)
            _minimizeButton.Click -= OnButtonClick;

        if (_maximizeButton != null)
            _maximizeButton.Click -= OnButtonClick;

        if (_closeButton != null)
            _closeButton.Click -= OnButtonClick;

        base.OnApplyTemplate(e);

        _minimizeButton = e.NameScope.Find<Button>("MinimizeButton");
        if (_minimizeButton != null)
            _minimizeButton.Click += OnButtonClick;

        _maximizeButton = e.NameScope.Find<Button>("MaxRestoreButton");
        if (_maximizeButton != null)
            _maximizeButton.Click += OnButtonClick;

        _closeButton = e.NameScope.Find<Button>("CloseButton");
        if (_closeButton != null)
            _closeButton.Click += OnButtonClick;
    }

    internal Button MinimizeButton => _minimizeButton;

    internal Button MaximizeButton => _maximizeButton;

    internal Button CloseButton => _closeButton;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _owner = this.VisualRoot as CoreWindow;

        if (_owner != null)
        {
            _windowStateObservable = _owner.GetObservable(Window.WindowStateProperty)
                .Subscribe(OnWindowStateChanged);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _windowStateObservable?.Dispose();
        _windowStateObservable = null;
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        if (_owner == null)
            return;

        if (sender == _minimizeButton)
        {
            _owner.WindowState = WindowState.Minimized;
        }
        else if (sender == _maximizeButton)
        {
            if (_owner.WindowState == WindowState.Maximized)
            {
                _owner.WindowState = WindowState.Normal;
            }
            else if (_owner.WindowState == WindowState.Normal)
            {
                _owner.WindowState = WindowState.Maximized;
            }
        }
        else if (sender == _closeButton)
        {
            _owner.Close();
        }
    }

    private void OnWindowStateChanged(WindowState state)
    {
        PseudoClasses.Set(":maximized", state == WindowState.Maximized);
    }

    internal bool HitTestMaxButton(Point pos)
    {
        if (_maximizeButton != null)
        {
            // v2 Bug in compositing renderer prevents HitTestCustom

            var test = (VisualRoot as IInputElement)?.InputHitTest(pos);
            if (test == _maximizeButton)
            {
                return true;
            }
            else
            {
                var vis = test as IVisual;
                
                while (vis != null)
                {
                    if (vis == _maximizeButton)
                        return true;

                    vis = vis.VisualParent;
                }
            }

            //return _maximizeButton.HitTestCustom(pos);
        }


        return false;
    }

    internal void FakeMaximizeHover(bool hover)
    {
        if (_maximizeButton != null)
        {
            // We can't set the IsPointerOver property b/c it's readonly and that make things angry
            // so we'll just force set the Pseudoclass
            ((IPseudoClasses)_maximizeButton.Classes).Set(":pointerover", hover);
            //_maximizeButton.SetValue(InputElement.IsPointerOverProperty, hover);
        }
    }

    internal void FakeMaximizePressed(bool pressed)
    {
        if (_maximizeButton != null)
        {
            _maximizeButton.SetValue(Button.IsPressedProperty, pressed);
        }
    }

    internal void FakeMaximizeClick()
    {
        OnButtonClick(_maximizeButton, null);
    }

    private IDisposable _windowStateObservable;
    private CoreWindow _owner;
    private Button _minimizeButton;
    private Button _maximizeButton;
    private Button _closeButton;
}
