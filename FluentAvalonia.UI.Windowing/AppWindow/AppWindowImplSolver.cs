using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Platform;
using FluentAvalonia.Interop;

namespace FluentAvalonia.UI.Windowing;

internal static class AppWindowImplSolver
{
    public static IWindowImpl GetWindowImpl()
    {
        bool useAppWindowImpl = OSVersionHelper.IsWindows() && !Design.IsDesignMode;

        if (useAppWindowImpl)
        {
            return GetAppWindowImpl();
        }

        return PlatformManager.CreateWindow();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IWindowImpl GetAppWindowImpl() =>
        new AppWindowImpl();
}
