using Avalonia;

namespace FluentAvalonia.UI.Controls;

public class TeachingTipTemplateSettings : AvaloniaObject
{
    public Thickness TopRightHighlightMargin { get; internal set; }

    public Thickness TopLeftHighlightMargin { get; internal set; }

    public FAIconElement IconElement { get; internal set; }
}
