using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;

namespace FluentAvaloniaTests.Helpers;

public static class MouseHelpers
{
    public static void ClickControl(this Control ctrl, Point localPoint, MouseButton button, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        var window = TopLevel.GetTopLevel(ctrl);

        var matrix = ctrl.TransformToVisual(window).Value;
        var point = localPoint.Transform(matrix);

        window.MouseDown(point, button, modifiers);
        window.MouseUp(point, button, modifiers);
    }

    public static void MoveMouseToControl(this Control ctrl, Point localPoint)
    {
        var window = TopLevel.GetTopLevel(ctrl);

        var matrix = ctrl.TransformToVisual(window).Value;
        var point = localPoint.Transform(matrix);

        window.MouseMove(point);
    }
}
