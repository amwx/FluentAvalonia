using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace FluentAvalonia.UI.Controls
{
    public class IconSourceElement : IconElement
    {
        static IconSourceElement()
        {
            ForegroundProperty.Changed.AddClassHandler<IconSourceElement>((x, _) => x.OnForegroundChanged());
            IconSourceProperty.Changed.AddClassHandler<IconSourceElement>((x,v) => x.OnIconSourceChanged(v));
            AffectsMeasure<IconSourceElement>(IconSourceProperty);
        }
               

        public static readonly StyledProperty<IconSource> IconSourceProperty =
             AvaloniaProperty.Register<IconSourceElement, IconSource>(nameof(IconSource));

        public IconSource IconSource
        {
            get => GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
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

        private void OnForegroundChanged()
        {
            if (IconSource != null)
            {
                IconSource.Foreground = Foreground;
            }
        }

        private Control _child;
    }
}
