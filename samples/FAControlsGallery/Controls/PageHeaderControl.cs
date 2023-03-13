using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace FAControlsGallery.Controls;

public class PageHeaderControl : TemplatedControl
{
    public PageHeaderControl()
    {
        SizeChanged += OnSizeChanged;
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
        _text2 = e.NameScope.Get<Image>("VersionTextHost");
        if (TitleTextImage != null)
        {           
            if (TitleTextImage.IsAbsoluteUri && TitleTextImage.IsFile)
            {
                _text1.Source = new Bitmap(TitleTextImage.LocalPath);
            }
            else
            {
                var al = AvaloniaLocator.Current.GetService<IAssetLoader>();
                using var s = al.Open(TitleTextImage);
                _text1.Source = new Bitmap(s);
            }
        }        
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var wid = e.NewSize.Width;
        if (wid < 630)
        {
            var delta = 630 - wid;

            _text1.Width = 300 - delta;
            _text2.Width = 90 - (delta * 0.5);
        }
        else
        {
            _text1.Width = _text2.Width = double.NaN;
        }

        PseudoClasses.Set(":small", wid < 450);
    }

    private Uri _titleTextImage;
    private Image _text1;
    private Image _text2;
}
