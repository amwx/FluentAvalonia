using FluentAvaloniaSamples.Pages;

namespace FluentAvaloniaSamples.ViewModels;

public class InfoBarPageViewModel : ViewModelBase
{
    public InfoBarPageViewModel(InfoBarPage page)
    {
        _owner = page;
    }

    private static string LongMessage => "A long essential app message for your users to be informed of, acknowledge, or take action on." +
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin dapibus dolor vitae justo rutrum, ut lobortis" +
            " nibh mattis. Aenean id elit commodo, semper felis nec.";

    public string Bar2CurrentMessage { get; private set; } = LongMessage;

    public string Bar2DynamicXamlMessage { get; private set; } = "A long essential app message...";

    public int Bar2MessageType
    {
        get => _bar2MessageType;
        set
        {
            if (RaiseAndSetIfChanged(ref _bar2MessageType, value))
            {
                if (value == 0) // Long Message
                {
                    Bar2CurrentMessage = LongMessage;
                    Bar2DynamicXamlMessage = "A long essential app message...";
                }
                else // Short Message
                {
                    Bar2CurrentMessage = Bar2DynamicXamlMessage = "Short Message!";
                }

                RaisePropertyChanged(nameof(Bar2CurrentMessage));
                RaisePropertyChanged(nameof(Bar2DynamicXamlMessage));
            }
        }
    }

    public int Bar2ButtonType
    {
        get => _bar2ButtonType;
        set
        {
            if (RaiseAndSetIfChanged(ref _bar2ButtonType, value))
            {
                _owner.UpdateBar2ActionButton(value);
                RaisePropertyChanged(nameof(Bar2ActionButtonXaml));
            }
        }
    }

    public string Bar2ActionButtonXaml
    {
        get
        {
            if (_bar2ButtonType == 0)
                return null;

            if (_bar2ButtonType == 1)
            {
                return "    <ui:InfoBar.ActionButton>\n" +
                    "        <Button Content=\"Action\" />\n" +
                    "    </ui:InfoBar.ActionButton>";
            }

            if (_bar2ButtonType == 2)
            {
                return "    <ui:InfoBar.ActionButton>\n" +
                    "        <ui:HyperlinkButton Content=\"Informational Link\" NavigateUri=\"https://www.example.com\" />\n" +
                    "    </ui:InfoBar.ActionButton>";
            }

            return null;
        }
    }

    private InfoBarPage _owner;
    private int _bar2MessageType;
    private int _bar2ButtonType;
}
