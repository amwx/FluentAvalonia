using Avalonia;
using Avalonia.Media;
using Avalonia.Utilities;

namespace FluentAvalonia.UI.Windowing;

/// <summary>
/// Values describing the complexity the title bar hit test logic should use
/// </summary>
public enum TitleBarHitTestType
{
    /// <summary>
    /// Hit testing is done with a simple bounds check
    /// </summary>
    Simple,

    /// <summary>
    /// Hit testing is done with bounds checks first, then
    /// uses the current Renderer to hit test visuals to check
    /// for interactive content
    /// </summary>
    /// <remarks>
    /// Use this if you're using something like a TabView or NavigationView
    /// where managing the drag rectangles manually may be too cumbersome
    /// </remarks>
    Complex
}


public class AppWindowTitleBar
{
    internal AppWindowTitleBar(AppWindow parent)
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
    /// Gets or sets how the hit test logic should handle hit testing controls that are placed
    /// in the title bar drag rect region
    /// </summary>
    public TitleBarHitTestType TitleBarHitTestType { get; set; }

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
            if (!MathUtilities.AreClose(_height, value))
            {
                _height = value;
                _parent.OnTitleBarHeightChanged(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the system reserved region for caption buttons
    /// </summary>
    /// <remarks>
    /// This value is always zero in LTR mode. In RTL mode, this value is constant unless
    /// render scaling changes on the window
    /// </remarks>
    public double LeftInset => _leftInset;

    /// <summary>
    /// Gets or sets the system reserved region for caption buttons
    /// </summary>
    /// <remarks>
    /// This value is always zero in RTL mode. In LTR mode, this value is constant unless
    /// render scaling changes on the window
    /// </remarks>
    public double RightInset => _rightInset;

    /// <summary>
    /// Sets custom specified drag regions for the window. Setting this value overrides the 
    /// default drag rect. Pass null to restore to the default.
    /// </summary>
    public void SetDragRectangles(Rect[] value)
    {
        _dragRects = value;
    }

    internal bool? HitTestDragRects(Point p)
    {
        if (_dragRects == null)
            return null;

        for (int i = 0; i < _dragRects.Length; i++)
        {
            if (_dragRects[i].Contains(p))
            {
                if (TitleBarHitTestType == TitleBarHitTestType.Simple)
                    return true;

                return _parent.ComplexHitTest(p);
            }
        }

        return false;
    }

    internal void SetInset(double inset, FlowDirection dir)
    {
        if (dir == FlowDirection.LeftToRight)
        {
            _leftInset = inset;
            _rightInset = 0;
        }
        else
        {
            _rightInset = inset;
            _leftInset = 0;
        }
    }

    private AppWindow _parent;
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
    private Rect[] _dragRects;
    private double _leftInset;
    private double _rightInset;
}
