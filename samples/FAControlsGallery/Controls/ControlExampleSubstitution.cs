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

    public object Value
    {
        get;
        set
        {
            SetAndRaise(ValueProperty, ref field, value);
            ValueChanged?.Invoke(this, null);
        }
    } = null;

    public bool IsEnabled
    {
        get;
        set
        {
            SetAndRaise(IsEnabledProperty, ref field, value);
            ValueChanged?.Invoke(this, null);
        }
    } = true;

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
