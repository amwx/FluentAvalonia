using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls.Primitives;

namespace FluentAvalonia.UI.Controls
{
    public class PickerFlyout : FlyoutBase
    {

        public static readonly StyledProperty<IControl> ContentProperty =
            AvaloniaProperty.Register<PickerFlyout, IControl>("Content");

        [Content]
        public IControl Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public virtual void OnConfirmed() 
        {
            Hide();
            Confirmed?.Invoke(this, null);
        }

        public virtual void OnCancel()
        {
            Hide();
            Cancelled?.Invoke(this, null);
        }

        protected override Control CreatePresenter()
        {
            return new PickerFlyoutPresenter(this) { Content = Content };
        }

        public event TypedEventHandler<PickerFlyout, object> Confirmed;
        public event TypedEventHandler<PickerFlyout, object> Cancelled;
    }
}
