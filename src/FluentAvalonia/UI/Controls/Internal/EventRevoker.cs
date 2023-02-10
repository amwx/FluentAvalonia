using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Utilities;

namespace FluentAvalonia.UI.Controls.Internal;

internal class EffectiveViewportRevoker
{
    public EffectiveViewportRevoker(Control control, EventHandler<EffectiveViewportChangedEventArgs> handler)
    {
        _control = new WeakReference<Control>(control);
        _handler = handler;
        control.EffectiveViewportChanged += handler;
    }

    public void Revoke()
    {
        if (_control.TryGetTarget(out var target))
        {
            target.EffectiveViewportChanged -= _handler;
        }
    }

    private EventHandler<EffectiveViewportChangedEventArgs> _handler;
    private WeakReference<Control> _control;
}
