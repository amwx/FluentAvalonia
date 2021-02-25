using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class ColorPickerButton : TemplatedControl, IStyleable
    {
        public static readonly StyledProperty<Color> ColorProperty =
            AvaloniaProperty.Register<ColorPickerButton, Color>("Color", defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        Type IStyleable.StyleKey => typeof(ColorPickerButton);

        public Color Color
        {
            get => GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public event TypedEventHandler<ColorPickerButton, ColorChangedEventArgs> ColorChanged;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            if (_flyout != null)
            {
                _flyout.Opened -= OnFlyoutOpened;
                _flyout.Confirmed -= OnFlyoutConfirmed;
                _flyout.Cancelled -= OnFlyoutCancelled;
            }

            base.OnApplyTemplate(e);

            var b = e.NameScope.Find<Button>("MainButton");
            _flyout = b.Flyout as PickerFlyout;

            if (_flyout != null)
            {
                _flyout.Opened += OnFlyoutOpened;
                _flyout.Confirmed += OnFlyoutConfirmed;
                _flyout.Cancelled += OnFlyoutCancelled;
            }

            _picker = e.NameScope.Get<ColorPicker>("ColorPicker");            
        }

        private void OnFlyoutOpened(Primitives.FlyoutBase sender, object args)
        {
            //Binding doesn't work between Color2 & Color, so we have to manually manage this
            //They are implicitly convertible tho, but binding doesn't like that
            //one Plus of this is we can't change the color of the dialog accidentally if we 
            //don't confirm the selection
            _picker.Color = Color;
        }

        private void OnFlyoutCancelled(PickerFlyout sender, object args)
        {
            OnCancelled();
        }

        private void OnFlyoutConfirmed(PickerFlyout sender, object args)
        {
            var old = Color;
            OnConfirmedCore(old, _picker.Color);
        }

        private void OnConfirmedCore(Color2 oldColor, Color2 newColor)
        {
            Color = newColor;
            var ea = new ColorChangedEventArgs(oldColor, newColor);
            OnConfirmed(ea);
            ColorChanged?.Invoke(this, ea);
        }

        protected virtual void OnCancelled() { }

        protected virtual void OnConfirmed(ColorChangedEventArgs args) { }

        private ColorPicker _picker;
        private PickerFlyout _flyout;
    }
}
