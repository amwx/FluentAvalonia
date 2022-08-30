namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Defines which components the spectrum display should use
/// </summary>
/// <remarks>
/// The values are defined by the X-Axis, then Y-Axis. The remaining component
/// of the given color space is displayed in the third component slider of the
/// <see cref="FAColorPicker"/>
/// </remarks>
public enum ColorSpectrumComponents
{
    /// <summary>
    /// HSV color space, Saturation is on the X-Axis, Value is on the Y-Axis
    /// </summary>
    SaturationValue = 0,

    /// <summary>
    /// HSV color space, Value is on the X-Axis, Hue is on the Y-Axis
    /// </summary>
    ValueHue = 1,

    /// <summary>
    /// HSV color space, Saturation is on the X-Axis, Hue is on the Y-Axis
    /// </summary>
    SaturationHue = 2,

    /// <summary>
    /// RGB color space, Blue is on the X-Axis, Green is on the Y-Axis
    /// </summary>
    BlueGreen = 3,

    /// <summary>
    /// RGB color space, Blue is on the X-Axis, Red is on the Y-Axis
    /// </summary>
    BlueRed = 4,

    /// <summary>
    /// RGB color space, Green is on the X-Axis, Red is on the Y-Axis
    /// </summary>
    GreenRed = 5
}
