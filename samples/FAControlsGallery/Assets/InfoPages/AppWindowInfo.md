### What is FAAppWindow
FAAppWindow (hereafter, AppWindow) attempts to keep a modern Windows Window experience in your app. On Mac/Linux, AppWindow does not intervene and the window behaves like a normal Avalonia Window.

### How does it work
On Windows, AppWindow forces an extended window theme with custom caption bar while preserving the system drop-shadow, border, and DWM effects. No changes are applied on Mac/Linux.

#### Note
Be sure to read the docs on Avalonia's Window Drawn Decorations APIs so you know how ElementRoles and extended windows work.

### v3 Change
In v1/v2 of FluentAvalonia, AppWindow handled everything separate from Avaloina (including a custom WndProc) for extending the client into the decorations area. Starting with FAv3 (Avaloina 12.0), AppWindow now 
relies on the WindowDrawnDecorations added to Avalonia for the 12.0 release. **if you experience issues with AppWindow, please verify they are FA specific before opening an issue. If it occurs with a normal Avalonia extended window, the bug report must be filed upstream with Avalonia**.

Note that with this change, on Windows, you are not able to change `ExtendClientAreaToDecorationsHint` and `ExtendClientAreaTitleBarHeightHint` has no effect.

### The TitleBar
Instead of the system caption bar, FA draws a custom TitleBar for FAAppWindow on Windows. This is a basic left-aligned Icon (if present) and title text. If you wish to customzie the titlebar or add extra items, please see the examples below. The default titlebar also reserves the top space of the window for the titlebar and does not extend your content into it. Again, follow the examples below to achieve this.
