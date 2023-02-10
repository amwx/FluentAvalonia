using System;
using Avalonia.Threading;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data for the closing event.
/// </summary>
public class ContentDialogClosingEventArgs : EventArgs
{
    internal ContentDialogClosingEventArgs(ContentDialogResult res)
    {
        Result = res;
    }

    /// <summary>
    /// Gets or sets a value that can cancel the closing of the dialog.
    /// A true value for Cancel cancels the default behavior.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets the <see cref="ContentDialogResult"/> of the closing event.
    /// </summary>
    public ContentDialogResult Result { get; }

    /// <summary>
    /// Gets a <see cref="Deferral"/> that the app can use to 
    /// respond asynchronously to the closing event.
    /// </summary>
    public Deferral GetDeferral()
    {
        _deferralCount++;

        return new Deferral(() =>
        {
            Dispatcher.UIThread.VerifyAccess();
            DecrementDeferralCount();
        });
    }

    internal void SetDeferral(Deferral deferral)
    {
        _deferral = deferral;
    }

    internal void IncrementDeferralCount()
    {
        _deferralCount++;
    }

    internal void DecrementDeferralCount()
    {
        _deferralCount--;
        if (_deferralCount == 0)
        {
            _deferral.Complete();
        }
    }

    private Deferral _deferral;
    private int _deferralCount;
}
