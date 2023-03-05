using Avalonia;
using Avalonia.Media;
using FluentAvalonia.Core;

namespace FAControlsGallery.Controls;

// From WinUI XamlControlsGallery
public class ControlExampleSubstitution : AvaloniaObject
{
    public static readonly DirectProperty<ControlExampleSubstitution, bool> IsEnabledProperty =
        AvaloniaProperty.RegisterDirect<ControlExampleSubstitution, bool>(nameof(IsEnabled),
             x => x.IsEnabled, (x, v) => x.IsEnabled = v);

    public static readonly DirectProperty<ControlExampleSubstitution, object> ValueProperty =
        AvaloniaProperty.RegisterDirect<ControlExampleSubstitution, object>(nameof(Value),
            x => x.Value, (x, v) => x.Value = v);

    public string Key { get; set; }

    private object _value = null;
    public object Value
    {
        get => _value;
        set
        {
            SetAndRaise(ValueProperty, ref _value, value);
            ValueChanged?.Invoke(this, null);
        }
    }

    private bool _enabled = true;
    public bool IsEnabled
    {
        get => _enabled;
        set
        {
            SetAndRaise(IsEnabledProperty, ref _enabled, value);
            ValueChanged?.Invoke(this, null);
        }
    }

    public event TypedEventHandler<ControlExampleSubstitution, object> ValueChanged;

    public string ValueAsString()
    {
        if (!IsEnabled)
        {
            return string.Empty;
        }

        object value = Value;

        // For solid color brushes, use the underlying color.
        if (value is SolidColorBrush)
        {
            value = ((SolidColorBrush)value).Color;
        }

        if (value == null)
        {
            return string.Empty;
        }

        return value.ToString();
    }
}
