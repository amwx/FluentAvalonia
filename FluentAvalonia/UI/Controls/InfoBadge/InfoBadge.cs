using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
    public class InfoBadge : TemplatedControl
    {
        public InfoBadge()
        {
            TemplateSettings = new InfoBadgeTemplateSettings();
            BoundsProperty.Changed.AddClassHandler<InfoBadge>((s, e) => s.OnBoundsChanged(e));
        }

        public static readonly StyledProperty<int> ValueProperty =
            AvaloniaProperty.Register<InfoBadge, int>(nameof(Value), -1);

        public static readonly StyledProperty<IconSource> IconSourceProperty =
            AvaloniaProperty.Register<InfoBadge, IconSource>(nameof(IconSource));

        public static readonly StyledProperty<InfoBadgeTemplateSettings> TemplateSettingsProperty =
            AvaloniaProperty.Register<InfoBadge, InfoBadgeTemplateSettings>(nameof(TemplateSettings));

        public int Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public IconSource IconSource
        {
            get => GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public InfoBadgeTemplateSettings TemplateSettings
        {
            get => GetValue(TemplateSettingsProperty);
            set => SetValue(TemplateSettingsProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            OnDisplayKindPropertiesChanged();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var defaultDesSize = base.MeasureOverride(availableSize);

            if (defaultDesSize.Width < defaultDesSize.Height)
            {
                return new Size(defaultDesSize.Height, defaultDesSize.Height);
            }

            return defaultDesSize;
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            if (change.Property== ValueProperty)
            {
                if (Value < -1)
                    throw new ArgumentOutOfRangeException("Value");
            }

            if (change.Property == ValueProperty || change.Property == IconSourceProperty)
            {
                OnDisplayKindPropertiesChanged();
            }
        }

        private void OnDisplayKindPropertiesChanged()
        {
            var icoSource = IconSource;
            if (Value >= 0)
            {
                PseudoClasses.Set(":value", true);

                PseudoClasses.Set(":fonticon", false);
                PseudoClasses.Set(":icon", false);
                PseudoClasses.Set(":dot", false);
            }
            else if (icoSource != null)
            {
                TemplateSettings.IconElement = IconHelpers.CreateFromUnknown(icoSource);

                PseudoClasses.Set(":fonticon", icoSource is FontIconSource);
                PseudoClasses.Set(":icon", icoSource is not FontIconSource);

                PseudoClasses.Set(":value", false);
                PseudoClasses.Set(":dot", false);
            }
            else
            {
                PseudoClasses.Set(":dot", true);

                PseudoClasses.Set(":value", false);
                PseudoClasses.Set(":fonticon", false);
                PseudoClasses.Set(":icon", false);
            }
        }

        private void OnBoundsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var rc = (Rect)e.NewValue;
            var cornerRadiusValue = rc.Height / 2;
            if (!IsSet(CornerRadiusProperty))
            {
                TemplateSettings.InfoBadgeCornerRadius = new CornerRadius(cornerRadiusValue);
            }
            else
            {
                TemplateSettings.InfoBadgeCornerRadius = new CornerRadius();
            }
        }
    }
}
