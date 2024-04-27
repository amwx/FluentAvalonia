using Avalonia.Controls;
using System.Collections;

namespace FluentAvalonia.UI.Controls;

internal class VirtualLayoutContextAdapter : NonVirtualizingLayoutContext
{
    public VirtualLayoutContextAdapter(VirtualizingLayoutContext context)
    {
        _virtualizingContext = new WeakReference<VirtualizingLayoutContext>(context);
    }

    protected override IReadOnlyList<Control> ChildrenCore()
    {
        _children ??= new ChildrenCollection(GetContext());
        return _children;
    }

    protected internal override object LayoutStateCore 
    { 
        get => GetContext()?.LayoutStateCore;
        set
        {
            if (GetContext() is VirtualizingLayoutContext vlc)
                vlc.LayoutStateCore = value;
        } 
    }

    private VirtualizingLayoutContext GetContext() =>
        _virtualizingContext.TryGetTarget(out var target) ? target : null;

    private WeakReference<VirtualizingLayoutContext> _virtualizingContext;
    private ChildrenCollection _children;

    // WinUI makes this Generic, but C# doesn't like the indexer getting a control
    // with returning a generic type
    private class ChildrenCollection : IReadOnlyList<Control>
    {
        public ChildrenCollection(VirtualizingLayoutContext context)
        {
            _context = context;
        }

        public int Count => _context.ItemCount;

        public Control this[int index]
        {
            get => _context.GetOrCreateElementAt(index, ElementRealizationOptions.None);
        }

        public IEnumerator<Control> GetEnumerator()
        {
            int ct = Count;
            for (int i = 0; i < ct; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private VirtualizingLayoutContext _context;
    }
}
