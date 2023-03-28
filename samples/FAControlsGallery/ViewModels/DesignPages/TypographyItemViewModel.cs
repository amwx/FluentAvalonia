using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace FAControlsGallery.ViewModels.DesignPages;

public class TypographyItemViewModel
{
    public TypographyItemViewModel(string name, string font, string szLnHgt)
    {
        StyleName = $"{name}TextBlockStyle";
        VariableFont = font;
        SizeLineHeight = szLnHgt;
        CopyCommand = new FACommand(ExecuteCopy);

        TextTheme = App.Current.FindResource(StyleName) as ControlTheme;
    }

    public string VariableFont { get; set; }

    public string SizeLineHeight { get; set; }

    public string StyleName { get; set; }

    public FACommand CopyCommand { get; }

    public ControlTheme TextTheme { get; }

    private async void ExecuteCopy(object param)
    {
        try
        {
            var text = "{StaticResource " + StyleName + "}";
            await Application.Current.Clipboard.SetTextAsync(text);
        }
        catch { }
    }
}
