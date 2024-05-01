using System.Diagnostics;
using Avalonia.Threading;

namespace FluentAvalonia.UI.Controls;

internal static class BuildTreeScheduler
{
    public static void RegisterWork(int priority, Action workFunc)
    {
        if (priority < 0)
            throw new ArgumentOutOfRangeException(nameof(priority), "Priority must be >= 0");
        if (workFunc == null)
            throw new ArgumentNullException(nameof(workFunc));

        QueueTick();
        _pendingWork.Add(new WorkInfo(priority, workFunc));
    }

    public static bool ShouldYield() =>
        _timer.DurationInMilliseconds() > _budgetInMs;

    public static void OnRendering()
    {
        bool budgetReached = ShouldYield();
        if (!budgetReached && _pendingWork.Count > 0)
        {
            // Sort in descending order of priority and work from the end of the list to avoid moving around during erase.
            _pendingWork.Sort((x, y) => x.Priority > y.Priority ? 1 : -1);

            int currentIndex = _pendingWork.Count - 1;
            do
            {
                _pendingWork[currentIndex].InvokeWorkFunc();
                _pendingWork.RemoveAt(currentIndex);
            }
            while (--currentIndex >= 0 && !ShouldYield());
        }

        if (_pendingWork.Count == 0)
        {
            // No more pending work, unhook from rendering event since being hooked up will cause wux to try to 
            // call the event at 60 frames per second
            _renderingToken = false;
            //CompositionTarget.Rendering -= OnRendering;
            // RepeaterTestHooks.NotifyBuildTreeCompleted();
        }

        // Reset the timer so it snaps the time just before rendering
        _timer.Reset();

        // NO CompositionTarget.Rendering event, we need to manually trigger another update as long as we have
        // _renderingToken == true
        if (_renderingToken)
            Dispatcher.UIThread.Post(OnRendering, DispatcherPriority.Render);
    }

    public static void QueueTick()
    {
        if (!_renderingToken)
        {
            _renderingToken = true;
            Dispatcher.UIThread.Post(OnRendering, DispatcherPriority.Render);
        }
    }

    private static double _budgetInMs = 40;
    private static readonly object _lockObj = new object();

    [ThreadStatic]
    private static QPCTimer _timer = new QPCTimer();
    [ThreadStatic]
    private static readonly List<WorkInfo> _pendingWork = new List<WorkInfo>();

    private static bool _renderingToken;
}

struct WorkInfo
{
    public WorkInfo(int priority, Action workFunc)
    {
        _priority = priority;
        _workFunc = workFunc;
    }

    public int Priority => _priority;

    public void InvokeWorkFunc() => _workFunc.Invoke();

    private int _priority;
    private Action _workFunc;
}
