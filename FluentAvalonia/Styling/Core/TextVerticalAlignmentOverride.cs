namespace FluentAvalonia.Styling;

/// <summary>
/// Specifies constants for when text alignment style overrides should be applied
/// See <see cref="FluentAvaloniaTheme.TextAlignmentOverrideBehavior"/> and GH PR #210
/// </summary>
public enum TextVerticalAlignmentOverride
{
    /// <summary>
    /// Specified no text alignment overrides should be used
    /// </summary>
    /// <remarks>
    /// This value should be used if your app is Windows only or if you
    /// know the Segoe font family is installed (or you don't care about 
    /// the text alignment issues)
    /// </remarks>
    Disabled,

    /// <summary>
    /// Specifies text alignment style overrides should be applied only on
    /// non-Windows operating systems
    /// </summary>
    /// <remarks>
    /// This value should be used if you don't care about potential styling 
    /// differences between Windows vs non-Windows and want to maintain 
    /// Fluent design principles on Windows.
    /// </remarks>
    EnabledNonWindows,

    /// <summary>
    /// Specifies text alignment style overrides should always be applied
    /// </summary>
    /// <remarks>
    /// This value should be used if you want to ensure the same look across
    /// operating systems
    /// </remarks>
    AlwaysEnabled
}
