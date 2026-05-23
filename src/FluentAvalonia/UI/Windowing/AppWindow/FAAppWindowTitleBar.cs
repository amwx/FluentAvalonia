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
        get => _backgroundColor;
        set
        {
            if (_backgroundColor != value)
            {
                _backgroundColor = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the title bar when the window is active
    /// </summary>
    public Color? ForegroundColor
    {
        get => _foregroundColor;
        set
        {
            if (_foregroundColor != value)
            {
                _foregroundColor = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the background color of the title bar when the window is inactive
    /// </summary>
    public Color? InactiveBackgroundColor
    {
        get => _inactiveBackgroundColor;
        set
        {
            if (_inactiveBackgroundColor != value)
            {
                _inactiveBackgroundColor = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the title bar when the window is inactive
    /// </summary>
    public Color? InactiveForegroundColor
    {
        get => _inactiveForegroundColor; 
        set
        {
            if (_inactiveForegroundColor != value)
            {
                _inactiveForegroundColor = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the background color of the caption buttons when the window is active
    /// </summary>
    public Color? ButtonBackgroundColor
    {
        get => _buttonBackgroundColor; 
        set
        {
            if (_buttonBackgroundColor != value)
            {
                _buttonBackgroundColor = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the caption buttons when the window is active
    /// </summary>
    public Color? ButtonForegroundColor
    {
        get => _buttonForegroundColor; 
        set
        {
            if (_buttonForegroundColor != value)
            {
                _buttonForegroundColor = value;
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
        get => _buttonHoverBackgroundColor;
        set
        {
            if (_buttonHoverBackgroundColor != value)
            {
                _buttonHoverBackgroundColor = value;
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
        get => _buttonHoverForegroundColor; 
        set
        {
            if (_buttonHoverForegroundColor != value)
            {
                _buttonHoverForegroundColor = value;
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
        get => _buttonPressedBackgroundColor;
        set
        {
            if (_buttonPressedBackgroundColor != value)
            {
                _buttonPressedBackgroundColor = value;
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
        get => _buttonPressedForegroundColor; 
        set
        {
            if (_buttonPressedForegroundColor != value)
            {
                _buttonPressedForegroundColor = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the background color of the caption buttons when the window is inactive
    /// </summary>
    public Color? ButtonInactiveBackgroundColor
    {
        get => _buttonInactiveBackgroundColor; 
        set
        {
            if (_buttonInactiveBackgroundColor != value)
            {
                _buttonInactiveBackgroundColor = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the caption buttons when the window is inactive
    /// </summary>
    public Color? ButtonInactiveForegroundColor
    {
        get => _buttonInactiveForegroundColor; 
        set
        {
            if (_buttonInactiveForegroundColor != value)
            {
                _buttonInactiveForegroundColor = value;
                _parent.TitleBarColorsChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the window content should display in the title bar area of the window
    /// </summary>
    public bool ExtendsContentIntoTitleBar
    {
        get => _extendsContentIntoTitleBar;
        set
        {
            if (_extendsContentIntoTitleBar != value)
            {
                _extendsContentIntoTitleBar = value;
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
        get => _height;
        set
        {
            if (!FAMathHelpers.IsClose(_height, value))
            {
                _height = value;
                _parent.OnTitleBarHeightChanged(value);
            }
        }
    }

    /// <summary>
    /// Sets whether the full screen button is visible in the title bar.
    /// </summary>
    public bool ShowFullScreenButton
    {
        get => _showFullScreenButton;
        set
        {
            if (_showFullScreenButton != value)
            {
                _showFullScreenButton = value;
                _parent.OnShowFullScreenButtonChanged(value);
            }
        }
    }

    private FAAppWindow _parent;
    private Color? _backgroundColor;
    private Color? _buttonBackgroundColor;
    private Color? _buttonForegroundColor;
    private Color? _buttonHoverBackgroundColor;
    private Color? _buttonHoverForegroundColor;
    private Color? _buttonInactiveBackgroundColor;
    private Color? _buttonInactiveForegroundColor;
    private Color? _buttonPressedBackgroundColor;
    private Color? _buttonPressedForegroundColor;
    private bool _extendsContentIntoTitleBar;
    private Color? _foregroundColor;
    private double _height = 32;
    private Color? _inactiveBackgroundColor;
    private Color? _inactiveForegroundColor;
    private bool _showFullScreenButton;
}
