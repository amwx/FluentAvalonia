namespace FluentAvalonia.Interop;

/// <summary>
/// Helper alowed to detect OS - keeping the IsWindows11 helper method here, all other
/// helpers pre2.5 are now replaced with direct calls to "OperatingSystem" class
/// </summary>
internal static class OSVersionHelper
{
    /// <summary>
    /// Return if current OS is Windows 11
    /// </summary>
    public static bool IsWindows11() =>
        OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);

    /// <summary>
    /// Return if current OS is at least Windows 10 Version 1607
    /// </summary>
    public static bool IsAtLeastWindows10_1607() =>
        OperatingSystem.IsWindowsVersionAtLeast(10, 0, 14393);

    /// <summary>
    /// Return if current OS is at least Windows 10 Version 1809
    /// </summary>
    public static bool IsAtLeastWindows10_1809() =>
        OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763);

    /// <summary>
    /// Return if current OS is at least Windows 10 Version 1903
    /// </summary>
    public static bool IsAtLeastWindows10_1903() =>
        OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362);
}
