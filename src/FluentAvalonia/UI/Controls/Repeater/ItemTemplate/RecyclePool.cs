#pragma warning disable
// Note this class has no documentation yet from Microsoft - disabling the warnings around
// public APIs with no documentation
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace FluentAvalonia.UI.Controls;

public class RecyclePool
{
    internal static readonly AttachedProperty<IDataTemplate> OriginTemplateProperty =
        AvaloniaProperty.RegisterAttached<RecyclePool, Control, IDataTemplate>("OriginTemplate");

    internal static readonly AttachedProperty<string> ReuseKeyProperty =
        AvaloniaProperty.RegisterAttached<RecyclePool, Control, string>("ReuseKey");

    public static string GetReuseKey(Control element) =>
        element.GetValue(ReuseKeyProperty);

    public static void SetReuseKey(Control element, string key) =>
        element.SetValue(ReuseKeyProperty, key);

    public void PutElement(Control element, string key) =>
        PutElementCore(element, key, null /* owner */);

    public void PutElement(Control element, string key, Control owner) =>
        PutElementCore(element, key, owner);

    public Control TryGetElement(string key) =>
        TryGetElementCore(key, null /*owner*/);

    public Control TryGetElement(string key, Control owner) =>
        TryGetElementCore(key, owner);

    protected virtual void PutElementCore(Control element, string key, Control owner)
    {
        EnsureOwnerIsPanelOrNull(owner);

        var elementInfo = new ElementInfo(element, owner as Panel);

        if (_elements.TryGetValue(key, out var value))
        {
            value.Add(elementInfo);
        }
        else
        {
            var pool = new List<ElementInfo>();
            pool.Add(elementInfo);
            _elements.Add(key, pool);
        }
    }
    
    protected virtual Control TryGetElementCore(string key, Control owner)
    {
        if (_elements.TryGetValue(key, out var elements))
        {
            if (elements.Count > 0)
            {
                ElementInfo elementInfo = default;
                bool found = false;
                for (int i = 0; i < elements.Count; i++)
                {
                    var x = elements[i];
                    if (x.Owner == owner || x.Owner == null)
                    {
                        elementInfo = x;
                        elements.RemoveAt(i);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    elementInfo = elements[elements.Count - 1];
                    elements.RemoveAt(elements.Count - 1);
                }

                EnsureOwnerIsPanelOrNull(owner);
                if (elementInfo.Owner != null && elementInfo.Owner != owner)
                {
                    // Element is still under its parent. remove it from its parent.
                    var panel = elementInfo.Owner as Panel;
                    if (panel != null)
                    {
                        bool foundE = panel.Children.Remove(elementInfo.Element);
                        if (!foundE)
                            throw new Exception("ItemsRepeater's child not found in its Children collection.");
                    }
                }

                return elementInfo.Element;
            }
        }

        return null;
    }

    private void EnsureOwnerIsPanelOrNull(Control owner)
    {
        if (owner == null || (owner != null && owner is Panel))
            return;

        throw new InvalidOperationException("Owner must to be a Panel or null.");
    }


    // RecyclePoolFactory.cpp

    public static RecyclePool GetPoolInstance(IDataTemplate template)
    {
        if (s_PoolInstance == null)
            s_PoolInstance = new Dictionary<IDataTemplate, RecyclePool>();

        if (s_PoolInstance.TryGetValue(template, out var rp))
            return rp;

        return null;
    }

    public static void SetPoolInstance(IDataTemplate template, RecyclePool pool)
    {
        if (s_PoolInstance == null)
            s_PoolInstance = new Dictionary<IDataTemplate, RecyclePool>();

        s_PoolInstance.Add(template, pool);
    }


    private struct ElementInfo
    {
        public ElementInfo(Control element, Panel owner)
        {
            Element = element;
            Owner = owner;
        }

        public Control Element;
        public Panel Owner;
    }

    private readonly Dictionary<string, List<ElementInfo>> _elements = 
        new Dictionary<string, List<ElementInfo>>();

    // WinUI stores this as a DependencyProperty on DataTemplate (attached), but since
    // we use IDataTemplate, we need a cache not tied to the property system
    private static Dictionary<IDataTemplate, RecyclePool> s_PoolInstance;
}
