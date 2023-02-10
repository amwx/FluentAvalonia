using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Reactive.Disposables;

namespace FluentAvalonia.UI.Windowing;

public class MinMaxCloseControl : TemplatedControl
{
    public MinMaxCloseControl()
    {
        KeyboardNavigation.SetIsTabStop(this, false);
    }

    internal Button MinimizeButton => _minimizeButton;

    internal Button MaximizeButton => _maximizeButton;

    internal Button CloseButton => _closeButton;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_minimizeButton != null)
            _minimizeButton.Click -= OnButtonClick;

        if (_maximizeButton != null)
            _maximizeButton.Click -= OnButtonClick;

        if (_closeButton != null)
            _closeButton.Click -= OnButtonClick;

        base.OnApplyTemplate(e);

        // Changed to.Get<>(), template shouldn't be messed with to extent these items aren't found
        // anymore, so throw if we have an invalid template
        _minimizeButton = e.NameScope.Get<Button>("MinimizeButton");
        _minimizeButton.Click += OnButtonClick;

        _maximizeButton = e.NameScope.Get<Button>("MaxRestoreButton");
        _maximizeButton.Click += OnButtonClick;

        _closeButton = e.NameScope.Get<Button>("CloseButton");
        _closeButton.Click += OnButtonClick;

        // MinMaxCloseControl should only be used in Template of AppWindow - so TemplatedParent
        // here should A) never be null, and B) Always be AppWindow
        _owner = TemplatedParent as AppWindow;
        _owner.Opened += OwnerWindowOpened;

        if (_owner.ShowAsDialog)
        {
            _minimizeButton.IsVisible = false;
            _maximizeButton.IsVisible = false;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _windowStateObservable?.Dispose();
        _windowStateObservable = null;
    }

    private void OwnerWindowOpened(object sender, EventArgs args)
    {
        _windowStateObservable = new CompositeDisposable(
            _owner.GetObservable(Window.WindowStateProperty).Subscribe(OnWindowStateChanged),
            _owner.GetObservable(WindowBase.IsActiveProperty).Subscribe(OnWindowActiveChanged));
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
        PseudoClasses.Set(":fullscreen", state == WindowState.FullScreen);
    }

    private void OnWindowActiveChanged(bool active)
    {
        ((IPseudoClasses)MinimizeButton.Classes).Set(":inactive", !active);
        ((IPseudoClasses)MaximizeButton.Classes).Set(":inactive", !active);
        ((IPseudoClasses)CloseButton.Classes).Set(":inactive", !active);
    }

    internal bool HitTest(Point p, out bool isMaximize)
    {
        isMaximize = false;
        if (_maximizeButton == null)
            return false;

        var mat = this.TransformToVisual(_owner).Value;
        var bnds = new Rect(Bounds.Size).TransformToAABB(mat);

        if (bnds.Contains(p))
        {
            mat = _maximizeButton.TransformToVisual(_owner).Value;
            bnds = new Rect(_maximizeButton.Bounds.Size).TransformToAABB(mat);

            isMaximize = bnds.Contains(p);
            return true;
        }

        return false;
    }

    internal bool HitTestMaxButton(Point pos)
    {
        if (_maximizeButton == null)
            return false;

        var mat = _maximizeButton.TransformToVisual(_owner).Value;
        var bnds = new Rect(_maximizeButton.Bounds.Size).TransformToAABB(mat);

        return bnds.Contains(pos);
    }

    internal void ClearMaximizedState()
    {
        FakeMaximizePressed(false);
        FakeMaximizeHover(false);
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
    private AppWindow _owner;
    private Button _minimizeButton;
    private Button _maximizeButton;
    private Button _closeButton;
}
