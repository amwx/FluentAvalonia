//This file is a part of FluentAvalonia
//AvaloniaUI - Licenced under MIT Licence, https://github.com/AvaloniaUI/Avalonia
//Adapted from the WinUI project, MIT Licence, https://github.com/microsoft/microsoft-ui-xaml

using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;
using FluentAvalonia.UI.Controls.Primitives;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Just an ordinary Flyout, content can be anything
    /// Basically just a simple wrapper around a Popup
    /// Not in WinUI, so impl is my own
    /// </summary>
    public class Flyout : FlyoutBase
    {
        public Flyout()
        {

        }

        static Flyout()
        {
            ContentProperty.Changed.AddClassHandler<Flyout>((s, v) => s.OnContentChanged(v));
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

        #endregion

        #region Methods

        private void OnContentChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (_presenter == null)
                _presenter = CreatePresenter() as FlyoutPresenter;
            _presenter.Content = e.NewValue;

            FlyoutRoot.Child = _presenter;
        }

        protected override Control CreatePresenter()
        {
            return new FlyoutPresenter();
        }

        #endregion

        private FlyoutPresenter _presenter;
    }
}
