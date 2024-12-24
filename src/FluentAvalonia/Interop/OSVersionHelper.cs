using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FluentAvalonia.Interop;

/// <summary>
/// Helper alowed to detect OS
/// </summary>
public static class OSVersionHelper
{
    /// <summary>
    /// Return if current OS is Windows
    /// </summary>
    /// <returns><see cref="bool"/></returns>
    public static bool IsWindows()
    {
#if NET5_0_OR_GREATER
        return OperatingSystem.IsWindows();
#else
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
    }

    /// <summary>
    /// Return if current OS is MacOS
    /// </summary>
    /// <returns><see cref="bool"/></returns>
    public static bool IsMacOS()
    {
#if NET5_0_OR_GREATER
        return OperatingSystem.IsMacOS();
#else
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#endif
    }

    /// <summary>
    /// Return if current OS is Linux
    /// </summary>
    /// <returns><see cref="bool"/></returns>
    public static bool IsLinux()
    {
#if NET5_0_OR_GREATER
        return OperatingSystem.IsLinux();
#else
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#endif
    }

    /// <summary>
    /// Return if current OS is Windows 11
    /// </summary>
    /// <returns><see cref="bool"/></returns>
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


    /// <summary>
    /// Return if current OS is specified Windows Version
    /// </summary>
    /// <param name="major">major Window Version</param>
    /// <param name="minor">minor Window Version</param>
    /// <param name="build">build Window</param>
    /// <returns></returns>
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
            return true;
        }

        return false;
    }
}
