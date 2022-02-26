using Avalonia;
using Avalonia.Controls.Primitives;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Represents a RadioButton in a <see cref="TaskDialog"/>
    /// </summary>
    public class TaskDialogRadioButton : TaskDialogCommand
    {
        /// <summary>
        /// Defines the <see cref="IsChecked"/> property
        /// </summary>
        public static readonly DirectProperty<TaskDialogRadioButton, bool?> IsCheckedProperty =
            ToggleButton.IsCheckedProperty.AddOwner<TaskDialogRadioButton>(x => x.IsChecked,
                (x, v) => x.IsChecked = v);

        /// <summary>
        /// Gets or sets whether this RadioButton is checked
        /// </summary>
        public bool? IsChecked
        {
            get => _isChecked;
            set => SetAndRaise(IsCheckedProperty, ref _isChecked, value);
        }

        private bool? _isChecked = false;
    }
}
