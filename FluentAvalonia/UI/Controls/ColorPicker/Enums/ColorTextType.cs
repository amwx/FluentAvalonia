namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Defines values for how the text should display in the Color TextBox of a <see cref="ColorPicker"/>
	/// </summary>
	public enum ColorTextType
    {
		/// <summary>
		/// The color should display in hex format, without alpha: #RRGGBB
		/// </summary>
        Hex,

		/// <summary>
		/// The color should display in hex format, with alpha: #AARRGGBB
		/// </summary>
        HexAlpha,

		/// <summary>
		/// The color should display in CSS rgb format: rgb (R, G, B)
		/// </summary>
        RGB,

		/// <summary>
		/// The color should display in CSS rgba format: rgba (R, G, B, A)
		/// </summary>
        RGBA
    }
}