﻿using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Platform;

namespace FluentAvalonia.UI;


[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(LocalizationEntry))]
[JsonSerializable(typeof(LocalizationMap))]
internal partial class LocalizationSourceGenerationContext : JsonSerializerContext;

/// <summary>
/// Dictionary of language entries for a resource name. &lt;language, value&gt; where
/// language is the abbreviated name, e.g., en-US
/// </summary>
internal sealed class LocalizationEntry() : Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

internal sealed class LocalizationMap()
    : Dictionary<string, LocalizationEntry>(StringComparer.InvariantCultureIgnoreCase);

/// <summary>
/// Helper class for storing localized string for FluentAvalonia/WinUI controls
/// </summary>
/// <remarks>
/// The string resources are taken from the WinUI repo. Not all resources in WinUI
/// may be available here, only those that are known to be used in a control
/// </remarks>
public class FALocalizationHelper
{
    private FALocalizationHelper()
    {
        using var al = AssetLoader.Open(new Uri("avares://FluentAvalonia/Assets/ControlStrings.json"));

        KeepType<LocalizationMap>();
        KeepType<LocalizationEntry>();
        _mappings = JsonSerializer.Deserialize(al,LocalizationSourceGenerationContext.Default.LocalizationMap);

        static void KeepType<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
            T>() { }
    }

    static FALocalizationHelper()
    {
        Instance = new FALocalizationHelper();
    }

    public static FALocalizationHelper Instance { get; }

    /// <summary>
    /// Gets a string resource by the specified name using the CurrentCulture
    /// </summary>
    public string GetLocalizedStringResource(string resName) =>
        GetLocalizedStringResource(CultureInfo.CurrentCulture, resName);

    /// <summary>
    /// Gets a string resource by the specified name and using the specified culture
    /// </summary>
    /// <remarks>
    /// InvariantCulture is not supported here and will default to en-US
    /// </remarks>
    public string GetLocalizedStringResource(CultureInfo ci, string resName)
    {
        // Don't allow InvariantCulture - default to en-us in that case
        if (ci == CultureInfo.InvariantCulture)
            ci = new CultureInfo(s_enUS);

        if (_mappings.ContainsKey(resName))
        {
            if (_mappings[resName].ContainsKey(ci.Name))
            {
                return _mappings[resName][ci.Name];
            }
            else if (_mappings[resName].ContainsKey(s_enUS))
            {
                return _mappings[resName][s_enUS];
            }
        }

        return string.Empty;
    }

    // <ResourceName, Entries>
    private readonly LocalizationMap _mappings;
    private static readonly string s_enUS = "en-US";


    
}
