# FluentAvalonia

Just a little project to extend some additional WinUI and Fluent design in to the Avalonia world.

NOTE: A major update is currently in progress, part of which will include an official nuget package & release.

Targets: Avalonia 0.10 & .net core 5.0

What's included so far?
- New WinUI styles (currently being rolled out) for some controls
-- NOTE: These are still being updated in WinUI and not all controls have been updated yet (here and in WinUI)
- Support for new FluentUI Icons
- NEW (beta) - NavigationView Control (the full thing ported from WinUI)
- SplitButton, ToggleSplitButton
- DropDownButton
- Editable ComboBox (experimental)
- ContentDialog
- FontIcon & FontIconSource
- PathIcon & PathIconSource
- BitmapIcon & BitmapIconSource (requires Skia renderer, which is default)
- SymbolIcon & SymbolIconSource (Symbols from new FluentUI Icons)
- ColorPicker & ColorPickerButton (in testing)
- Frame control for navigation (in testing)
- NumberBox (in testing)
- Theme Manager with support for runtime theme changing, getting Windows system accent color & dark mode preferences (including the Native titlebar on supported Win10 systems, 1809+)

Future goals:
- Finish adapting new WinUI styles as they're rolled out
- Port other WinUI controls

LightTheme 
![image](https://user-images.githubusercontent.com/40413319/109102163-db175c00-76ed-11eb-8d94-a6fbc1b4e5cc.png)

DarkTheme
![image](https://user-images.githubusercontent.com/40413319/109102203-f7b39400-76ed-11eb-9fc1-35ed6273be86.png)



Avalonia : https://github.com/AvaloniaUI/Avalonia  
WinUI : https://github.com/microsoft/microsoft-ui-xaml/  
WinUI & Avalonia are available under MIT licences.

## Licence: MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
