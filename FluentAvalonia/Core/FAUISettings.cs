using System;
using System.Runtime.CompilerServices;
using FluentAvalonia.Interop;

namespace FluentAvalonia.Core;

/// <summary>
/// Provides settings related to the behavior of UI elements, like animation, etc.
/// </summary>
public class FAUISettings
{
    static FAUISettings()
    {
        s_Instance = new FAUISettings();
    }

    /// <summary>
    /// Checks whether animations are enabled or have been disabled
    /// </summary>
    public static bool AreAnimationsEnabled()
    {
        return s_Instance._areAnimationsEnabled;
    }

    /// <summary>
    /// Enables or disables animations for the current application
    /// </summary>
    public static void SetAnimationsEnabledAtAppLevel(bool isEnabled)
    {
        s_Instance._areAnimationsEnabled = isEnabled;
    }

    /// <summary>
    /// Gets the minimum size required for a drag-drop operation
    /// </summary>
    public static void GetSystemDragSize(double scaling, out double cxDrag, out double cyDrag)
    {
        cxDrag = 4 * scaling;
        cyDrag = 4 * scaling;
    }

    private static void GetWin32DragSize(double scaling, out double cxDrag, out double cyDrag)
    {
        cxDrag = Win32Interop.GetSystemMetricsForDpi(68, (uint)Math.Round(96 * scaling));
        cyDrag = Win32Interop.GetSystemMetricsForDpi(69, (uint)Math.Round(96 * scaling));
    }

    private static readonly FAUISettings s_Instance;
    private bool _areAnimationsEnabled = false;
}
