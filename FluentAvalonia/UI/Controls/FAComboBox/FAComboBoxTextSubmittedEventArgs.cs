using System;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Provides data when the user enters custom text into the ComboBox.
/// </summary>
public class FAComboBoxTextSubmittedEventArgs : EventArgs
{
    internal FAComboBoxTextSubmittedEventArgs(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Gets or sets whether the TextSubmitted event was handled or not. If **true**,
    /// the framework will not automatically update the selected item of the ComboBox
    /// to the new value.
    /// </summary>
    /// <remarks>
    /// You should handle this event if binding to SelectedItem and entered text is not
    /// translatable to your view model. Otherwise, the selection will be cleared
    /// </remarks>
    public bool Handled { get; set; }

    /// <summary>
    /// Gets the custom text value entered by the user
    /// </summary>
    public string Text { get; }
}
