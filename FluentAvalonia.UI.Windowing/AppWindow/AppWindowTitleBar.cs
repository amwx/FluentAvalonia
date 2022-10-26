using Avalonia;
using Avalonia.Media;
using Avalonia.Utilities;

namespace FluentAvalonia.UI.Windowing;

public class AppWindowTitleBar
{
    internal AppWindowTitleBar(AppWindow parent)
    {
        _parent = parent;
    }

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

    public double LeftInset => _leftInset;

    public double RightInset => _rightInset;

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
                return true;
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
