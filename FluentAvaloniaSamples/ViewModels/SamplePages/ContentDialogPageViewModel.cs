using System;
using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.Pages.SamplePageAssets;
using FluentAvaloniaSamples.Utilities;

namespace FluentAvaloniaSamples.ViewModels;

public class ContentDialogPageViewModel : ViewModelBase
{
    public ContentDialogPageViewModel()
    {
        LaunchDialogCommand = new FACommand(ExecuteLaunchCommand);
    }

    public string Title
    {
        get => _title;
        set => RaiseAndSetIfChanged(ref _title, value);
    }

    public string Content
    {
        get => _content;
        set => RaiseAndSetIfChanged(ref _content, value);
    }

    public string PrimaryButtonText
    {
        get => _primaryText;
        set => RaiseAndSetIfChanged(ref _primaryText, value);
    }

    public string SecondaryButtonText
    {
        get => _secondaryText;
        set => RaiseAndSetIfChanged(ref _secondaryText, value);
    }

    public string CloseButtonText
    {
        get => _closeText;
        set => RaiseAndSetIfChanged(ref _closeText, value);
    }

    public bool FullSizeDesired
    {
        get => _fullSize;
        set => RaiseAndSetIfChanged(ref _fullSize, value);
    }

    public ContentDialogButton[] ContentDialogButtons { get; } = Enum.GetValues<ContentDialogButton>();

    public ContentDialogButton ContentDialogDefaultButton
    {
        get => _defaultButton;
        set => RaiseAndSetIfChanged(ref _defaultButton, value);
    }

    public bool IsPrimaryButtonEnabled
    {
        get => _primaryEnabled;
        set => RaiseAndSetIfChanged(ref _primaryEnabled, value);
    }

    public bool IsSecondaryButtonEnabled
    {
        get => _secondaryEnabled;
        set => RaiseAndSetIfChanged(ref _secondaryEnabled, value);
    }

    public FACommand LaunchDialogCommand { get; }

    public async void ExecuteLaunchCommand(object parameter)
    {
        bool hasDeferral = bool.Parse(parameter.ToString());

        var cd = new ContentDialog
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

    private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var def = args.GetDeferral();
        await Task.Delay(3000);
        def.Complete();
    }

    public async void ShowInputDialogAsync()
    {
        var dialog = new ContentDialog()
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

    private string _title = "Title Here";
    private string _content = "Dialog message here (doesn't have to be a string, can be anything)";
    private string _primaryText = "Primary Button";
    private string _secondaryText = "Secondary Button";
    private string _closeText = "Close Button";
    private bool _fullSize;
    private ContentDialogButton _defaultButton;
    private bool _primaryEnabled = true;
    private bool _secondaryEnabled = true;
}
