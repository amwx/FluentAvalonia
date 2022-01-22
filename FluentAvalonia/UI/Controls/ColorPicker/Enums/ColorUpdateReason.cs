namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Defines the source of the change in color of a <see cref="ColorPicker"/>
	/// </summary>
	public enum ColorUpdateReason
    {
		/// <summary>
		/// The initial setting of the Color property, forces all input areas to have their
		/// color set
		/// </summary>
        Initial,

		/// <summary>
		/// The color was updated by setting the Color property programmatically. Forces all
		/// input ares to have their colors set
		/// </summary>
        Programmatic,

		/// <summary>
		/// The color change was caused by interaction with the ColorSpectrum
		/// </summary>
        Spectrum,

		/// <summary>
		/// The color change was caused by interaction with the third component slider of the 
		/// ColorSpectrum
		/// </summary>
        ThirdComponent,

		/// <summary>
		/// The color change was caused by interaction with the Hue Slider
		/// </summary>
        HueSlider,

		/// <summary>
		/// The color change was caused by interaction with the Hue TextBox
		/// </summary>
        HueBox,

		/// <summary>
		/// The color change was caused by interaction with the Saturation Slider
		/// </summary>
        SaturationSlider,

		/// <summary>
		/// The color change was caused by interaction with the Saturation TextBox
		/// </summary>
        SaturationBox,

		/// <summary>
		/// The color change was caused by interaction with the Value Slider
		/// </summary>
        ValueSlider,

		/// <summary>
		/// The color change was caused by interaction with the Value TextBox
		/// </summary>
        ValueBox,

		/// <summary>
		/// The Color change was caused by interaction with the Red Slider
		/// </summary>
        RedRamp,

		/// <summary>
		/// The Color change was caused by interaction with the Red TextBox
		/// </summary>
        RedBox,

		/// <summary>
		/// The Color change was caused by interaction with the Green Slider
		/// </summary>
        GreenRamp,

		/// <summary>
		/// The Color change was caused by interaction with the Green TextBox
		/// </summary>
        GreenBox,

		/// <summary>
		/// The Color change was caused by interaction with the Blue Slider
		/// </summary>
        BlueRamp,

		/// <summary>
		/// The Color change was caused by interaction with the Blue TextBox
		/// </summary>
        BlueBox,

		/// <summary>
		/// The Color change was caused by interaction with the alpha slider of the spectrum display
		/// </summary>
        SpectrumAlphaRamp,

		/// <summary>
		/// The Color change was caused by interaction with the alpha ramp in the text entry area
		/// </summary>
		AlphaRamp,//Part of text entry area

		/// <summary>
		/// The Color change was caused by interaction with the Alpha TextBox
		/// </summary>
        AlphaBox,
		
		/// <summary>
		/// The Color change was caused by interaction with the Hex/RGB input TextBox
		/// </summary>
        HexBox
    }
}