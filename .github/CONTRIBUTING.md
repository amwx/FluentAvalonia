# Contributing to FluentAvalonia

## Before contributing:
Please open an issue before submitting a PR to ensure the feature or bug fix you want to provide will be accepted and to ensure I don't already have a fix in the works. 

NOTE: If the fix you want to supply is a small change (like typo or other single-line issues), then you may skip the issue.

## Code Style
Modern C# coding style is generally used here, though I have a few exceptions

- Order of C# files:
  - Constructors (instance then static)
  - AvaloniaProperties
  - CLR properties
  - RoutedEvents
  - CLR events
  - Override methods
  - Public/internal methods
  - Private methods
  - Fields, constants, and static members
- If a C# file has more than 3-4 AvaloniaProperties, all AvaloniaProperties, CLR properties, RoutedEvents, and Events should be moved into a separate partial class file with a `.prop.cs` or `.properties.cs` extension.
- Do NOT use `new()` syntax. Always include the type, e.g., `x = new MyClass()`
- Use of `var` or Type of left-hand side of assignment is up to you. But, do not use var in a traditional for-loop
- Do NOT enable nullable reference types.

For Xaml files:
- When naming controls in a template, do NOT use the `PART_` prefix. This is only necessary in the controls inherited from Avalonia where this is used. FluentAvalonia controls should not follow this old WPF convention.
- Identation should be 4 spaces
- When adding properties to a Control, the first property should be on the line with the item, then all remaining properties should be on a separate line aligned to the first property. 
```xaml
<TextBlock Foreground="White"
           Text="My text here"
           Width="50" />
```
If the properties and values are small (like Width and Height), they may be combined on one line, if desired.
```xaml
<TextBlock Foreground="White"
           Text="My text here"
           Width="50" Height="50" />
```
- When adding nested styles inside of ControlThemes:
  - If the nested style is pseudo-class based, there should be no space between the nesting operator `^` and the class name:
```xaml
<Style Selector="^:pointerover" />
```
  - If the nesting operator is not immediately followed by a pseudoclass, there should be a space following the `^`:
```xaml
<Style Selector="^ /template/ ContentPresenter" />
```
