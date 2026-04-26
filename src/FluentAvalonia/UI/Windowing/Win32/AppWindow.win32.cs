using System.Runtime.CompilerServices;
using Avalonia.Controls;
using FluentAvalonia.Interop;

namespace FluentAvalonia.UI.Windowing;

public partial class AppWindow
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void InitializeAppWindow()
    {
        IsWindows = true;
        IsWindows11 = OSVersionHelper.IsWindows11();

        _win32Manager = new Win32WindowManager(this);

        // Force AppWindow into darkmode at the system level
        Win32Interop.ApplyTheme(_win32Manager.Hwnd, true);

        // NOTE FOR FUTURE: 
        // Do NOT enable these properties, doing so causes a clash of logic between here and
        // the actual window logic within avalonia leading to the window shrinking when restoring
        // from maximized or minimized state. 
        //ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        //ExtendClientAreaToDecorationsHint = true;
        PseudoClasses.Add(":windows");

        PlatformFeatures = new Win32AppWindowFeatures(this);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void HandleFullScreenTransition(WindowState state)
    {
        if (state == WindowState.FullScreen && !_win32Manager.IsFullscreen)
        {
            _win32Manager.GoToFullScreen();
        }
        else if (_win32Manager.IsFullscreen)
        {
            _win32Manager.RestoreFromFullScreen();
        }
    }

    private Win32WindowManager _win32Manager;
}
