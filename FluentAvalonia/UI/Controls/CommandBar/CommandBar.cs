using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// A basic toolbar implementation with Fluent styling applied.
    /// NOTE: This is NOT the WinUI CommandBar, I just like the name better than "toolbar"
    /// </summary>
    public class CommandBar : ContentControl, ICustomKeyboardNavigation
    {
        public CommandBar()
        {
            var newL = new AvaloniaList<CommandBarItemBase>();
            newL.CollectionChanged += OnPrimaryItemsChanged;
            _primaryCommands = newL;

            newL = new AvaloniaList<CommandBarItemBase>();
            newL.CollectionChanged += OnSecondaryItemsChanged;
            _secondaryCommands = newL;

        }


        public static readonly StyledProperty<bool> IsStickyProperty =
            AvaloniaProperty.Register<CommandBar, bool>("IsSticky");

        public static readonly DirectProperty<CommandBar, bool> IsOpenProperty =
            AvaloniaProperty.RegisterDirect<CommandBar, bool>("IsOpen",
                x => x.IsOpen, (x, v) => x.IsOpen = v);

        public static readonly DirectProperty<CommandBar, bool> IsDynamicOverflowEnabledProperty =
            AvaloniaProperty.RegisterDirect<CommandBar, bool>("IsDynamicOverflowEnabled",
                x => x.IsDynamicOverflowEnabled, (x,v) => x.IsDynamicOverflowEnabled = v);

        public static readonly StyledProperty<CommandBarClosedDisplayMode> ClosedDisplayModeProperty =
            AvaloniaProperty.Register<CommandBar, CommandBarClosedDisplayMode>("ClosedDisplayMode");

        public static readonly DirectProperty<CommandBar, IList<CommandBarItemBase>> PrimaryCommandsProperty =
            AvaloniaProperty.RegisterDirect<CommandBar, IList<CommandBarItemBase>>("PrimaryCommands",
                x => x.PrimaryCommands);

        public static readonly DirectProperty<CommandBar, IList<CommandBarItemBase>> SecondaryCommandsProperty =
            AvaloniaProperty.RegisterDirect<CommandBar, IList<CommandBarItemBase>>("SecondaryCommands",
                x => x.SecondaryCommands);

        public bool IsSticky
        {
            get => GetValue(IsStickyProperty);
            set => SetValue(IsStickyProperty, value);
        }

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (SetAndRaise(IsOpenProperty, ref _isOpen, value))
                {
                    if (_popup != null)
                    {
                        _popup.IsOpen = value;
                    }
                    PseudoClasses.Set(":open", value);
                }
            }
        }

        public bool IsDynamicOverflowEnabled
        {
            get => _dynamicOverflow;
            set
            {
                if (SetAndRaise(IsDynamicOverflowEnabledProperty, ref _dynamicOverflow, value))
                {

                }
            }
        }

        public CommandBarClosedDisplayMode ClosedDisplayMode
        {
            get => GetValue(ClosedDisplayModeProperty);
            set => SetValue(ClosedDisplayModeProperty, value);
        }

        public IList<CommandBarItemBase> PrimaryCommands
        {
            get => _primaryCommands;
        }

        public IList<CommandBarItemBase> SecondaryCommands
        {
            get => _secondaryCommands;
        }

        public event EventHandler<object> Opening;
        public event EventHandler<object> Opened;
        public event EventHandler<object> Closing;
        public event EventHandler<object> Closed;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            _appliedTemplate = false;
            if (_moreButton != null)
            {
                _moreButton.Click -= OnMoreButtonClick;
            }
            if (_popup != null)
            {
                _popup.Closed -= OnPopupClosed;
                _popup.Opened -= OnPopupOpened;
            }

            base.OnApplyTemplate(e);

            _content = e.NameScope.Find<ContentControl>("ContentControl");
            _primaryItemsHost = e.NameScope.Find<ItemsControl>("PrimaryItemsControl");
            _secondaryItemsHost = e.NameScope.Find<ItemsControl>("SecondaryItemsControl");
            _moreButton = e.NameScope.Find<Button>("MoreButton");
            if (_moreButton != null)
            {
                _moreButton.Click += OnMoreButtonClick;
            }
            _popup = e.NameScope.Find<Popup>("OverflowHost");
            if (_popup != null)
            {
                _popup.Closed += OnPopupClosed;
                _popup.Opened += OnPopupOpened;
            }

            _internalPrimaryList = new AvaloniaList<CommandBarItemBase>();
            _primaryItemsHost.Items = _internalPrimaryList;
                        
            _internalSecondaryList = new AvaloniaList<CommandBarItemBase>();
            _secondaryItemsHost.Items = _internalSecondaryList;
            
            _appliedTemplate = true;

            if (_primaryCommands.Count > 0)
            {
                _internalPrimaryList.AddRange(_primaryCommands);                
            }

            if (_secondaryCommands.Count > 0)
            {
                _internalSecondaryList.AddRange(_secondaryCommands);
                for (int i = 0; i < _internalSecondaryList.Count; i++)
                {
                    if (_internalSecondaryList[i] is CommandBarToggleButton cbtb)
                    {
                        cbtb.IsInOverflow = true;
                        numToggle++;

                        if (cbtb.Icon != null)
                            numIcons++;
                    }
                    else if (_internalSecondaryList[i] is CommandBarButton cbb)
                    {
                        cbb.IsInOverflow = true;
                        if (cbb.Icon != null)
                            numIcons++;
                    }
                    else if (_internalSecondaryList[i] is CommandBarItemBase cbi)
                    {
                        cbi.IsInOverflow = true;
                    }
                }

                PseudoClasses.Set(":overflowicons", numIcons > 0);
                PseudoClasses.Set(":overflowtoggle", numToggle > 0);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_isDirty || _dynamicOverflow)
            {
                HandleItems(availableSize);
            }

            return base.MeasureOverride(availableSize);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ClosedDisplayModeProperty)
            {
                UpdateVisualStateForDisplayMode();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                case Key.Up:
                case Key.Left:
                case Key.Right:
                    break;

                case Key.Escape:
                    break;
            }
            base.OnKeyDown(e);
        }


        protected virtual void OnClosing()
        {
            Closing?.Invoke(this, null);
        }

        protected virtual void OnClosed() 
        {
            Closed?.Invoke(this, null);
        }

        protected virtual void OnOpening() 
        {
            Opening?.Invoke(this, null);
        }

        protected virtual void OnOpened() 
        {
            Opened?.Invoke(this, null);
        }


        private void HandleItems(Size availableSize)
        {
            if (!_appliedTemplate || _primaryItemsHost == null || _secondaryItemsHost == null)
                return;

            if (_dynamicOverflow)
            {

                //Return overflowed items, if any...must do this so all primary items measure with the correct size
                for (int i = 0; i < _primaryCommands.Count; i++)
                {
                    if (_primaryCommands[i].Parent == _secondaryItemsHost)
                    {
                        _internalSecondaryList.Remove(_primaryCommands[i]);
                        _primaryCommands[i].IsInOverflow = false;
                        _internalPrimaryList.Insert(Math.Min(_internalPrimaryList.Count, i), _primaryCommands[i]);
                    }
                }

                Debug.Assert(_primaryCommands.Count == _internalPrimaryList.Count);

                if (_internalSecondaryList.Count != _secondaryCommands.Count)
                {
                    _internalSecondaryList.Clear();
                    _internalSecondaryList.AddRange(_secondaryCommands);
                }

                if (double.IsInfinity(availableSize.Width))
                {
                    //_internalPrimaryList.AddRange(_primaryCommands);
                    //_internalSecondaryList.AddRange(_secondaryCommands);
                }
                else
                {
                    _moreButton.IsVisible = true;
                    _content.Measure(availableSize);
                    _moreButton.Measure(availableSize);

                    //Always assume more button is visible
                    double constraint = availableSize.Width - _content.DesiredSize.Width - _moreButton.Width;

                    double width = 0;
                    int numOverflow = 0;
                    int numIconsOverflow = 0;
                    int numToggleOverflow = 0;
                    for (int i = 0; i < _primaryCommands.Count; i++)
                    {
                        _primaryCommands[i].Measure(availableSize);
                        if (_primaryCommands[i].DesiredSize.Width + width > constraint)
                        {
                            //Item goes to overflow
                            _internalPrimaryList.Remove(_primaryCommands[i]);
                            _internalSecondaryList.Insert(numOverflow, _primaryCommands[i]);
                            _primaryCommands[i].IsInOverflow = true;
                            numOverflow++;
                            if (_primaryCommands[i] is CommandBarToggleButton tb)
                            {
                                numToggleOverflow++;
                                if (tb.Icon != null)
                                    numIconsOverflow++;
                            }
                            else if (_primaryCommands[i] is CommandBarButton b)
                            {
                                if (b.Icon != null)
                                    numIconsOverflow++;
                            }
                            // If we don't signal we're at the overflow point, small items may
                            // end up remaining in PrimaryItems... This may be desired in some cases
                            // but isn't expected behavior for now. Add width to ensure all items after
                            // the first go to overflow
                            width += _primaryCommands[i].DesiredSize.Width;
                        }
                        else
                        {
                            width += _primaryCommands[i].DesiredSize.Width;
                        }
                    }

                    _moreButton.IsVisible = numOverflow > 0 || _secondaryCommands.Count > 0;

                    PseudoClasses.Set(":overflowicons", numIcons + numIconsOverflow > 0);
                    PseudoClasses.Set(":overflowtoggle", numToggle + numToggleOverflow > 0);
                }
            }
            else
            {
                if (_internalSecondaryList.Count != _secondaryCommands.Count)
                {
                    _internalSecondaryList.Clear();
                    _internalSecondaryList.AddRange(_secondaryCommands);
                }

                //_primaryItemsHost.Items = _primaryCommands;
                //_secondaryItemsHost.Items = _secondaryCommands;
            }
        }

        

        private void OnPrimaryItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_appliedTemplate)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        _internalPrimaryList.Insert(e.NewStartingIndex + i, (CommandBarItemBase)e.NewItems[i]);
                    }
                    
                    break;

                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        _internalPrimaryList.Remove((CommandBarItemBase)e.OldItems[i]);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _internalPrimaryList.Clear();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems.Count == 1)
                    {
                        _internalPrimaryList[e.NewStartingIndex] = (CommandBarItemBase)e.NewItems[0];
                    }                    
                    break;

                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException("Moving items at runtime is not supported");
            }

            _isDirty = true;
            InvalidateMeasure();
        }

        private void OnSecondaryItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        if (e.NewItems[i] is CommandBarToggleButton cbtb)
                        {
                            cbtb.IsInOverflow = true;
                            numToggle++;

                            if (cbtb.Icon != null)
                                numIcons++;
                        }
                        else if (e.NewItems[i] is CommandBarButton cbb)
                        {
                            cbb.IsInOverflow = true;
                            if (cbb.Icon != null)
                                numIcons++;
                        }                        
                        else if (e.NewItems[i] is CommandBarItemBase cbi)
                        {
                            cbi.IsInOverflow = true;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        if (e.OldItems[i] is CommandBarToggleButton cbtb)
                        {
                            cbtb.IsInOverflow = false;
                            numToggle--;

                            if (cbtb.Icon != null)
                                numIcons--;
                        } 
                        else if(e.OldItems[i] is CommandBarButton cbb)
                        {
                            cbb.IsInOverflow = false;
                            if (cbb.Icon != null)
                                numIcons--;
                        }
                        else if (e.OldItems[i] is CommandBarItemBase cbi)
                        {
                            cbi.IsInOverflow = false;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        if (e.OldItems[i] is CommandBarItemBase cbi)
                        {
                            cbi.IsInOverflow = false;
                        }
                    }
                    numIcons = 0;
                    numToggle = 0;
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        if (e.OldItems[i] is CommandBarToggleButton cbtb)
                        {
                            cbtb.IsInOverflow = false;
                            numToggle--;

                            if (cbtb.Icon != null)
                                numIcons--;
                        }
                        else if (e.OldItems[i] is CommandBarButton cbb)
                        {
                            cbb.IsInOverflow = false;
                            if (cbb.Icon != null)
                                numIcons--;
                        }                        
                        else if (e.OldItems[i] is CommandBarItemBase cbi)
                        {
                            cbi.IsInOverflow = false;
                        }
                    }
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        if (e.NewItems[i] is CommandBarToggleButton cbtb)
                        {
                            cbtb.IsInOverflow = true;
                            numToggle++;

                            if (cbtb.Icon != null)
                                numIcons++;
                        }
                        else if(e.NewItems[i] is CommandBarButton cbb)
                        {
                            cbb.IsInOverflow = true;
                            if (cbb.Icon != null)
                                numIcons++;
                        }                        
                        else if (e.NewItems[i] is CommandBarItemBase cbi)
                        {
                            cbi.IsInOverflow = true;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException("Cannot move commands during runtime");
            }
            _isDirty = true;
        }


        private void OnMoreButtonClick(object sender, RoutedEventArgs e)
        {
            IsOpen = !IsOpen;
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            OnOpened();
            _secondaryItemsHost?.Focus();
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            OnClosed();
            IsOpen = false;
        }

        private void UpdateVisualStateForDisplayMode()
        {
            var cdm = ClosedDisplayMode;
            PseudoClasses.Set(":minimal", cdm == CommandBarClosedDisplayMode.Minimal);
            PseudoClasses.Set(":hidden", cdm == CommandBarClosedDisplayMode.Hidden);
        }

        public (bool handled, IInputElement next) GetNext(IInputElement element, NavigationDirection direction)
        {            
            if (direction == NavigationDirection.Next)
            {
                if (element == this)//Default handling upon entrance
                    return (false, null);

                if (element == _moreButton && IsOpen)
                {
                    for (int i = 0; i < _internalSecondaryList.Count; i++)
                    {
                        if (_internalSecondaryList[i].Focusable)
                        {
                            return (true, _internalSecondaryList[i]);
                        }
                    }
                }
                
                if (_primaryItemsHost.IsVisualAncestorOf(element))
                {
                    var index = _internalPrimaryList.IndexOf((CommandBarItemBase)element);
                    if (index != -1)
                    {
                        for (int i = index + 1; i < _internalPrimaryList.Count; i++)
                        {
                            if (_internalPrimaryList[i].Focusable)
                            {
                                return (true, _internalPrimaryList[i]);
                            }
                        }

                        //Nothing found move to MoreButton
                        return (true, _moreButton);
                    }
                }                
            }
            else if (direction == NavigationDirection.Previous)
            {
                if (element == this) //Previous entrance goes to MoreButton
                { 
                    return (true, _moreButton); 
                }
                else if (element == _moreButton)
                {
                    for (int i = _internalPrimaryList.Count - 1; i >= 0; i--)
                    {
                        if (_internalPrimaryList[i].Focusable)
                        {
                            return (true, _internalPrimaryList[i]);
                        }
                    }
                }

                if (_primaryItemsHost.IsVisualAncestorOf(element))
                {
                    var index = _internalPrimaryList.IndexOf((CommandBarItemBase)element);                    
                    if (index != -1)
                    {
                        for (int i = index - 1; i >= 0; i--)
                        {
                            if (_internalPrimaryList[i].Focusable)
                            {
                                return (true, _internalPrimaryList[i]);
                            }
                        }

                    }
                }
            }

            return (false, null);
        }



        //In the overflow menu, Icons & ToggleButtons share space and things are adjusted by margins
        //So we need to keep track of how many we have of each so we can set the right Visual state
        private int numIcons = 0;
        private int numToggle = 0;

        //private bool _effMoreButtonVisibility;

        private ContentControl _content;
        private ItemsControl _primaryItemsHost;
        private ItemsControl _secondaryItemsHost;
        private Button _moreButton;
        private Popup _popup;

        private bool _appliedTemplate;
        private bool _isDirty;

        //These lists are attached to the presenters. We don't want to move items in PrimaryCommands
        //or SecondaryCommands when overflow is enabled, and we don't want completely remove & reattach
        //all controls either
        private AvaloniaList<CommandBarItemBase> _internalPrimaryList;
        private AvaloniaList<CommandBarItemBase> _internalSecondaryList;

        private IList<CommandBarItemBase> _primaryCommands;
        private IList<CommandBarItemBase> _secondaryCommands;
        private bool _dynamicOverflow = true;
        private bool _isOpen = false;
    }
}
