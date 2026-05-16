# Contributing to FluentAvalonia

## Before contributing:
Please open an issue before submitting a PR to ensure the feature or bug fix you want to provide will be accepted and to ensure I don't already have a fix in the works. PRs created without a corresponding issue and approval that do not fit the exception below may be closed without explanation.

NOTE: If the fix you want to supply is a small change (like typo or other single-line issues), then you may skip the issue. If you are unsure, just open the issue.

## Code Style
Note FA does not follow all the most recent guidelines from .net. Some yes, but not all. I'm stuck in my ways, sorry in advance.

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
  - Fields can also go in this file (especially if any `DirectProperty`s)
- Use of `var` or Type of left-hand side of assignment is up to you. But, do not use var in a traditional for-loop
- Do NOT enable nullable reference types.
  
Ensure maximum readility and understanding by not hiding details behind the compiler. Operate under the assumption that tooltips and intellisense aren't available:
- Do NOT use `new()` syntax (e.g., `x = new MyClass()`). Always include the type on the right-hand side of assignment expressions.
- Do not use primary constructors for non `record` items. (I don't even like it there, but it is what it is)
- Do not use collection expressions. Please make sure an explicit collection type is on the right-hand side of assignment expressions.
  - Instead use Collection initializers, which are far superior and ensure maximum readibility
- Avoid using the `field` keyword. Just add the field, it's not that hard.
  - I won't reject new additions with this, but I will reject changes to existing files for this.

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
