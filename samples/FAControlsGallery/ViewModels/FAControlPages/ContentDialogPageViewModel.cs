using FluentAvalonia.UI.Controls;
using FAControlsGallery.Pages.SamplePageAssets;

namespace FAControlsGallery.ViewModels;

public class ContentDialogPageViewModel : ViewModelBase
{
    public ContentDialogPageViewModel()
    {
        LaunchDialogCommand = new FACommand(ExecuteLaunchCommand);
    }

    public string Title
    {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    } = "Title Here";

    public string Content
    {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    } = "Dialog message here (doesn't have to be a string, can be anything)";

    public string PrimaryButtonText
    {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    } = "Primary Button";

    public string SecondaryButtonText
    {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    } = "Secondary Button";

    public string CloseButtonText
    {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    } = "Close Button";

    public bool FullSizeDesired
    {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    }

    public FAContentDialogButton[] ContentDialogButtons { get; } = Enum.GetValues<FAContentDialogButton>();

    public FAContentDialogButton ContentDialogDefaultButton
    {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsPrimaryButtonEnabled
    {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    } = true;

    public bool IsSecondaryButtonEnabled
    {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    } = true;

    public FACommand LaunchDialogCommand { get; }

    public async void ExecuteLaunchCommand(object parameter)
    {
        bool hasDeferral = bool.Parse(parameter.ToString());

        var cd = new FAContentDialog
        {
            PrimaryButtonText = PrimaryButtonText,
            SecondaryButtonText = SecondaryButtonText,
            CloseButtonText = CloseButtonText,
            Title = Title,
            Content = Content,
            IsPrimaryButtonEnabled = IsPrimaryButtonEnabled,
            IsSecondaryButtonEnabled = IsSecondaryButtonEnabled,
            DefaultButton = ContentDialogDefaultButton,
            FullSizeDesired = FullSizeDesired
        };

        if (hasDeferral)
        {
            cd.PrimaryButtonClick += OnPrimaryButtonClick;
            await cd.ShowAsync();
            cd.PrimaryButtonClick -= OnPrimaryButtonClick;
        }
        else
        {
            await cd.ShowAsync();
        }
    }

    private async void OnPrimaryButtonClick(FAContentDialog sender, FAContentDialogButtonClickEventArgs args)
    {
        var def = args.GetDeferral();
        await Task.Delay(3000);
        def.Complete();
    }

    public async void ShowInputDialogAsync()
    {
        var dialog = new FAContentDialog()
        {
            Title = "Let's go ...",
            PrimaryButtonText = "Ok :-)",
            SecondaryButtonText = "Not OK :-(",
            CloseButtonText = "Leave me alone!"
        };

        var viewModel = new CustomContentDialogViewModel(dialog);
        dialog.Content = new ContentDialogInputExample()
        {
            DataContext = viewModel
        };

        _ = await dialog.ShowAsync();
    }
}
