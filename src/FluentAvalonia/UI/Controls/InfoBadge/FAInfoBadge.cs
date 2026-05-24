using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

// InfoBadge is up to date with WinUI as of 5/9/26

/// <summary>
/// Represents a control for indicating notifications, alerts, new content, 
/// or to attract focus to an area within an app.
/// </summary>
public partial class FAInfoBadge : TemplatedControl
{
    public FAInfoBadge()
    {
        TemplateSettings = new FAInfoBadgeTemplateSettings();
        SizeChanged += HandleSizeChanged;
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

        if (change.Property == ValueProperty)
        {
            if (Value < -1)
                throw new ArgumentOutOfRangeException(nameof(Value));
        }
        else if (change.Property == ValueProperty || change.Property == IconSourceProperty)
        {
            OnDisplayKindPropertiesChanged();
        }
    }

    private void OnDisplayKindPropertiesChanged()
    {
        var icoSource = IconSource;
        if (Value >= 0)
        {
            PseudoClasses.Set(s_pcValue, true);

            PseudoClasses.Set(s_pcFontIcon, false);
            PseudoClasses.Set(FASharedPseudoclasses.s_pcIcon, false);
            PseudoClasses.Set(s_pcDot, false);
        }
        else if (icoSource != null)
        {
            TemplateSettings.IconElement = FAIconHelpers.CreateFromUnknown(icoSource);

            PseudoClasses.Set(s_pcFontIcon, icoSource is FAFontIconSource);
            PseudoClasses.Set(FASharedPseudoclasses.s_pcIcon, icoSource is not FAFontIconSource);

            PseudoClasses.Set(s_pcValue, false);
            PseudoClasses.Set(s_pcDot, false);
        }
        else
        {
            PseudoClasses.Set(s_pcDot, true);

            PseudoClasses.Set(s_pcValue, false);
            PseudoClasses.Set(s_pcFontIcon, false);
            PseudoClasses.Set(FASharedPseudoclasses.s_pcIcon, false);
        }
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs args)
    {
        var cornerRadiusValue = args.NewSize.Height * 0.5;
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
