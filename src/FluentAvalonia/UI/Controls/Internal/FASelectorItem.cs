using System.ComponentModel;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls.Internal;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class FASelectorItem : ContentControl, ISelectable
{
    static FASelectorItem()
    {
        SelectableMixin.Attach<FASelectorItem>(IsSelectedProperty);
        FocusableProperty.OverrideDefaultValue<FASelectorItem>(true);
        AutomationProperties.IsOffscreenBehaviorProperty.OverrideDefaultValue<FASelectorItem>(IsOffscreenBehavior.FromClip);
    }

    /// <summary>
    /// Defines the <see cref="IsSelected"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<FASelectorItem>();

    /// <summary>
    /// Gets or sets whether the item is selected
    /// </summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (IgnorePointerId(e.Pointer.Id))
            return;

        if (e.Pointer.Type == PointerType.Mouse)
        {
            _isPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        }
        else
        {
            _isPressed = true;
        }

        if (_isPressed)
            UpdateVisualState();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        ProcessPointerOver(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (IgnorePointerId(e.Pointer.Id))
            return;

        if (_isPressed)
        {
            _isPressed = false;
            UpdateVisualState();
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        ProcessPointerCanceled(e.Pointer);
    }

    private void ProcessPointerOver(PointerEventArgs args)
    {
        if (IgnorePointerId(args.Pointer.Id))
            return;

        var rc = new Rect(Bounds.Size);
        var pt = args.GetPosition(this);
        _isPressed = rc.Contains(pt);
        PseudoClasses.Set(":pointerover", _isPressed);
    }

    private void ProcessPointerCanceled(IPointer args)
    {
        if (IgnorePointerId(args.Id))
            return;

        _isPressed = false;
        ResetTrackedPointerId();
        UpdateVisualState();
    }

    private void ResetTrackedPointerId()
    {
        _trackedPointerId = 0;
    }

    private bool IgnorePointerId(int id)
    {
        if (_trackedPointerId == 0)
        {
            _trackedPointerId = id;
        }
        else if (_trackedPointerId != id)
        {
            return true;
        }

        return false;
    }

    private void UpdateVisualState()
    {
        PseudoClasses.Set(FASharedPseudoclasses.s_pcPressed, _isPressed);
    }

    private bool _isPressed;
    private int _trackedPointerId;
}
