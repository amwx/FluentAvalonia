using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Represents an icon that uses an IconSource as its content.
    /// </summary>
    public class IconSourceElement : IconElement
    {
        /// <summary>
        /// Defines the <see cref="IconSource"/> property
        /// </summary>
        public static readonly StyledProperty<IconSource> IconSourceProperty =
             AvaloniaProperty.Register<IconSourceElement, IconSource>(nameof(IconSource));

        /// <summary>
        /// Gets or sets the IconSource used as the icon content.
        /// </summary>
        public IconSource IconSource
        {
            get => GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);

            if (change.Property == IconSourceProperty)
			{
                OnIconSourceChanged(change);
                InvalidateMeasure();
			}
		}

		private void OnIconSourceChanged(AvaloniaPropertyChangedEventArgs args)
        {
            var newIcon= (IconSource)args.NewValue;

            if (_child != null)
            {
                ((ISetLogicalParent)_child).SetParent(null);
                LogicalChildren.Clear();
                VisualChildren.Remove(_child);
            }

            if (newIcon != null)
            {
                _child = IconHelpers.CreateFromUnknown(newIcon);
                if (_child != null)
                {
                    ((ISetLogicalParent)_child).SetParent(this);
                    VisualChildren.Add(_child);
                    LogicalChildren.Add(_child);
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return LayoutHelper.MeasureChild(_child, availableSize, new Thickness());
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return LayoutHelper.ArrangeChild(_child, finalSize, new Thickness());
        }

        private Control _child;
    }
}
