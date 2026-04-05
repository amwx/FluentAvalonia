using Avalonia.Controls;
using System.Collections;

namespace FluentAvalonia.UI.Controls;

internal class VirtualLayoutContextAdapter : FANonVirtualizingLayoutContext
{
    public VirtualLayoutContextAdapter(FAVirtualizingLayoutContext context)
    {
        _virtualizingContext = new WeakReference<FAVirtualizingLayoutContext>(context);
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
            if (GetContext() is FAVirtualizingLayoutContext vlc)
                vlc.LayoutStateCore = value;
        } 
    }

    private FAVirtualizingLayoutContext GetContext() =>
        _virtualizingContext.TryGetTarget(out var target) ? target : null;

    private WeakReference<FAVirtualizingLayoutContext> _virtualizingContext;
    private ChildrenCollection _children;

    // WinUI makes this Generic, but C# doesn't like the indexer getting a control
    // with returning a generic type
    private class ChildrenCollection : IReadOnlyList<Control>
    {
        public ChildrenCollection(FAVirtualizingLayoutContext context)
        {
            _context = context;
        }

        public int Count => _context.ItemCount;

        public Control this[int index]
        {
            get => _context.GetOrCreateElementAt(index, FAElementRealizationOptions.None);
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

        private FAVirtualizingLayoutContext _context;
    }
}
