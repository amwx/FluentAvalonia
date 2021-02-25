using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class CommandBarToggleButton : CommandBarButton
    {
        public static readonly StyledProperty<bool> IsCheckedProperty =
            AvaloniaProperty.Register<CommandBarToggleButton, bool>("IsChecked");

        public bool IsChecked
        {
            get => GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsCheckedProperty)
            {
                UpdateVisualStateForCheck();
            }
            else if (change.Property == FlyoutProperty)
            {
                throw new InvalidOperationException("Cannot attach a flyout to type CommandBarToggleButton");
            }
        }

        protected override void OnClick()
        {
            IsChecked = !IsChecked;
            base.OnClick();
        }

        private void UpdateVisualStateForCheck()
        {
            PseudoClasses.Set(":checked", IsChecked);
        }
    }
}
