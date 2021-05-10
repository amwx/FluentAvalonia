using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls.Primitives;

namespace FluentAvalonia.UI.Controls
{
    public class Flyout : FlyoutBase
    {
        public Flyout()
        {
            
        }

        static Flyout()
        {
            //ContentProperty.Changed.AddClassHandler<Flyout>((s, v) => s.OnContentChanged(v));
        }

        #region AvaloniaProperties

        public static readonly StyledProperty<Control> ContentProperty =
           AvaloniaProperty.Register<Flyout, Control>("Content");

        #endregion

        #region CLR Properties

        [Content]
        public Control Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public Styles FlyoutPresenterStyle => _styles ??= new Styles();

        #endregion

        protected override Control CreatePresenter()
        {
            return new FlyoutPresenter
            {
                [!ContentControl.ContentProperty] = this[!Flyout.ContentProperty]
            };
        }
        
        protected override void OnOpening()
        {
            //Pass the FlyoutPresenterStyle into the Popup
            //Since that's internal, we can control the Styles on it
            Popup.Child?.Styles.Clear();
            Popup.Child?.Styles.Add(FlyoutPresenterStyle);
            base.OnOpening();
        }

        protected override void OnOpened()
        {
            base.OnOpened();
        }

        private Styles _styles;
    }
}
