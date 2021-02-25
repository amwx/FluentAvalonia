using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class PickerFlyoutPresenter : ContentControl
    {
        public PickerFlyoutPresenter(PickerFlyout pf)
        {
            _owner = pf;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _cancelButton = e.NameScope.Find<Button>("CancelButton");
            _confirmButton = e.NameScope.Find<Button>("ConfirmButton");

            if (_cancelButton != null)
            {
                _cancelButton.Click += OnCancelClick;
            }

            if (_confirmButton != null)
            {
                _confirmButton.Click += OnConfirmClick;
            }
        }

        private void OnConfirmClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _owner.OnConfirmed();
        }

        private void OnCancelClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _owner.OnCancel();
        }

        private Button _cancelButton;
        private Button _confirmButton;
        private PickerFlyout _owner;
    }
}
