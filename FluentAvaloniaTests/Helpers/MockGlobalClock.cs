using System;
using Avalonia.Animation;

namespace FluentAvaloniaTests.Helpers;

public class MockGlobalClock : ClockBase, IGlobalClock
{
    public new void Pulse(TimeSpan systemTime) => base.Pulse(systemTime);
}
