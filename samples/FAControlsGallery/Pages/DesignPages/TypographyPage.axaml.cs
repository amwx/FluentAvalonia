using Avalonia.Controls;
using FAControlsGallery.ViewModels.DesignPages;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages.DesignPages;

public partial class TypographyPage : ControlsPageBase
{
    public TypographyPage()
    {
        InitializeComponent();

        ShowToggleThemeButton = false;
        ControlName = "Typography";
        PreviewImage = (IconSource)App.Current.FindResource("TextPageIcon");

        Description = "Type helps provide structure and hierarchy to the UI. The default font is Segoe UI " +
            "on Windows 10 and Segoe UI Variable on Windows 11. Best practice is to use Regular weight for " +
            "most text and use Semibold for titles. The minimum values should be 12px Regular and 14px Semibold. " +
            "This page was adapted from the WinUI 3 Gallery.";

        CrossPlatformNotice.Message = "On non-Windows systems, Segoe UI/Segoe UI Variable fonts are not available " +
            "(licensing) and no open alternative is available. FluentAvalonia uses the system default font in these " +
            "cases, see below for how to adjust some styles to cope with this.";

        TypeRampListBox.ItemsSource = new List<TypographyItemViewModel>
        {
            new TypographyItemViewModel("Caption",     "Small, Regular",    "12/16 epx"),
            new TypographyItemViewModel("Body",        "Text, Regular",     "14/20 epx"),
            new TypographyItemViewModel("BodyStrong",  "Text, SemiBold",   "14/20 epx"),
            new TypographyItemViewModel("Subtitle",    "Display, SemiBold", "20/28 epx"),
            new TypographyItemViewModel("Title",       "Display, SemiBold", "28/36 epx"),
            new TypographyItemViewModel("TitleLarge",  "Display, SemiBold", "40/52 epx"),
            new TypographyItemViewModel("Display",     "Display, SemiBold", "68/92 epx"),
        };
    }
}
