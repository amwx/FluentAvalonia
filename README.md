# FluentAvalonia

Just a little project to extend Microsoft's Fluent Design system & some WinUI controls into the Avalonia world. This is all a work in progress so bear with me as I get it all sorted out. 

Important notes:
FluentAvalonia has a dependency on the Segoe MDL2 Assets font for symbols (affects FontIcon and CheckBox)    
These are already installed on Windows 10 by default, I'm currently looking into fall back methods for when Segoe MDL2 isn't available, as MS font licences prohibit redistribution. Project still runs, you'll just get the unknown character glyph.  

Currently the project targets Avalonia CI Build 0.9.999-cibuild0007716-beta, and .net core 3.1. In the future, after 0.10, I'll stick to normal release versions =D

What's included so far?
- Styles for
   Button
   RadioButton
   CheckBox
   Base Colors
- Content Dialog
- Basic Flyout for Button
- IconElements
- SplitView

Thing's I'm working on (from WinUI):
- Navigation View
- Calendar View
- Date Picker
- Calendar Date Picker
- Time Picker
- Styles for other base Avalonia Controls where needed
- Color Spectrum/Color picker



Avalonia : https://github.com/AvaloniaUI/Avalonia  
WinUI : https://github.com/microsoft/microsoft-ui-xaml/  
WinUI & Avalonia are available under MIT licences.

## Licence: MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


