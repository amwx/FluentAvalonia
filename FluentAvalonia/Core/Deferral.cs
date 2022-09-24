using System;

namespace FluentAvalonia.Core;

public delegate void DeferralCompletedHandler();

public sealed class Deferral
{
    public Deferral(DeferralCompletedHandler completedHandler)
    {
        if (completedHandler is null)
            throw new ArgumentNullException(nameof(completedHandler), "Completion delegate cannot be null");

        _handler = completedHandler;
    }

    public void Complete()
    {
        _handler.Invoke();
    }

    private DeferralCompletedHandler _handler;
}
