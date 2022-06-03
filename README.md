# FluentAvalonia

Bringing more of Fluent design and WinUI controls into Avalonia.

[![Nuget](https://img.shields.io/nuget/v/FluentAvaloniaUI?color=%236A5ACD&label=FluentAvaloniaUI%20%28nuget%29)](https://www.nuget.org/packages/FluentAvaloniaUI/)
![GitHub repo size](https://img.shields.io/github/repo-size/amwx/FluentAvalonia?color=%234682B4)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/amwx/FluentAvalonia?color=%23483D8B)

Currently Targets: Avalonia 0.10.13 & multitargets netstandard2.0;netstandard2.1;net6.0

Note: Windows 7, 8/8.1 are not supported by FluentAvalonia.

Check out the (newly refreshed) sample app for a demo, with code examples, for more!

![image](https://user-images.githubusercontent.com/40413319/152464696-65a4de6f-1a06-4cca-9f80-c545ad0585ce.png)


For the most part, FluentAvalonia has been made independent of Avalonia and does not require you to include a reference to adding the Fluent theme from Avalonia (more on these below)

To include the styles for FluentAvalonia, add the following to your App.xaml (or .axaml)

````Xml
<!-- in App.axaml -->
<!-- Define xmlns:sty="using:FluentAvalonia.Styling" -->

<App.Styles>
    <sty:FluentAvaloniaTheme />
</App.Styles>
````

Namespace for Controls
````Xml
xmlns:ui="using:FluentAvalonia.UI.Controls"
xmlns:uip="using:FluentAvalonia.UI.Controls.Primitives"
````

By default, FluentAvalonia uses the Fluent v2 styles. All controls in core Avalonia also have been provided a template here to provide a cohesive UX, thus making FluentAvalonia independent (style wise). The ONLY exception is `ContextMenu`, where `ContextFlyout` should be used instead.


<details>
    <summary> <b>FluentAvaloniaTheme has several additional options for customizing:</b> </summary>
    
````C#    
// FluentAvalonia Theme is automatically registered with the AvaloniaLocator on startup. To quickly access it in code,
var faTheme = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();

// Set the Current theme using the Requested Theme Property

// For Light Mode
RequestedTheme = "Light";

// For Dark Mode
RequestedTheme = "Dark";

// For HighContast
RequestedTheme = "HighContrast";

// On Windows the following properties are available, which all default to true:

// Use the System font as the primary font for the App. On Windows 10, this is Segoe UI. On Windows 11, this is Segoe UI Variable Text
// This value is only respected on startup
public bool UseSystemFontOnWindows { get; set; } = true;

// Use the current User System Accent Color as the Accent Color in the app
public bool UseUserAccentColorOnWindows { get; set; } = true;

// Set the app theme to whatever the Windows theme is, respects HighContrast mode
// This value is only respected on startup
public bool UseSystemThemeOnWindows { get; set; } = true;


// Additionally, by default on Windows, all Win32 windows appear in light mode, regardless of the System setting. To force the window to dark mode:

var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
thm.ForceNativeTitleBarToTheme(Window); // Window is the Window object you want to force

// On a regular window, this will force the titlebar and window border into dark mode. NOTE: If you have accent colors enabled on titlebars and window borders
// in system settings, this most likely won't have much of an effect, since the accent color is used instead.

// If you're using my CoreWindow class, this is done automatically.


// The following properties are available on ALL systems:

// To set a custom accent color set this property. The 3 light and 3 dark variants will be generated for you.
// NOTE: I do not verify whether the custom accent color ensures good legibility and accessibility - that is up to you
// If you would like more control over the shades generated, you can directly override the resources in the Application level ResourceDictionary
public Color? CustomAccentColor { get; set; }

CustomAccentColor = Colors.Orange;

// To return to default, set the property to null;
// On Mac/Linux, and Windows with (UseUserAccentColorOnWindows = false), the default color is SlateBlue. Otherwise it returns to the System defined accent.


// NEW in v1.2, if there are controls you don't use, you can use this property to skip loading their template
// This saves a bit of memory, but more importantly reduces the number of styles that need to be evaluated which can add a performance benefit
// This is a semi-colon (;) delimited string of controls. 
public string SkipControls { get; set; }


// For example, to skip the NavigationView and DataGrid controls
SkipControls = "NavigationView;DataGrid";

// The search mechanism just uses a string.Contains() to evaluate each entry. This for controls like NavigationView and CommandBar where
// those terms are in the related controls (NavigationViewItem, CommandBarButton) - this will automatically remove those as well so you
// don't need to specify everything. See the Styles files in (FluentAvalonia/Styling/[BasicControls | Controls]) for naming - though you
// don't need to include "Styles" unless you want specifically only that file

// Have custom resources you want changed when the Theme changes?

// Listen to the RequestedThemeChanged event:
public TypedEventHandler<FluentAvaloniaTheme, RequestedThemeChangedEventArgs> RequestedThemeChanged;

RequestedThemeChanged += OnAppThemeChanged;

private void OnAppThemeChanged(FluentAvaloniaTheme faTheme, RequestedThemeChangedEventArgs args)
{
    // Retreive the new theme from args
    var newTheme = args.NewTheme;

    // your logic
}
````
</details>


# What's included so far?
- Fluent v2 styles for all controls (Light, Dark, and HighContrast themes available)
- FluentAvaloniaTheme - provides easy way to change theme, and accent color at runtime & load all styles/resources
- NavigationView Control (the full thing ported from WinUI)
- Frame control (to assist with Navigation using the NavigationView)
- ContentDialog
- SplitButton, ToggleSplitButton
- DropDownButton
- FontIcon & FontIconSource
- PathIcon & PathIconSource
- BitmapIcon & BitmapIconSource (requires Skia renderer, which is default)
- SymbolIcon & SymbolIconSource (Symbols from new FluentUI Icons)
- ImageIcon & ImageIconSource
- ColorPicker & ColorPickerButton (note: not the WinUI version)
- NumberBox
- InfoBar
- InfoBadge
- MenuFlyout - a revamped version closer to WinUI with binding support
-- MenuFlyoutItem
-- ToggleMenuFlyoutItem
-- MenuFlyoutSubItem
-- RadioMenuFlyoutItem

Future goals:
- TabView (coming soon)
- ListView/GridView (project suspended for now, assessing API and best way to implement)
- CalendarView
- BreadcrumBar
- Other WinUI controls

Avalonia : https://github.com/AvaloniaUI/Avalonia  
WinUI : https://github.com/microsoft/microsoft-ui-xaml/  
WinUI & Avalonia are available under MIT licences.

## Licence: MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
