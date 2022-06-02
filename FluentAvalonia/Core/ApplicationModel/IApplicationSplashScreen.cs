using Avalonia.Media;

namespace FluentAvalonia.Core.ApplicationModel
{
    /// <summary>
    /// Defines a user specified UWP-like SplashScreen for CoreWindow
    /// </summary>
    public interface IApplicationSplashScreen
    {
        /// <summary>
        /// Specifies the name of the Application to display during the SplashScreen
        /// </summary>
        string AppName { get; }

        /// <summary>
        /// Specifies the desired image to be shown during the SplashScreen
        /// </summary>
        IImage AppIcon { get; }

        /// <summary>
        /// Specifies custom content to be shown during the SplashScreen
        /// </summary>
        object SplashScreenContent { get; }

        /// <summary>
        /// Called by CoreWindow to run necessary background tasks during the splashscreen
        /// </summary>
        /// <remarks>
        /// This method is called in a background thread (i.e., you don't need to include your own Task). 
        /// Remember that UI thread related tasks must be posted to the dispatcher from this method
        /// </remarks>
        void RunTasks();

        /// <summary>
        /// Specifies the minimum show time (in milliseconds) for the SplashScreen.
        /// </summary>
        /// <remarks>
        /// For quick background loading jobs, you may get undesirable visual effects from the window opening,
        /// and immediately switching from Splash to main content. If the background tasks (i.e., RunTasks()) 
        /// finishes before this time, the background thread will hold until the desired time elapses, before
        /// returning to let CoreWindow finish opening.
        /// </remarks>
        int MinimumShowTime { get; }
    }
}
