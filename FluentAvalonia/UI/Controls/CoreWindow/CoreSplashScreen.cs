using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using FluentAvalonia.Core.ApplicationModel;

namespace FluentAvalonia.UI.Controls
{
    public class CoreSplashScreen : TemplatedControl
    {
        public IApplicationSplashScreen SplashScreen { get; set; }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (SplashScreen != null)
            {
                // User set content has priority
                if (SplashScreen.SplashScreenContent != null)
                {
                    var cp = e.NameScope.Find<ContentPresenter>("ContentHost");
                    cp.Content = SplashScreen.SplashScreenContent;
                }
                else if (SplashScreen.AppIcon != null) // Followed by the icon
                {
                    var img = e.NameScope.Find<Image>("AppImageHost");
                    img.Source = SplashScreen.AppIcon;
                }
                else if (!string.IsNullOrEmpty(SplashScreen.AppName)) // Followed by just the app name
                {
                    var tb = e.NameScope.Find<TextBlock>("AppNameText");
                    tb.Text = SplashScreen.AppName;
                }
            }            
        }
    }
}
