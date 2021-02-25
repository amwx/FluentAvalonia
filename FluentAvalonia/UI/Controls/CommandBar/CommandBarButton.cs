using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace FluentAvalonia.UI.Controls
{
    // To keep things simpler, we don't derive from Button, so we must recreate the Button
    // functionality to get expected behavior

    //Submenu support not implemented yet...

    public class CommandBarButton : CommandBarItemBase, ICommandSource
    {
        static CommandBarButton()
        {
            FocusableProperty.OverrideDefaultValue<CommandBarButton>(true);
        }

        public static readonly DirectProperty<CommandBarButton, ICommand> CommandProperty =
            AvaloniaProperty.RegisterDirect<CommandBarButton, ICommand>(nameof(Command),
                button => button.Command, (button, command) => button.Command = command, enableDataValidation: true);

        public static readonly StyledProperty<object> CommandParameterProperty =
            AvaloniaProperty.Register<CommandBarButton, object>(nameof(CommandParameter));

        public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
            RoutedEvent.Register<CommandBarButton, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

        public static readonly StyledProperty<KeyGesture> KeyboardAcceleratorProperty =
            HotKeyManager.HotKeyProperty.AddOwner<CommandBarButton>();

        public static readonly StyledProperty<bool> IsPressedProperty =
            AvaloniaProperty.Register<CommandBarButton, bool>(nameof(IsPressed));

        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<CommandBarButton, string>("Label");

        public static readonly StyledProperty<IconElement> IconProperty =
            AvaloniaProperty.Register<CommandBarButton, IconElement>("Icon");

        public static readonly StyledProperty<CommandBarLabelPosition> LabelPositionProperty =
            AvaloniaProperty.Register<CommandBarButton, CommandBarLabelPosition>("LabelPosition");

        public static readonly DirectProperty<CommandBarButton, FlyoutBase> FlyoutProperty =
            AvaloniaProperty.RegisterDirect<CommandBarButton, FlyoutBase>("Flyout",
            (s) => s.Flyout, (s, v) => s.Flyout = v);

        public ICommand Command
        {
            get { return _command; }
            set { SetAndRaise(CommandProperty, ref _command, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public KeyGesture KeyboardAccelerator
        {
            get { return GetValue(KeyboardAcceleratorProperty); }
            set { SetValue(KeyboardAcceleratorProperty, value); }
        }

        public bool IsPressed
        {
            get { return GetValue(IsPressedProperty); }
            private set { SetValue(IsPressedProperty, value); }
        }

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public IconElement Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public CommandBarLabelPosition LabelPosition
        {
            get => GetValue(LabelPositionProperty);
            set => SetValue(LabelPositionProperty, value);
        }

        public FlyoutBase Flyout
        {
            get => _flyout;
            set 
            { 
                if (SetAndRaise(FlyoutProperty, ref _flyout, value))
                {
                    PseudoClasses.Set(":flyout", value != null);
                }
            }
        }

        protected override bool IsEnabledCore => base.IsEnabledCore && _commandCanExecute;


        public event EventHandler<RoutedEventArgs> Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            _appliedTemplate = false;
            base.OnApplyTemplate(e);
            _appliedTemplate = true;
            UpdateVisualStateForIconAndContent();
            UpdateToolTip();
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            if (_keyAcc != null)
            {
                KeyboardAccelerator = _keyAcc;
            }
            base.OnAttachedToLogicalTree(e);

            if (Command != null)
            {
                Command.CanExecuteChanged += CanExecuteChanged;
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            if (KeyboardAccelerator != null)
            {
                _keyAcc = KeyboardAccelerator;
                KeyboardAccelerator = null;
            }
            base.OnDetachedFromLogicalTree(e);
            if (Command != null)
            {
                Command.CanExecuteChanged -= CanExecuteChanged;
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter | e.Key == Key.Space)
            {
                IsPressed = true;
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter | e.Key == Key.Space)
            {
                IsPressed = false;
                OnClick();
                e.Handled = true;
            }
            base.OnKeyUp(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                IsPressed = true;
                e.Handled = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (IsPressed && e.InitialPressMouseButton == MouseButton.Left)
            {
                IsPressed = false;
                e.Handled = true;
                OnClick();
            }
        }

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            IsPressed = false;
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsPressedProperty)
            {
                UpdatePressedState();
            }
            else if (change.Property == CommandProperty)
            {
                if (((ILogical)this).IsAttachedToLogicalTree)
                {
                    if (change.OldValue.GetValueOrDefault() is ICommand oldC)
                    {
                        oldC.CanExecuteChanged -= CanExecuteChanged;
                    }
                    if (change.NewValue.GetValueOrDefault() is ICommand newC)
                    {
                        newC.CanExecuteChanged += CanExecuteChanged;
                    }

                    CanExecuteChanged(this, EventArgs.Empty);
                }                
            }
            else if (change.Property == LabelProperty || change.Property == IconProperty ||
                change.Property == LabelPositionProperty)
            {
                UpdateVisualStateForIconAndContent();
                UpdateToolTip();
            }
            else if (change.Property == KeyboardAcceleratorProperty)
            {
                UpdateToolTip();
                PseudoClasses.Set(":keyaccel", change.NewValue.GetValueOrDefault() != null);
            }
        }

        protected override void UpdateDataValidation<T>(AvaloniaProperty<T> property, BindingValue<T> value)
        {
            base.UpdateDataValidation(property, value);
            if (property == CommandProperty)
            {
                if (value.Type == BindingValueType.BindingError)
                {
                    if (_commandCanExecute)
                    {
                        _commandCanExecute = false;
                        UpdateIsEffectivelyEnabled();
                    }
                }
            }
        }

        protected virtual void OnClick()
        {
            if (_flyout != null)
            {
                OpenFlyout();
            }
            else
            {
                var e = new RoutedEventArgs(ClickEvent);
                RaiseEvent(e);

                if (!e.Handled && Command?.CanExecute(CommandParameter) == true)
                {
                    Command.Execute(CommandParameter);
                    e.Handled = true;
                }

                if (IsInOverflow)
                {
                    var pop = this.FindLogicalAncestorOfType<Popup>();
                    if (pop != null)
                    {
                        pop.IsOpen = false;
                    }
                }
                else
                {
                    var prnt = this.FindLogicalAncestorOfType<CommandBar>();
                    if (prnt != null && prnt.IsOpen)
                    {
                        prnt.IsOpen = false;
                    }
                }

            }            
        }

        protected virtual void OpenFlyout()
        {
            _flyout.ShowAt(this);
        }

        private void UpdatePressedState()
        {
            PseudoClasses.Set(":pressed", IsPressed);
        }

        private void UpdateVisualStateForIconAndContent()
        {
            bool hasIcon = Icon != null; 
            var lPos = LabelPosition;
            PseudoClasses.Set(":label", !string.IsNullOrEmpty(Label));

            //If we have an icon, all three states are possible. If only label, display under :labelright
            bool collapsed = !IsInOverflow && hasIcon && lPos == CommandBarLabelPosition.Collapsed;
            bool bottom = !collapsed && !IsInOverflow && hasIcon && lPos == CommandBarLabelPosition.Bottom;
            PseudoClasses.Set(":labelcollapsed", collapsed);
            PseudoClasses.Set(":labelright", !collapsed && !bottom);
            PseudoClasses.Set(":labelbottom", bottom);

            PseudoClasses.Set(":icon", hasIcon);
        }

        private void UpdateToolTip()
        {
            if (!_appliedTemplate)
                return;

            //TODO: Disable on overflow
            if (Icon != null && LabelPosition == CommandBarLabelPosition.Collapsed)
            {
                var expectedToolTip = KeyboardAccelerator == null ? Label : $"{Label} ({KeyboardAccelerator})";

                if (ToolTip.GetTip(this) != _suggestedToolTip)
                {
                    //ToolTip is manually set & we shouldn't change it
                }
                else
                {
                    ToolTip.SetTip(this, expectedToolTip);
                    _suggestedToolTip = expectedToolTip;
                }                
            }
            else
            {
                //Show a tooltip with the keyboard accelerator only if not manually set
                if (ToolTip.GetTip(this) == null)
                {
                    if (KeyboardAccelerator != null)
                    {
                        ToolTip.SetTip(this, $"{Label} ({KeyboardAccelerator})");
                    }
                    else
                    {
                        ToolTip.SetTip(this, null);
                    }                    
                }                
            }
        }

        private void CanExecuteChanged(object sender, EventArgs e)
        {
            bool canExec = (Command == null) || Command.CanExecute(CommandParameter);

            if (canExec != _commandCanExecute)
            {
                _commandCanExecute = canExec;
                UpdateIsEffectivelyEnabled();
            }
        }

        void ICommandSource.CanExecuteChanged(object sender, EventArgs e) => CanExecuteChanged(sender, e);

        private ICommand _command;
        private bool _commandCanExecute = true;
        private KeyGesture _keyAcc;
        private object _suggestedToolTip;
        private bool _appliedTemplate;
        private FlyoutBase _flyout;
    }
}
