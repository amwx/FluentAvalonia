using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace FAControlsGallery.Controls;

public class PageHeaderControl : TemplatedControl
{
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

        if (TitleTextImage != null)
        {
            var img = e.NameScope.Get<Image>("TitleTextImageHost");
            if (TitleTextImage.IsAbsoluteUri && TitleTextImage.IsFile)
            {
                img.Source = new Bitmap(TitleTextImage.LocalPath);
            }
            else
            {
                var al = AvaloniaLocator.Current.GetService<IAssetLoader>();
                using var s = al.Open(TitleTextImage);
                img.Source = new Bitmap(s);
            }
        }        
    }

    private Uri _titleTextImage;
}
