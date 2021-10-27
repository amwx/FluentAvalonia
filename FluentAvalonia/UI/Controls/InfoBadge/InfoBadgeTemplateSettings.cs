using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
    public class InfoBadgeTemplateSettings : AvaloniaObject
    {
        public static readonly StyledProperty<CornerRadius> InfoBadgeCornerRadiusProperty =
            AvaloniaProperty.Register<InfoBadgeTemplateSettings, CornerRadius>(nameof(InfoBadgeCornerRadius));

        public static readonly StyledProperty<IconElement> IconElementProperty =
            AvaloniaProperty.Register<InfoBadgeTemplateSettings, IconElement>(nameof(IconElement));


        public CornerRadius InfoBadgeCornerRadius
        {
            get => GetValue(InfoBadgeCornerRadiusProperty);
            internal set => SetValue(InfoBadgeCornerRadiusProperty, value);
        }

        public IconElement IconElement
        {
            get => GetValue(IconElementProperty);
            internal set => SetValue(IconElementProperty, value);
        }
    }
}
