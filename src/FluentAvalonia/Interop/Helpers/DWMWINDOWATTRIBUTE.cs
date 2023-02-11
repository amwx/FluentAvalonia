namespace FluentAvalonia.Interop;

internal enum DWMWINDOWATTRIBUTE
{
    /// <summary>
    /// Use with DwmGetWindowAttribute. Discovers whether non-client rendering is enabled. 
    /// The retrieved value is of type BOOL. TRUE if non-client rendering is enabled; otherwise, FALSE.
    /// </summary>
    DWMWA_NCRENDERING_ENABLED = 1,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Sets the non-client rendering policy. 
    /// The pvAttribute parameter points to a value from the DWMNCRENDERINGPOLICY enumeration.
    /// </summary>
    DWMWA_NCRENDERING_POLICY,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Enables or forcibly disables DWM transitions. T
    /// he pvAttribute parameter points to a value of type BOOL. TRUE to disable transitions, 
    /// or FALSE to enable transitions.
    /// </summary>
    DWMWA_TRANSITIONS_FORCEDISABLED,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Enables content rendered in the non-client area to be 
    /// visible on the frame drawn by DWM. The pvAttribute parameter points to a value of type BOOL. 
    /// TRUE to enable content rendered in the non-client area to be visible on the frame; otherwise, FALSE.
    /// </summary>
    DWMWA_ALLOW_NCPAINT,

    /// <summary>
    /// Use with DwmGetWindowAttribute. Retrieves the bounds of the caption button area in the window-relative space.
    /// The retrieved value is of type RECT. If the window is minimized or otherwise not visible to the user, 
    /// then the value of the RECT retrieved is undefined. You should check whether the retrieved RECT contains a 
    /// boundary that you can work with, and if it doesn't then you can conclude that the window 
    /// is minimized or otherwise not visible.
    /// </summary>
    DWMWA_CAPTION_BUTTON_BOUNDS,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Specifies whether non-client content is right-to-left (RTL) mirrored.
    /// The pvAttribute parameter points to a value of type BOOL. TRUE if the non-client content is right-to-left (RTL) 
    /// mirrored; otherwise, FALSE.
    /// </summary>
    DWMWA_NONCLIENT_RTL_LAYOUT,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Forces the window to display an iconic thumbnail or peek representation 
    /// (a static bitmap), even if a live or snapshot representation of the window is available. This value is normally 
    /// set during a window's creation, and not changed throughout the window's lifetime. Some scenarios, however,
    /// might require the value to change over time. The pvAttribute parameter points to a value of type BOOL. 
    /// TRUE to require a iconic thumbnail or peek representation; otherwise, FALSE.
    /// </summary>
    DWMWA_FORCE_ICONIC_REPRESENTATION,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Sets how Flip3D treats the window. The pvAttribute parameter points to a 
    /// value from the DWMFLIP3DWINDOWPOLICY enumeration.
    /// </summary>
    DWMWA_FLIP3D_POLICY,

    /// <summary>
    /// Use with DwmGetWindowAttribute. Retrieves the extended frame bounds rectangle in screen space.
    /// The retrieved value is of type RECT.
    /// </summary>
    DWMWA_EXTENDED_FRAME_BOUNDS,

    /// <summary>
    /// Use with DwmSetWindowAttribute. The window will provide a bitmap for use by DWM as an iconic thumbnail or 
    /// peek representation (a static bitmap) for the window. DWMWA_HAS_ICONIC_BITMAP can be specified with 
    /// DWMWA_FORCE_ICONIC_REPRESENTATION. DWMWA_HAS_ICONIC_BITMAP normally is set during a window's creation 
    /// and not changed throughout the window's lifetime. Some scenarios, however, might require the value to 
    /// change over time. The pvAttribute parameter points to a value of type BOOL. 
    /// TRUE to inform DWM that the window will provide an iconic thumbnail or peek representation; otherwise, FALSE.
    /// </summary>
    DWMWA_HAS_ICONIC_BITMAP,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Do not show peek preview for the window. The peek view shows a full-sized
    /// preview of the window when the mouse hovers over the window's thumbnail in the taskbar. If this attribute is set,
    /// hovering the mouse pointer over the window's thumbnail dismisses peek (in case another window in the group has
    /// a peek preview showing). The pvAttribute parameter points to a value of type BOOL. 
    /// TRUE to prevent peek functionality, or FALSE to allow it.
    /// </summary>
    DWMWA_DISALLOW_PEEK,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Prevents a window from fading to a glass sheet when peek is invoked. 
    /// The pvAttribute parameter points to a value of type BOOL. TRUE to prevent the window from fading during another 
    /// window's peek, or FALSE for normal behavior.
    /// </summary>
    DWMWA_EXCLUDED_FROM_PEEK,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Cloaks the window such that it is not visible to the user. The window is still 
    /// composed by DWM. Using with DirectComposition: Use the DWMWA_CLOAK flag to cloak the layered child window when 
    /// animating a representation of the window's content via a DirectComposition visual that has been associated with 
    /// the layered child window. For more details on this usage case, see How to animate the bitmap of a layered child window.
    /// </summary>
    DWMWA_CLOAK,

