using Avalonia;
using System;
using Avalonia.Input;
using System.Reactive.Linq;


namespace FluentAvalonia.UI.Controls
{
    public class ComboBoxItem : Avalonia.Controls.ListBoxItem
    {
        public ComboBoxItem()
        {
            //this.GetObservable(ComboBoxItem.IsFocusedProperty).Where(focused => focused)
            //    .Subscribe(x => (Parent as ComboBox)?.OnItemFocused(this,x));

            this.GotFocus += ComboBoxItem_GotFocus;
        }

        private void ComboBoxItem_GotFocus(object sender, GotFocusEventArgs args)
        {
            (Parent as ComboBox)?.OnItemFocused(this, args);
        }

        static ComboBoxItem()
        {
            FocusableProperty.OverrideDefaultValue<ComboBoxItem>(true);
        }
    }
}
