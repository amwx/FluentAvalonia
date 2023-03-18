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
    }

    public static readonly DirectProperty<PageHeaderControl, PageHeaderTextType> TextTypeProperty =
        AvaloniaProperty.RegisterDirect<PageHeaderControl, PageHeaderTextType>(nameof(TextType),
            x => x.TextType, (x, v) => x.TextType = v);

    public PageHeaderTextType TextType
    {
        get => _textType;
        set => SetAndRaise(TextTypeProperty, ref _textType, value);
    }

    public static readonly DirectProperty<PageHeaderControl, Uri> TitleTextImageProperty =
        AvaloniaProperty.RegisterDirect<PageHeaderControl, Uri>(nameof(TitleTextImage),
            x => x.TitleTextImage, (x, v) => x.TitleTextImage = v);

    public Uri TitleTextImage
    {
        get => _titleTextImage;
        set => SetAndRaise(TitleTextImageProperty, ref _titleTextImage, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _text1 = e.NameScope.Get<Image>("TitleTextImageHost");
        UpdateTitleText();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ActualThemeVariantProperty)
        {
            UpdateTitleText();
        }
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

        var al = AvaloniaLocator.Current.GetService<IAssetLoader>();
        using var s = al.Open(new Uri($"{asset}{header}"));
        _text1.Source = new Bitmap(s);
    }

    private Uri _titleTextImage;
    private Image _text1;
    private PageHeaderTextType _textType;
}
