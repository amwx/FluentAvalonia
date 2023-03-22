using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a header for a group of menu items in a NavigationMenu.
/// </summary>
[PseudoClasses(s_pcHeaderTextCollapsed, s_pcHeaderTextVisible)]
[PseudoClasses(s_pcTopMode)]
[TemplatePart(s_tpRootGrid, typeof(Grid))]
public class NavigationViewItemHeader : NavigationViewItemBase
{
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _splitViewRevokers?.Dispose();

        base.OnApplyTemplate(e);

        var splitView = GetSplitView;
        if (splitView != null)
        {
            _splitViewRevokers = new FACompositeDisposable(
                splitView.GetPropertyChangedObservable(SplitView.IsPaneOpenProperty).Subscribe(OnSplitViewPropertyChanged),
                splitView.GetPropertyChangedObservable(SplitView.DisplayModeProperty).Subscribe(OnSplitViewPropertyChanged));

            UpdateIsClosedCompact();
        }

        _rootGrid = e.NameScope.Find<Grid>(s_tpRootGrid);

        UpdateVisualState();
        UpdateItemIndentation();
    }

    protected override void OnNavigationViewItemBaseDepthChanged()
    {
        UpdateItemIndentation();
    }

    private void OnSplitViewPropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == SplitView.IsPaneOpenProperty ||
            args.Property == SplitView.DisplayModeProperty)
        {
            UpdateIsClosedCompact();
        }
    }

    private void UpdateIsClosedCompact()
    {
        var splitView = GetSplitView;
        if (splitView != null)
        {
            _isClosedCompact = !splitView.IsPaneOpen && (splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay ||
                splitView.DisplayMode == SplitViewDisplayMode.CompactInline);

            UpdateVisualState();
        }
    }

    private void UpdateVisualState()
    {
        //states :headertextcollapsed, :headertextvisible
        bool collapsed = _isClosedCompact && IsTopLevelItem;
        PseudoClasses.Set(s_pcHeaderTextCollapsed, collapsed);
        PseudoClasses.Set(s_pcHeaderTextVisible, !collapsed);

        var navView = GetNavigationView;
        if (navView != null)
        {
            PseudoClasses.Set(s_pcTopMode, navView.PaneDisplayMode == NavigationViewPaneDisplayMode.Top);
        }
    }

    private void UpdateItemIndentation()
    {
        if (_rootGrid == null)
            return;

        var oldMargin = _rootGrid.Margin;
        var newLeft = Depth * _itemIndentation;
        _rootGrid.Margin = new Thickness(newLeft, oldMargin.Top, oldMargin.Right, oldMargin.Bottom);
    }

    private IDisposable _splitViewRevokers;
    private Grid _rootGrid;
    private bool _isClosedCompact;

    private const string s_tpRootGrid = "RootGrid";

    private const string s_pcTopMode = ":topmode";
    private const string s_pcHeaderTextVisible = ":headertextvisible";
    private const string s_pcHeaderTextCollapsed = ":headertextcollapsed";
}