    /// <summary>
    /// Use with DwmGetWindowAttribute. If the window is cloaked, provides one of the following values explaining why.
    /// DWM_CLOAKED_APP (value 0x0000001). The window was cloaked by its owner application.
    /// DWM_CLOAKED_SHELL (value 0x0000002). The window was cloaked by the Shell.
    /// DWM_CLOAKED_INHERITED (value 0x0000004). The cloak value was inherited from its owner window.
    /// </summary>
    DWMWA_CLOAKED,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Freeze the window's thumbnail image with its current visuals. Do no further 
    /// live updates on the thumbnail image to match the window's contents.
    /// </summary>
    DWMWA_FREEZE_REPRESENTATION,

    /// <summary>
    /// 
    /// </summary>
    DWMWA_PASSIVE_UPDATE_MODE,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Enables a non-UWP window to use host backdrop brushes. 
    /// If this flag is set, then a Win32 app that calls Windows::UI::Composition APIs can build transparency 
    /// effects using the host backdrop brush (see Compositor.CreateHostBackdropBrush). 
    /// The pvAttribute parameter points to a value of type BOOL. TRUE to enable host backdrop brushes 
    /// for the window, or FALSE to disable it.
    /// Value is supported with Windows 11 Build 22000
    /// </summary>
    DWMWA_USE_HOSTBACKDROPBRUSH,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Allows the window frame for this window to be drawn in dark mode colors
    /// when the dark mode system setting is enabled. For compatibility reasons, all windows default to light mode
    /// regardless of the system setting. The pvAttribute parameter points to a value of type BOOL.
    /// TRUE to honor dark mode for the window, FALSE to always use light mode.
    /// Windows 11 Build 22000
    /// </summary>
    DWMWA_USE_IMMERSIVE_DARK_MODE = 20,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Specifies the rounded corner preference for a window. The pvAttribute parameter 
    /// points to a value of type DWM_WINDOW_CORNER_PREFERENCE.
    /// Windows 11 Build 22000
    /// </summary>
    DWMWA_WINDOW_CORNERPREFERENCE = 33,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Specifies the color of the window border. 
    /// The pvAttribute parameter points to a value of type COLORREF. 
    /// The app is responsible for changing the border color according to state changes, 
    /// such as a change in window activation. 
    /// Windows 11 Build 22000
    /// </summary>
    DWMWA_BORDER_COLOR,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Specifies the color of the caption. 
    /// The pvAttribute parameter points to a value of type COLORREF.
    /// Windows 11 22000
    /// </summary>
    DWMWA_CAPTION_COLOR,

    /// <summary>
    /// Use with DwmSetWindowAttribute. Specifies the color of the caption text. 
    /// The pvAttribute parameter points to a value of type COLORREF. 
    /// Windows 11 22000
    /// </summary>
    DWMWA_TEXT_COLOR,

    /// <summary>
    /// Use with DwmGetWindowAttribute. Retrieves the width of the outer border that the DWM 
    /// would draw around this window. The value can vary depending on the DPI of the window. 
    /// The pvAttribute parameter points to a value of type UINT.
    /// Windows 11 22000
    /// </summary>
    DWMWA_VISIBLE_FRAME_BORDER_THICKNESS,

    /// <summary>
    /// Use with DwmGetWindowAttribute or DwmSetWindowAttribute. Retrieves or specifies the system-drawn 
    /// backdrop material of a window, including behind the non-client area. The pvAttribute parameter points 
    /// to a value of type DWM_SYSTEMBACKDROP_TYPE. 
    /// Windows 11 22621
    /// </summary>
    DWMWA_SYSTEMBACKDROP_TYPE,


    DWMWA_LAST
}
