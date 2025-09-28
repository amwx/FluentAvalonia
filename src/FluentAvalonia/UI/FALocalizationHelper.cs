using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

using Avalonia.Platform;

namespace FluentAvalonia.UI;

/// <summary>
/// Helper class for storing localized string for FluentAvalonia/WinUI controls
/// </summary>
/// <remarks>
/// The string resources are taken from the WinUI repo. Not all resources in WinUI
/// may be available here, only those that are known to be used in a control
/// </remarks>
public partial class FALocalizationHelper
{
    private FALocalizationHelper()
    {
        using var al = AssetLoader.Open(new Uri("avares://FluentAvalonia/Assets/ControlStrings.json"));
        _mappings = JsonSerializer.Deserialize(al, FALocalizationJsonSerializerContext.Default.LocalizationMap);
    }

    static FALocalizationHelper()
    {
        Instance = new FALocalizationHelper();
    }

    public static FALocalizationHelper Instance { get; }

    /// <summary>
    /// Gets a string resource by the specified name using the CurrentUICulture
    /// </summary>
    public string GetLocalizedStringResource(string resName) =>
        GetLocalizedStringResource(CultureInfo.CurrentUICulture, resName);

    /// <summary>
    /// Gets a string resource by the specified name and using the specified culture
    /// </summary>
    /// <remarks>
    /// InvariantCulture is not supported here and will default to en-US
    /// </remarks>
    public string GetLocalizedStringResource(CultureInfo ci, string resName)
    {
        string cultureName;

        // If running in globalization-invariant mode, always use "en-US" fallback
        if (ci == CultureInfo.InvariantCulture)
        {
            cultureName = s_enUS;
        }
        else
        {
            cultureName = ci.Name;
        }

        if (_mappings.ContainsKey(resName))
        {
            var cultureMap = _mappings[resName];

            if (cultureMap.ContainsKey(cultureName))
            {
                return cultureMap[cultureName];
            }
            else if (cultureMap.ContainsKey(s_enUS))
            {
                return cultureMap[s_enUS];
            }
        }

        return string.Empty;
    }

    // <ResourceName, Entries>
    private readonly LocalizationMap _mappings;
    private static readonly string s_enUS = "en-US";

    /// <summary>
    /// Dictionary of language entries for a resource name. &lt;language, value&gt; where
    /// language is the abbreviated name, e.g., en-US
    /// </summary>
    public class LocalizationEntry : Dictionary<string, string>
    {
        public LocalizationEntry()
            : base(StringComparer.InvariantCultureIgnoreCase)
        {

        }
    }

    private class LocalizationMap : Dictionary<string, LocalizationEntry>
    {
        public LocalizationMap()
            : base(StringComparer.InvariantCultureIgnoreCase)
        {

        }
    }

    [JsonSerializable(typeof(LocalizationMap))]
    private partial class FALocalizationJsonSerializerContext : JsonSerializerContext
    {
    }
}
