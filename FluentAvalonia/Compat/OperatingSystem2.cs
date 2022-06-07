using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
#if NETSTANDARD
using System.Runtime.InteropServices;
#endif

// https://github.com/BeyondDimension/OperatingSystem2/blob/2.0.0/OperatingSystem2.windows.cs

namespace System
{
    internal static class OperatingSystem2
    {
        /// <summary>
        /// Indicates whether the current application is running on Windows.
        /// </summary>
        /// <returns><see langword="true"/> if the current application is running on Windows; <see langword="false"/> otherwise.</returns>
        [SupportedOSPlatformGuard("windows")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWindows() =>
#if NET5_0_OR_GREATER
            OperatingSystem.IsWindows();
#else
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif

        /// <summary>
        /// Indicates whether the current application is running on macOS.
        /// </summary>
        /// <returns><see langword="true"/> if the current application is running on macOS; <see langword="false"/> otherwise.</returns>
        [SupportedOSPlatformGuard("macos")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMacOS() =>
#if NET5_0_OR_GREATER
            OperatingSystem.IsMacOS();
#else
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#endif

        /// <summary>
        /// Indicates whether the current application is running on Linux.
        /// </summary>
        /// <returns><see langword="true"/> if the current application is running on Linux; <see langword="false"/> otherwise.</returns>
        [SupportedOSPlatformGuard("linux")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLinux() =>
#if NET5_0_OR_GREATER
            OperatingSystem.IsLinux();
#else
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#endif

#if !NET5_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetInt32(int value) => value < 0 ? 0 : value;

        private static bool IsVersionAtLeast(int major_left, int minor_left, int build_left, int revision_left, int major_right, int minor_right, int build_right, int revision_right)
        {
            if (major_left > major_right)
                return true;
            if (major_left < major_right)
                return false;

            if (minor_left > minor_right)
                return true;
            if (minor_left < minor_right)
                return false;

            if (build_left > build_right)
                return true;
            if (build_left < build_right)
                return false;

            if (revision_left > revision_right)
                return true;
            if (revision_left < revision_right)
                return false;

            return true;
        }

        private static bool IsVersionAtLeast(Version version_left, int major_right = 0, int minor_right = 0, int build_right = 0, int revision_right = 0)
        {
            var major_left = version_left.Minor;
            var minor_left = GetInt32(version_left.Minor);
            var build_left = GetInt32(version_left.Build);
            var revision_left = GetInt32(version_left.Revision);

            return IsVersionAtLeast(major_left, minor_left, build_left, revision_left
                , major_right, minor_right, build_right, revision_right);
        }

#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Version Version() => Environment.OSVersion.Version;


        [SupportedOSPlatformGuard("windows")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWindowsVersionAtLeast(int major, int minor = 0, int build = 0, int revision = 0)
        {
#if NET5_0_OR_GREATER
            return OperatingSystem.IsWindowsVersionAtLeast(major, minor, build, revision);
#else
            return IsWindows() && IsVersionAtLeast(Version(), major, minor, build, revision);
#endif
        }

        private static bool IsWindows10AtLeast_() => IsWindowsVersionAtLeast(10);
        private static readonly Lazy<bool> s_isWindows10AtLeast = new(IsWindows10AtLeast_);
        /// <summary>
        /// Indicates whether the current application is running on Windows 10 or later.
        /// </summary>
        /// <returns><see langword="true"/> if the current application is running on Windows 10; <see langword="false"/> otherwise.</returns>
        [SupportedOSPlatformGuard("windows10.0")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWindows10AtLeast() => s_isWindows10AtLeast.Value;

        private static bool IsWindows11AtLeast_() => IsWindowsVersionAtLeast(10, 0, 22000);
        private static readonly Lazy<bool> s_isWindows11AtLeast = new(IsWindows11AtLeast_);
        /// <summary>
        /// Indicates whether the current application is running on Windows 11 or later.
        /// </summary>
        /// <returns><see langword="true"/> if the current application is running on Windows 11; <see langword="false"/> otherwise.</returns>
        [SupportedOSPlatformGuard("windows10.0.22000")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWindows11AtLeast() => s_isWindows11AtLeast.Value;
    }
}
