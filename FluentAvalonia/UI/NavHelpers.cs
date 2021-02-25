using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class NavHelpers : AvaloniaObject
    {
        public static readonly AttachedProperty<bool> XYFocusKeyboardNavigationProperty =
            AvaloniaProperty.RegisterAttached<NavHelpers, IControl, bool>("XYFocusKeyboardNavigation", false, true);

        public static bool GetXYFocusKeyboardNavigation(IControl c)
        {
            return c.GetValue(XYFocusKeyboardNavigationProperty);
        }

        public static void SetXYFocusKeyboardNavigation(IControl c, bool value)
        {
            if (c.GetValue(XYFocusKeyboardNavigationProperty))
            {
                c.RemoveHandler(InputElement.KeyDownEvent, OnElementPreviewKeyDown);
            }

            c.SetValue(XYFocusKeyboardNavigationProperty, value);

            if (value)
            {
                c.AddHandler(InputElement.KeyDownEvent, OnElementPreviewKeyDown, Avalonia.Interactivity.RoutingStrategies.Bubble);
            }
        }

        private static void OnElementPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                var cur = FocusManager.Instance?.Current;
                if (cur != null)
                {
                    var next = KeyboardNavigationHandler.GetNext(cur, NavigationDirection.Next) as Control;
                    if (next != null && next.GetValue(XYFocusKeyboardNavigationProperty) == true)
                    {
                        FocusManager.Instance?.Focus(next, NavigationMethod.Directional);
                    }
                }
            }
            else if (e.Key == Key.Up)
            {
                var cur = FocusManager.Instance?.Current;
                if (cur != null)
                {
                    var next = KeyboardNavigationHandler.GetNext(cur, NavigationDirection.Previous) as Control;
                    if (next != null && next.GetValue(XYFocusKeyboardNavigationProperty) == true)
                    {
                        FocusManager.Instance?.Focus(next, NavigationMethod.Directional);
                    }
                }
            }
        }
    }
}
