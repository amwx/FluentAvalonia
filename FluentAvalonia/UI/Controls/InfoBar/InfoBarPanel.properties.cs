using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls.Primitives
{
	/// <summary>
	/// Represents a panel that arranges its items horizontally if there is available space, otherwise vertically.
	/// </summary>
	/// <remarks>
	/// This control is specific to the <see cref="InfoBar"/> and generally should not be used elsewhere
	/// </remarks>
	public partial class InfoBarPanel : Panel
	{
		/// <summary>
		/// Defines the <see cref="HorizontalOrientationPadding"/> property
		/// </summary>
		public static readonly StyledProperty<Thickness> HorizontalOrientationPaddingProperty =
			AvaloniaProperty.Register<InfoBarPanel, Thickness>(nameof(HorizontalOrientationPadding));

		/// <summary>
		/// Defines the <see cref="VerticalOrientationPadding"/> property
		/// </summary>
		public static readonly StyledProperty<Thickness> VerticalOrientationPaddingProperty =
			AvaloniaProperty.Register<InfoBarPanel, Thickness>(nameof(VerticalOrientationPadding));

		/// <summary>
		/// Gets and sets the distance between the edges of the InfoBarPanel and its children when the 
		/// panel is oriented horizontally.
		/// </summary>
		public Thickness HorizontalOrientationPadding
		{
			get => GetValue(HorizontalOrientationPaddingProperty);
			set => SetValue(HorizontalOrientationPaddingProperty, value);
		}

		/// <summary>
		/// Gets and sets the distance between the edges of the InfoBarPanel and its children when the
		/// panel is oriented vertically.
		/// </summary>
		public Thickness VerticalOrientationPadding
		{
			get => GetValue(VerticalOrientationPaddingProperty);
			set => SetValue(VerticalOrientationPaddingProperty, value);
		}

		/// <summary>
		/// Defines the HorizontalOrientationMargin attached property
		/// </summary>
		public static readonly AttachedProperty<Thickness> HorizontalOrientationMarginProperty =
			AvaloniaProperty.RegisterAttached<InfoBarPanel, IControl, Thickness>("HorizontalOrientationMargin");

		/// <summary>
		/// Defines the VerticalOrientationMargin attached property
		/// </summary>
		public static readonly AttachedProperty<Thickness> VerticalOrientationMarginProperty =
			AvaloniaProperty.RegisterAttached<InfoBarPanel, IControl, Thickness>("VerticalOrientationMargin");

		/// <summary>
		/// Sets the HorizontalOrientationMargin to an object.
		/// </summary>
		/// <param name="c">The IControl to set the property on</param>
		/// <param name="t">The desired Thickness</param>
		public static void SetHorizontalOrientationMargin(IControl c, Thickness t)
		{
			c.SetValue(HorizontalOrientationMarginProperty, t);
		}

		/// <summary>
		/// Gets the HorizontalOrientationMargin from an object.
		/// </summary>
		/// <param name="c">The IControl to retreive the value from</param>
		/// <returns>The HorizontalOrientationMargin thickness</returns>
		public static Thickness GetHorizontalOrientationMargin(IControl c)
		{
			return c.GetValue<Thickness>(HorizontalOrientationMarginProperty);
		}

		/// <summary>
		/// Sets the VerticalOrientationMargin to an object.
		/// </summary>
		/// <param name="c">The IControl to set the property on</param>
		/// <param name="t">The desired Thickness</param>
		public static void SetVerticalOrientationMargin(IControl c, Thickness t)
		{
			c.SetValue(VerticalOrientationMarginProperty, t);
		}

		/// <summary>
		/// Gets the VerticalOrientationMargin from an object.
		/// </summary>
		/// <param name="c">The IControl to retreive the value from</param>
		/// <returns>The VerticalOrientationMargin thickness</returns>
		public static Thickness GetVerticalOrientationMargin(IControl c)
		{
			return c.GetValue<Thickness>(VerticalOrientationMarginProperty);
		}
	}
}
