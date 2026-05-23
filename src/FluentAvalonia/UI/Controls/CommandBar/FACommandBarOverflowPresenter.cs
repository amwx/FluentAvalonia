using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Styling;
using System.Collections;
using System.Collections.Specialized;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Displays the overflow content of a CommandBar.
/// </summary>
/// <remarks>
/// This class generally should not be used on your own and is meant for
/// the template of a <see cref="FACommandBar"/>
/// </remarks>
[PseudoClasses(s_pcIcons, s_pcToggle)]
public class FACommandBarOverflowPresenter : ItemsControl
{
    public FACommandBarOverflowPresenter()
    {
        ItemsView.CollectionChanged += ItemsCollectionChanged;
    }

    protected override Type StyleKeyOverride => typeof(FACommandBarOverflowPresenter);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            ItemsChanged(change);
        }
    }

    protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
    {
        recycleKey = null;
        return !(item is IFACommandBarElement);
    }

    private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                RegisterItems(e.NewItems);
                break;

            case NotifyCollectionChangedAction.Remove:
                UnregisterItems(e.OldItems);
                break;

            case NotifyCollectionChangedAction.Reset:
                // Reset may not be called with item list
                if (e.OldItems != null)
                {
                    UnregisterItems(e.OldItems);
                }
                else
                {
                    _hasIcons = 0;
                    _hasToggle = 0;
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                UnregisterItems(e.OldItems);
                RegisterItems(e.NewItems);
                break;
        }

        UpdateVisualState();
    }

    private void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _hasIcons = 0;
        _hasToggle = 0;

        if (e.NewValue is IList l)
        {
            RegisterItems(l);
        }

        UpdateVisualState();
    }

    private void RegisterItems(IList l)
    {
        for (int i = 0; i < l.Count; i++)
        {
            if (l[i] is FACommandBarButton cbb)
            {
                if (cbb.IconSource != null)
                    _hasIcons++;

                cbb.IsInOverflow = true;
            }
            else if (l[i] is FACommandBarToggleButton cbtb)
            {
                _hasToggle++;

                if (cbtb.IconSource != null)
                    _hasIcons++;

                cbtb.IsInOverflow = true;
            }
            else if (l[i] is FACommandBarElementContainer cont)
            {
                cont.IsInOverflow = true;
            }
            else if (l[i] is FACommandBarSeparator sep)
            {
                sep.IsInOverflow = true;
            }
        }
    }

    private void UnregisterItems(IList l)
    {
        for (int i = 0; i < l.Count; i++)
        {
            if (l[i] is FACommandBarButton cbb)
            {
                if (cbb.IconSource != null)
                    _hasIcons--;

                cbb.IsInOverflow = false;
                ((IPseudoClasses)cbb.Classes).Set(s_pcIcons, false);
                ((IPseudoClasses)cbb.Classes).Set(s_pcToggle, false);
            }
            else if (l[i] is FACommandBarToggleButton cbtb)
            {
                _hasToggle--;

                if (cbtb.IconSource != null)
                    _hasIcons--;

                cbtb.IsInOverflow = false;
                ((IPseudoClasses)cbtb.Classes).Set(s_pcIcons, false);
                ((IPseudoClasses)cbtb.Classes).Set(s_pcToggle, false);
            }
            else if (l[i] is FACommandBarElementContainer cont)
            {
                cont.IsInOverflow = false;
                ((IPseudoClasses)cont.Classes).Set(s_pcIcons, false);
                ((IPseudoClasses)cont.Classes).Set(s_pcToggle, false);
            }
            else if (l[i] is FACommandBarSeparator sep)
            {
                sep.IsInOverflow = false;
            }
        }
    }

    private void UpdateVisualState()
    {
        var items = Items as IList;

        bool icon = _hasIcons > 0;
        bool toggle = _hasToggle > 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] is Control c && c.Classes is IPseudoClasses pc)
            {
                pc.Set(s_pcIcons, icon);
                pc.Set(s_pcToggle, toggle);
            }
        }
    }

    private int _hasIcons;
    private int _hasToggle;
    private const string s_pcIcons = ":icons";
    private const string s_pcToggle = ":toggle";
}
