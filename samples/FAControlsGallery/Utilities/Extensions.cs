using System.Diagnostics;

namespace FAControlsGallery.Utilities;

internal static class Extensions
{
    public static void FireAndForget(this Task t)
    {
        _ = t.ContinueWith(x =>
        {
            if (x.IsFaulted)
            {
                Debug.WriteLine("Exception occurred and caught by FireAndForget");
            }
        }, TaskScheduler.Default);
    }
}
