using System.Runtime.CompilerServices;
using Avalonia.Controls;
using FluentAvalonia.Interop;

namespace FluentAvalonia.UI.Windowing;

public partial class FAAppWindow
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void InitializeAppWindow()
    {
        IsWindows = true;
        IsWindows11 = OSVersionHelper.IsWindows11();

        _win32Manager = new Win32WindowManager(this);

        // Force AppWindow into darkmode at the system level
        Win32Interop.ApplyTheme(_win32Manager.Hwnd, true);
        PseudoClasses.Add(":windows");
        PlatformFeatures = new Win32AppWindowFeatures(this);
    }

    private Win32WindowManager _win32Manager;
}
