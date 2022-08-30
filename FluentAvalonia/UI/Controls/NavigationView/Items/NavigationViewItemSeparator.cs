using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using System;
using System.Reactive.Disposables;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a line that separates menu items in a NavigationView.
/// </summary>
[PseudoClasses(":horizontal", ":horizontalcompact", ":vertical")]
public class NavigationViewItemSeparator : NavigationViewItemBase
{
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _appliedTemplate = false;

        _splitViewRevokers?.Dispose();

        base.OnApplyTemplate(e);

        _rootGrid = e.NameScope.Find<Panel>("RootGrid");

        var splitView = GetSplitView;
        if (splitView != null)
        {
            _splitViewRevokers = new CompositeDisposable(
                splitView.GetPropertyChangedObservable(SplitView.IsPaneOpenProperty).Subscribe(OnSplitViewPropertyChanged),
                splitView.GetPropertyChangedObservable(SplitView.DisplayModeProperty).Subscribe(OnSplitViewPropertyChanged));

            UpdateIsClosedCompact(false);
        }

        _appliedTemplate = true;
        UpdateVisualState();
        UpdateItemIndentation();
    }

    protected override void OnNavigationViewItemBaseDepthChanged()
    {
        UpdateItemIndentation();
    }

    protected override void OnNavigationViewItemBasePositionChanged()
    {
        UpdateVisualState();
    }

    private void OnSplitViewPropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        UpdateIsClosedCompact(true);
    }

    private void UpdateVisualState()
    {
        if (!_appliedTemplate)
            return;

        //States: :horizontalcompact, :horizontal, :vertical
        bool isTop = Position == NavigationViewRepeaterPosition.TopFooter || Position == NavigationViewRepeaterPosition.TopPrimary;

        PseudoClasses.Set(":horizontal", !isTop && !_isClosedCompact);
        PseudoClasses.Set(":horizontalcompact", !isTop && _isClosedCompact);
        PseudoClasses.Set(":vertical", isTop);
    }

    private void UpdateItemIndentation()
    {
        if (_rootGrid == null)
            return;

        var oldMargin = _rootGrid.Margin;
        var newLeft = Depth * _itemIndentation;
        _rootGrid.Margin = new Thickness(newLeft, oldMargin.Top, oldMargin.Right, oldMargin.Bottom);
    }

    private void UpdateIsClosedCompact(bool updateVisState)
    {
        var splitView = GetSplitView;
        if (splitView != null)
        {
            _isClosedCompact = !splitView.IsPaneOpen &&
                (splitView.DisplayMode == SplitViewDisplayMode.CompactInline || splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay);

            if (updateVisState)
                UpdateVisualState();
        }
    }

    private IDisposable _splitViewRevokers;
    private bool _appliedTemplate;
    private bool _isClosedCompact;
    private Panel _rootGrid;
}
