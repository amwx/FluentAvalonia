//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml


namespace FluentAvalonia.UI.Media
{
    /// <summary>
    /// Use this interface to specify controls which should be the parent container
    /// for the reveal style to apply. Most typically this is set on Windows. But
    /// you could set it on a ListBox, for example, and keep the reveal style contained
    /// with in the listbox
    /// Note, this does NOT apply animations when clicked. Just provides a border around 
    /// controls with reveal active set. 
    /// To propertly implement, implement the interface, be sure to declare AvaloniaProperties
    /// for for any properties, and place a RevealBorder in the template of the control you want
    /// reveal to show on. 
    /// The RevealBorder will walk up the logical tree until it finds something with ISupportReveal
    /// and connect to the RevealLocation & IsRevealActiveProperties
    /// </summary>
    public interface ISupportReveal
    {
        public bool IsRevealActive { get; set; }
    }
}
