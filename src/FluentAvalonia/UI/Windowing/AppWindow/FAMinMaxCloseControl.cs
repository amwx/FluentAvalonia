using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Input;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Windowing;

/// <summary>
/// Represents the caption buttons for a <see cref="FAAppWindow"/>
/// </summary>
public class FAMinMaxCloseControl : TemplatedControl
{
    public FAMinMaxCloseControl()
    {
        KeyboardNavigation.SetIsTabStop(this, false);
    }

    internal Button MinimizeButton => _minimizeButton;

    internal Button MaximizeButton => _maximizeButton;

    internal Button CloseButton => _closeButton;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _minimizeButton?.Click -= OnButtonClick;
        _maximizeButton?.Click -= OnButtonClick;
        _closeButton?.Click -= OnButtonClick;

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
        _owner = TemplatedParent as FAAppWindow;
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
        _windowStateObservable = new FACompositeDisposable(
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

    internal CaptionButton HitTest(Point p)
    {
        if (_maximizeButton == null)
            return CaptionButton.None;

        var mat = this.TransformToVisual(_owner).Value;
        var bnds = new Rect(Bounds.Size).TransformToAABB(mat);

        if (bnds.Contains(p))
        {
            if (HitTestMaxButton(p))
            {
                return CaptionButton.Maximize;
            }
            else if (HitTestMinButton(p))
            {
                return CaptionButton.Minimize;
            }
            else if (HitTestCloseButton(p))
            {
                return CaptionButton.Close;
            }
        }

        return CaptionButton.None;
    }

    internal bool HitTestMaxButton(Point pos)
    {
        if (_maximizeButton == null)
            return false;

        var mat = _maximizeButton.TransformToVisual(_owner).Value;
        var bnds = new Rect(_maximizeButton.Bounds.Size).TransformToAABB(mat);

        return bnds.Contains(pos);
    }

    internal bool HitTestMinButton(Point pos)
    {
        if (_minimizeButton == null)
            return false;

        var mat = _minimizeButton.TransformToVisual(_owner).Value;
        var bnds = new Rect(_minimizeButton.Bounds.Size).TransformToAABB(mat);

        return bnds.Contains(pos);
    }

    internal bool HitTestCloseButton(Point pos)
    {
        if (_closeButton == null)
            return false;

        var mat = _closeButton.TransformToVisual(_owner).Value;
        var bnds = new Rect(_closeButton.Bounds.Size).TransformToAABB(mat);

        return bnds.Contains(pos);
    }

    internal void ClearButtonState()
    {
        ClearButtonState(CaptionButton.Maximize);
        ClearButtonState(CaptionButton.Minimize);
        ClearButtonState(CaptionButton.Close);
    }

    internal void ClearButtonState(CaptionButton button)
    {
        FakeButtonHover(button, false);
        FakeButtonPressed(button, false);
    }

    internal void FakeButtonHover(CaptionButton b, bool hover)
    {
        var button = GetButton(b);

        if (button != null)
        {
            ((IPseudoClasses)button.Classes).Set(":pointerover", hover);
        }
    }

    internal void FakeButtonPressed(CaptionButton b, bool pressed)
    {
        var button = GetButton(b);

        if (button != null)
        {
            ((IPseudoClasses)button.Classes).Set(":pressed", pressed);
        }
    }

    internal void FakeButtonClick(CaptionButton b)
    {
        var button = GetButton(b);

        if (button != null)
            OnButtonClick(button, null);
    }

    private Button GetButton(CaptionButton b)
    {
        return b switch
        {
            CaptionButton.Maximize => _maximizeButton,
            CaptionButton.Minimize => _minimizeButton,
            CaptionButton.Close => _closeButton,
            _ => null
        };
    }

    private IDisposable _windowStateObservable;
    private FAAppWindow _owner;
    private Button _minimizeButton;
    private Button _maximizeButton;
    private Button _closeButton;
}

internal enum CaptionButton
{
    None,
    Minimize,
    Maximize,
    Close
}
