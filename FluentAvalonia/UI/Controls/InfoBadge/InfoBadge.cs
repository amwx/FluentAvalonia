using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Represents a control for indicating notifications, alerts, new content, 
    /// or to attract focus to an area within an app.
    /// </summary>
    public partial class InfoBadge : TemplatedControl
    {
        public InfoBadge()
        {
            TemplateSettings = new InfoBadgeTemplateSettings();
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

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property== ValueProperty)
            {
                if (Value < -1)
                    throw new ArgumentOutOfRangeException(nameof(Value));
            }
            else if (change.Property == ValueProperty || change.Property == IconSourceProperty)
            {
                OnDisplayKindPropertiesChanged();
            }
            else if (change.Property == BoundsProperty)
			{
                OnBoundsChanged(change);
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
