using Avalonia.Media;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Windowing;

/// <summary>
/// Represents the title bar of an <see cref="FAAppWindow"/> allowing customization such as
/// colors, hit testing, and allowing app content in the title bar area
/// </summary>
public class FAAppWindowTitleBar
{
    internal FAAppWindowTitleBar(FAAppWindow parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Gets or sets the background color of the title bar when the window is active
    /// </summary>
    public Color? BackgroundColor
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the title bar when the window is active
    /// </summary>
    public Color? ForegroundColor
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the background color of the title bar when the window is inactive
    /// </summary>
    public Color? InactiveBackgroundColor
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the title bar when the window is inactive
    /// </summary>
    public Color? InactiveForegroundColor
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the background color of the caption buttons when the window is active
    /// </summary>
    public Color? ButtonBackgroundColor
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the caption buttons when the window is active
    /// </summary>
    public Color? ButtonForegroundColor
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the background color of the caption buttons when the window is active
    /// and the pointer is over the minimize or maximize button
    /// </summary>
    public Color? ButtonHoverBackgroundColor
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the caption buttons when the window is active
    /// and the pointer is over the minimize or maximize button
    /// </summary>
    public Color? ButtonHoverForegroundColor
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the background color of the caption buttons when the window is active
    /// and the pointer is pressed on the minimize or maximize button
    /// </summary>
    public Color? ButtonPressedBackgroundColor
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the caption buttons when the window is active
    /// and the pointer is pressed on the minimize or maximize button
    /// </summary>
    public Color? ButtonPressedForegroundColor
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the background color of the caption buttons when the window is inactive
    /// </summary>
    public Color? ButtonInactiveBackgroundColor
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the caption buttons when the window is inactive
    /// </summary>
    public Color? ButtonInactiveForegroundColor
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the window content should display in the title bar area of the window
    /// </summary>
    public bool ExtendsContentIntoTitleBar
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _parent.OnExtendsContentIntoTitleBarChanged(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the height of the default title bar
    /// </summary>
    /// <remarks>
    /// If <see cref="ExtendsContentIntoTitleBar" /> is true, this value describes the height of the
    /// default drag rect and caption buttons only. If custom drag rects are set, only the caption
    /// buttons are affected by this
    /// </remarks>
    public double Height
    {
        get;
        set
        {
            if (!FAMathHelpers.IsClose(field, value))
            {
                field = value;
                _parent.OnTitleBarHeightChanged(value);
            }
        }
    } = 32;

    /// <summary>
    /// Sets whether the full screen button is visible in the title bar.
    /// </summary>
    public bool ShowFullScreenButton
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _parent.OnShowFullScreenButtonChanged(value);
            }
        }
    }

    private FAAppWindow _parent;
}
