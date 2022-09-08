using System;
using FluentAvalonia.UI.Controls;

namespace FluentAvaloniaSamples.ViewModels;

public class CustomContentDialogViewModel : ViewModelBase
{
    private readonly ContentDialog dialog;

    public CustomContentDialogViewModel(ContentDialog dialog)
    {
        if (dialog is null)
        {
            throw new ArgumentNullException(nameof(dialog));
        }

        this.dialog = dialog;
        dialog.Closed += DialogOnClosed;
    }

    private void DialogOnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        dialog.Closed -= DialogOnClosed;

        var resultHint = new ContentDialog()
        {
            Content = $"You chose \"{args.Result}\"",
            Title = "Result",
            PrimaryButtonText = "Thanks"
        };

        _ = resultHint.ShowAsync();
    }

    private string _UserInput;

    /// <summary>
    /// Gets or sets the user input to check 
    /// </summary>
    public string UserInput
    {
        get => _UserInput;
        set
        {
            if (RaiseAndSetIfChanged(ref _UserInput, value))
            {
                HandleUserInput();
            }
        }
    }

    private void HandleUserInput()
    {
        switch (UserInput.ToLowerInvariant())
        {
            case "accept":
            case "ok":
                dialog.Hide(ContentDialogResult.Primary);
                break;

            case "dismiss":
            case "not ok":
                dialog.Hide(ContentDialogResult.Secondary);
                break;

            case "cancel":
            case "close":
            case "hide":
                dialog.Hide();
                break;
        }
    }

    private static readonly string[] _AvailableKeyWords = new[]
    {
        "Accept",
        "OK",
        "Dismiss",
        "Not OK",
        "Close",
        "Cancel",
        "Hide"
    };

    public string[] AvailableKeyWords => _AvailableKeyWords;
}
