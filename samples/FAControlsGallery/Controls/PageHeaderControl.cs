using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;

namespace FAControlsGallery.Controls;

public enum PageHeaderTextType
{
    Main,
    CoreControls,
    FAControls,
}

public class PageHeaderControl : TemplatedControl
{
    public PageHeaderControl()
    {
        SizeChanged += OnSizeChanged;
        ActualThemeVariantChanged += OnActualThemeVariantChanged;
    }

    public static readonly DirectProperty<PageHeaderControl, PageHeaderTextType> TextTypeProperty =
        AvaloniaProperty.RegisterDirect<PageHeaderControl, PageHeaderTextType>(nameof(TextType),
            x => x.TextType, (x, v) => x.TextType = v);

    public PageHeaderTextType TextType
    {
        get;
        set => SetAndRaise(TextTypeProperty, ref field, value);
    }

    public static readonly DirectProperty<PageHeaderControl, Uri> TitleTextImageProperty =
        AvaloniaProperty.RegisterDirect<PageHeaderControl, Uri>(nameof(TitleTextImage),
            x => x.TitleTextImage, (x, v) => x.TitleTextImage = v);

    public Uri TitleTextImage
    {
        get;
        set => SetAndRaise(TitleTextImageProperty, ref field, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _text1 = e.NameScope.Get<Image>("TitleTextImageHost");
        UpdateTitleText();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var wid = e.NewSize.Width;
        if (wid < 630)
        {
            var delta = 630 - wid;

            _text1.Width = 400 - delta;
        }
        else
        {
            _text1.Width = double.NaN;
        }

        PseudoClasses.Set(":small", wid < 450);
    }

    private void UpdateTitleText()
    {
        if (_text1 == null)
            return;

        var theme = ActualThemeVariant;

        const string asset = "avares://FAControlsGallery/Assets/Images/";

        var header = TextType switch
        {
            PageHeaderTextType.CoreControls => "FAHeader_CoreControls",
            PageHeaderTextType.FAControls => "FAHeader_NewControls",
            _ => "FAHeader2"
        };

        if (theme == ThemeVariant.Light)
        {
            header += "_Dark";
        }

        header += ".png";

        using var s = AssetLoader.Open(new Uri($"{asset}{header}"));
        _text1.Source = new Bitmap(s);
    }

    private void OnActualThemeVariantChanged(object sender, EventArgs e)
    {
        UpdateTitleText();
    }

    private Image _text1;
}
