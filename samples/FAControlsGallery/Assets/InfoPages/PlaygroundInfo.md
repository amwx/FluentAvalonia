Use this edit box to type in some markup and load it to see how it looks and 
behaves.

### Usage Notes
- For obvious reasons, DataContext Bindings don't work
- Resource lookups are based in the context of this container so you have the full FA theme available
- Use the theme switch button above to dynamically swap the theme and see how it looks in both light and dark mode!

### How it works
When you click load, your code is automatically loaded into a `UserControl` and loaded into the container. You can then interact and adjust your code to see how it looks, behaves, and performs. 

The UserControl contains the following xmlns definitions:
```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:ui="using:FluentAvalonia.UI.Controls"
             xmlns:uip="using:FluentAvalonia.UI.Controls.Primitives"
             xmlns:data="using:FluentAvalonia.UI.Data"
             xmlns:input="using:FluentAvalonia.UI.Input"
             xmlns:media="using:FluentAvalonia.UI.Media"
             xmlns:wnd="using:FluentAvalonia.UI.Windowing"
             Foreground="{DynamicResource TextFillColorPrimaryBrush}">
```
