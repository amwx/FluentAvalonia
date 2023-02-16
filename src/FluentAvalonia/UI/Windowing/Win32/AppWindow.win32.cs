using System.Runtime.CompilerServices;
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

        // NOTE FOR FUTURE: 
        // Do NOT enable these properties, doing so causes a clash of logic between here and
        // the actual window logic within avalonia leading to the window shrinking when restoring
        // from maximized or minimized state. 
        //ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        //ExtendClientAreaToDecorationsHint = true;
        PseudoClasses.Add(":windows");

        PlatformFeatures = new Win32AppWindowFeatures(this);
    }

    private Win32WindowManager _win32Manager;
}
