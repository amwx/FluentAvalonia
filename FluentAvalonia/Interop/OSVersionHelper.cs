using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FluentAvalonia.Interop;

public static class OSVersionHelper
{
    public static bool IsWindows()
    {
#if NET5_0_OR_GREATER
        return OperatingSystem.IsWindows();
#else
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
    }

    public static bool IsMacOS()
    {
#if NET5_0_OR_GREATER
        return OperatingSystem.IsMacOS();
#else
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#endif
    }

    public static bool IsLinux()
    {
#if NET5_0_OR_GREATER
        return OperatingSystem.IsLinux();
#else
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#endif
    }

    public static bool IsWindows11()
    {
#if NET5_0_OR_GREATER
        return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);
#else
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return IsWindowsVersionAtLeast(10, 0, 22000);
        }

        return false;
#endif
    }

    public static bool IsWindowsAtLeast(int major, int minor, int build)
    {
#if NET5_0_OR_GREATER
        return OperatingSystem.IsWindowsVersionAtLeast(major, minor, build);
#else
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return IsWindowsVersionAtLeast(major, minor, build);
        }

        return false;
#endif
    }

    /// <summary>
    /// Use RtlGetVersion to check version info on Windows
    /// </summary>
    /// <param name="major"></param>
    /// <param name="minor"></param>
    /// <param name="build"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe bool IsWindowsVersionAtLeast(int major, int minor = 0, int build = 0)
    {
        OSVERSIONINFOEX osInfo = new OSVERSIONINFOEX 
        { 
            OSVersionInfoSize = (uint)sizeof(OSVERSIONINFOEX)
        };

        if (Win32Interop.RtlGetVersion(&osInfo) == 0)
        {
            if (major != osInfo.MajorVersion)
            {
                return osInfo.MajorVersion > major;
            }
            if (minor != osInfo.MinorVersion)
            {
                return osInfo.MinorVersion > minor;
            }
            if (build != osInfo.BuildNumber)
            {
                return osInfo.BuildNumber > build;
            }           
        }

        return false;
    }
}
