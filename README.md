# FluentAvalonia

Bringing more of Fluent design and WinUI controls into Avalonia.

[![Nuget](https://img.shields.io/nuget/v/FluentAvaloniaUI?style=flat-square)](https://www.nuget.org/packages/FluentAvaloniaUI/)
(NOTE: nuget package is under id FluentAvaloniaUI)

Currently Targets: Avalonia 0.10.10 & .net core 5.0

Note: Windows 7, 8/8.1 are not supported by FluentAvalonia.

# Getting Started
Place the following in your App.xaml :
    
    Namespace for FluentAvalonia.Styling
    xmlns:sty="using:FluentAvalonia.Styling"
    
    Namespace for Controls
    xmlns:ui="using:FluentAvalonia.UI.Controls"
    xmlns:uip="using:FluentAvalonia.UI.Controls.Primitives"
    
For the most part, FluentAvalonia has been made independent of Avalonia and does not require you to include a reference to adding the Fluent theme from Avalonia (more on these below)

To include the styles for FluentAvalonia, add the following to your App.xaml (or .axaml)

    <sty:FluentAvaloniaTheme />
    
By default, FluentAvalonia is now using the new WinUI styles that have been rolling out since November 2020. These are still a work in progress both here and in WinUI itself. All controls in core Avalonia also have been provided a template here to provide a cohesive UX, thus making FluentAvalonia independent (style wise).

As of v1.1.5, Fluent v1 styles (what's in core Avalonia) are no longer supported within FluentAvalonia.

FluentAvaloniaTheme has several additional options for customizing:

You can set the theme mode (light or dark) by setting

    RequestedTheme = "Light" or "Dark"

As of v1.1.5, "HighContrast" was added as an option, and on Windows will automatically set. Testing is still ongoing for this.

On Windows you can set the following: 

    UseSegoeUIOnWindows - If true (default), will replace the resource 'ContentControlThemeFontFamily' with SegoeUI, the default Windows font. On Windows 11, this setting will instead default to Segoe UI Variable Text.

    GetUserAccentColor - If true (default), the AccentColor resources are obtained directly from the user's perferences in Windows.

    DefaultToUserTheme - If true (default), will attempt to determine if the user currently has Light or Dark mode enabled and set the app to that theme at startup. This requires Win 10 1809 or greater. This also controls auto setting of High Contrast theme added in v1.1.5.
    
Runtime theme changing is also supported. When initialized, FluentAvaloniaTheme is registered into the AvaloniaLocator, so it can be easily obtained later if you desire to switch themes:

    var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
    thm.RequestedTheme = "Light" or "Dark";
    
You can also force the native Win32 title bar to respect your app theme too (if it differs from the system), however, this is a bit more manual. Call the ForceNativeTitleBarToTheme method and pass in the window you want to change. 

    var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
    thm.ForceNativeTitleBarToTheme(Window);


# What's included so far?
- New WinUI styles (currently being rolled out) for some controls
-- NOTE: These are still being updated in WinUI and not all controls have been updated yet (here and in WinUI)
- Support for new FluentUI Icons
- NavigationView Control (the full thing ported from WinUI)
- Frame control (to assist with Navigation using the NavigationView)
- ContentDialog
- SplitButton, ToggleSplitButton
- DropDownButton
- Editable ComboBox (experimental)
- FontIcon & FontIconSource
- PathIcon & PathIconSource
- BitmapIcon & BitmapIconSource (requires Skia renderer, which is default)
- SymbolIcon & SymbolIconSource (Symbols from new FluentUI Icons)
- ImageIcon & ImageIconSource
- ColorPicker & ColorPickerButton (note: not the WinUI version)
- NumberBox
- InfoBar
- InfoBadge

Future goals:
- ListView/GridView
- CalendarView
- Other WinUI controls as they're rolled out or requested.

For more, check out the sample app

Avalonia : https://github.com/AvaloniaUI/Avalonia  
WinUI : https://github.com/microsoft/microsoft-ui-xaml/  
WinUI & Avalonia are available under MIT licences.

## Licence: MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
