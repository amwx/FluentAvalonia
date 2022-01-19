using Avalonia;
using Avalonia.Controls.Primitives;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Represents a control for indicating notifications, alerts, new content, 
    /// or to attract focus to an area within an app.
    /// </summary>
    public partial class InfoBadge : TemplatedControl
    {
        /// <summary>
        /// Defines the <see cref="Value"/> property
        /// </summary>
        public static readonly StyledProperty<int> ValueProperty =
            AvaloniaProperty.Register<InfoBadge, int>(nameof(Value), -1);

        /// <summary>
        /// Defines the <see cref="IconSource"/> property
        /// </summary>
        public static readonly StyledProperty<IconSource> IconSourceProperty =
            AvaloniaProperty.Register<InfoBadge, IconSource>(nameof(IconSource));

        /// <summary>
        /// Defines the <see cref="TemplateSettings"/> property
        /// </summary>
        public static readonly StyledProperty<InfoBadgeTemplateSettings> TemplateSettingsProperty =
            AvaloniaProperty.Register<InfoBadge, InfoBadgeTemplateSettings>(nameof(TemplateSettings));

        /// <summary>
        /// Gets or sets the integer to be displayed in a numeric InfoBadge.
        /// </summary>
        public int Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Gets or sets the icon to be used in an InfoBadge.
        /// </summary>
        public IconSource IconSource
        {
            get => GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        /// <summary>
        /// Provides calculated values that can be referenced as TemplatedParent sources when defining 
        /// templates for an InfoBadge. Not intended for general use.
        /// </summary>
        public InfoBadgeTemplateSettings TemplateSettings
        {
            get => GetValue(TemplateSettingsProperty);
            internal set => SetValue(TemplateSettingsProperty, value);
        }
    }
}
