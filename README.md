# FluentAvalonia

Bringing more of Fluent design and WinUI controls into Avalonia.

[![Nuget](https://img.shields.io/nuget/v/FluentAvaloniaUI?style=flat-square)](https://www.nuget.org/packages/FluentAvaloniaUI/)
(NOTE: nuget package is under id FluentAvaloniaUI)

Currently Targets: Avalonia 0.10.6 & .net core 5.0

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
    
By default, FluentAvalonia is now using the new WinUI styles that have been rolling out since last November. These are still a work in progress both here and in WinUI itself. Most controls in core Avalonia also have been provided a template here to provide a cohesive UX, thus making FluentAvalonia independent. See the Core Basic Controls page for the controls that do not have a corresponding template present and you will need to provide one. In these cases, only a template is needed and no additional resources are necessary.

However, you may want to take advantage of something without completely overhauling your app design by upgrading styles. If this is the case, FluentAvaloniaTheme also has a property 'ControlsVersion' which you can set to load the default FluentTheme from Avalonia and still use FluentAvalonia controls. For new controls in FluentAvalonia, the templates themselves have *NOT* been rolled back to the old Fluent style, but the color/brush resources have been remapped to the old resources. This may cause some small differences, but is the easiest and best solution as many controls have complex templates and changes which may not revert easily. Right now, this can only be changed at startup (something goes wrong right now if you try to change it at runtime, and I don't know what).

    <!-- Use the default FluentTheme from Avalonia -->
    <sty:FluentAvaloniaTheme ControlsVersion="1" />
    
    <!-- (default) Use the new Fluent design styles -->
    <sty:FluentAvaloniaTheme ControlsVersion="2" />
    

FluentAvaloniaTheme has several additional options for customizing (these work regardless of which ControlsVersion you use:

You can set the theme mode (light or dark) by setting

    RequestedTheme = "Light" or "Dark"

On Windows you can set the following: 

    UseSegoeUIOnWindows - If true (default), will replace the resource 'ContentControlThemeFontFamily' with SegoeUI, the default Windows font.

    GetUserAccentColor - If true (default), the AccentColor resources are obtained directly from the user's perferences in Windows.

    DefaultToUserTheme - If true (default), will attempt to determine if the user currently has Light or Dark mode enabled and set the app to that theme at startup. This requires Win 10 1809 or greater.
    
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
- NEW (beta) - NavigationView Control (the full thing ported from WinUI)
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
- NumberBox (in testing)

Future goals:
- Finish adapting new WinUI styles as they're rolled out

LightTheme 
![image](https://user-images.githubusercontent.com/40413319/109102163-db175c00-76ed-11eb-8d94-a6fbc1b4e5cc.png)

DarkTheme
![image](https://user-images.githubusercontent.com/40413319/109102203-f7b39400-76ed-11eb-9fc1-35ed6273be86.png)

For more, check out the sample app

Avalonia : https://github.com/AvaloniaUI/Avalonia  
WinUI : https://github.com/microsoft/microsoft-ui-xaml/  
WinUI & Avalonia are available under MIT licences.

## Licence: MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
