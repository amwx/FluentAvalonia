namespace FluentAvalonia.Core;

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

    private static readonly FAUISettings s_Instance;
    private bool _areAnimationsEnabled = false;
}
