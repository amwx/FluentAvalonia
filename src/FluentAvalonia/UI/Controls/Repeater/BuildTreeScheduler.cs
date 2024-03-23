using Avalonia.Threading;

namespace FluentAvalonia.UI.Controls;

public static class BuildTreeScheduler
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

    public static void OnRendering(object sender, EventArgs args)
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

            if (_pendingWork.Count == 0)
            {
                // No more pending work, unhook from rendering event since being hooked up will case wux to try to 
                // call the event at 60 frames per second
                _renderingToken = false;
                //CompositionTarget.Rendering -= OnRendering;
                // RepeaterTestHooks.NotifyBuildTreeCompleted();
            }
        }

        // Reset the timer so it snaps the time just before rendering
        _timer.Reset();
    }

    public static void QueueTick()
    {
        if (!_renderingToken)
        {
            _renderingToken = true;
            Dispatcher.UIThread.Post(() => OnRendering(null, null), DispatcherPriority.Render);
        }
    }

    private static double _budgetInMs = 40;

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
