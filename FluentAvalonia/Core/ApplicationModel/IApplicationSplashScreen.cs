using System.Threading.Tasks;
using Avalonia.Media;

namespace FluentAvalonia.Core.ApplicationModel
{
    public interface IApplicationSplashScreen
    {
        string AppName { get; }

        IImage AppIcon { get; }

        object SplashScreenContent { get; }

        void RunTasks();

        int MinimumShowTime { get; }
    }
}
