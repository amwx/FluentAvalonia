using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using FluentAvalonia.UI.Media;

namespace FluentAvalonia.Styling;

internal static class LinuxThemeResolver
{
    public static Color2? TryLoadAccentColor()
    {
        if (_config == null)
        {
            TryLoadLinuxDesktopEnvironmentConfig();
        }

        Color2? aColor = null;
        if (_config != null)
        {
            switch (_desktopEnvironment)
            {
                case DesktopEnvironment.KDE:
                    var match = new Regex("^AccentColor=(\\d+),(\\d+),(\\d+)$", RegexOptions.Multiline)
                        .Match(_config);
                    if (!match.Success)
                    {
                        // Accent color is from the current color scheme
                        match = new Regex("^\\[Colors:Selection\\].*?BackgroundNormal=(\\d+),(\\d+),(\\d+)",
                                RegexOptions.Multiline | RegexOptions.Singleline)
                            .Match(_config);
                    }

                    if (match.Success)
                    {
                        aColor = Color2.FromRGB(byte.Parse(match.Groups[1].Value), byte.Parse(match.Groups[2].Value),
                            byte.Parse(match.Groups[3].Value));
                    }
                    break;
                case DesktopEnvironment.LXQt:
                    match =
                        new Regex("^highlight_color=#([\\da-f]{2})([\\da-f]{2})([\\da-f]{2})$", RegexOptions.Multiline)
                            .Match(_config);
                    if (match.Success)
                    {
                        aColor = Color2.FromRGB(Convert.ToByte(match.Groups[1].Value, 16),
                            Convert.ToByte(match.Groups[2].Value, 16), Convert.ToByte(match.Groups[3].Value, 16));
                    }
                    break;
                case DesktopEnvironment.LXDE:
                    match = new Regex("selected_bg_color:#([\\da-f]{2}).{2}([\\da-f]{2}).{2}([\\da-f]{2}).{2}")
                        .Match(_config);
                    if (match.Success)
                    {
                        aColor = Color2.FromRGB(Convert.ToByte(match.Groups[1].Value, 16),
                            Convert.ToByte(match.Groups[2].Value, 16), Convert.ToByte(match.Groups[3].Value, 16));
                    }
                    break;
            }
        }

        return aColor;
    }

    public static string TryLoadSystemTheme()
    {
        if (_config == null)
        {
            TryLoadLinuxDesktopEnvironmentConfig();
        }

        if (_config != null)
        {
            switch (_desktopEnvironment)
            {
                case DesktopEnvironment.KDE:
                {
                    var match = new Regex("^ColorScheme=(.*)$", RegexOptions.Multiline)
                        .Match(_config);
                    if (match.Success)
                    {
                        return GetThemeFromName(match.Groups[1].Value);
                    }

                    break;
                }
                case DesktopEnvironment.LXDE:
                {
                    var match = new Regex("^sNet\\/ThemeName=(.*)$", RegexOptions.Multiline).Match(_config);
                    if (match.Success)
                    {
                        return GetThemeFromName(match.Groups[1].Value);
                    }

                    break;
                }
                case DesktopEnvironment.LXQt:
                {
                    var match = new Regex("^theme=(.*)$", RegexOptions.Multiline).Match(_config);
                    if (match.Success)
                    {
                        return GetThemeFromName(match.Groups[1].Value);
                    }

                    break;
                }
            }
        }
        else switch (_desktopEnvironment)
        {
            case DesktopEnvironment.Cinnamon:
                return GetThemeFromName(ReadGsettingsKey("org.cinnamon.desktop.interface", "gtk-theme"));
            case DesktopEnvironment.Other:
                var color = ReadGsettingsKey("org.gnome.desktop.interface", "color-scheme");
                return color switch
                {
                    "prefer-light" => FluentAvaloniaTheme.LightModeString,
                    "prefer-dark" => FluentAvaloniaTheme.DarkModeString,
                    _ => GetThemeFromName(ReadGsettingsKey("org.gnome.desktop.interface", "gtk-theme"))
                };
        }

        return FluentAvaloniaTheme.LightModeString;
    }

    private static void TryLoadLinuxDesktopEnvironmentConfig()
    {
        var config = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var file = _desktopEnvironment switch
        {
            DesktopEnvironment.KDE => Path.Combine(config, "kdeglobals"),
            DesktopEnvironment.LXDE => Path.Combine(config, "lxsession/LXDE/desktop.conf"),
            DesktopEnvironment.LXQt => Path.Combine(config, "lxqt/lxqt.conf"),
            _ => null
        };

        if (file != null)
        {
            try
            {
                _config = File.ReadAllText(file);
            }
            catch { }
        }
    }

    private static string GetThemeFromName(string name)
    {
        return name != null && name.IndexOf("dark", StringComparison.OrdinalIgnoreCase) != -1
            ? FluentAvaloniaTheme.DarkModeString
            : FluentAvaloniaTheme.LightModeString;
    }

    private static string ReadGsettingsKey(string schema, string key)
    {
        var p = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                FileName = "gsettings",
                Arguments = $"get {schema} {key}"
            },
        };
        p.Start();
        p.WaitForExit();

        if (p.ExitCode == 0)
        {
            var result = p.StandardOutput.ReadToEnd().Trim().Replace("'", string.Empty);
            return result.Contains("No such") ? null : result;
        }

        return null;
    }

    private static DesktopEnvironment GetDesktopEnvironment()
    {
        var name = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
        if (name == null)
        {
            return DesktopEnvironment.Other;
        }

        // Some distributions may add their own name to the variable, e.g. "ubuntu:GNOME"
        if (name.Contains("KDE"))
        {
            return DesktopEnvironment.KDE;
        }
        if (name.Contains("GNOME"))
        {
            return DesktopEnvironment.GNOME;
        }
        if (name.Contains("LXDE"))
        {
            return DesktopEnvironment.LXDE;
        }
        if (name.Contains("Cinnamon"))
        {
            return DesktopEnvironment.Cinnamon;
        }

        return name.Contains("LXQt") ? DesktopEnvironment.LXQt : DesktopEnvironment.Other;
    }

    private static string _config;

    private static readonly DesktopEnvironment _desktopEnvironment = GetDesktopEnvironment();

    private enum DesktopEnvironment
    {
        KDE,
        GNOME,
        LXQt,
        LXDE,
        Cinnamon,
        Other
    }
}
