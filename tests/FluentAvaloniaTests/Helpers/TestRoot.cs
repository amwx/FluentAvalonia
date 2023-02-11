using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Moq;

namespace FluentAvaloniaTests;

public class TestRoot : Decorator, IFocusScope, ILayoutRoot, IInputRoot, IRenderRoot, IStyleHost, ILogicalRoot
{
	public TestRoot()
	{
		//Renderer = Mock.Of<IRenderer>();
		LayoutManager = new LayoutManager(this);
		IsVisible = true;
        Renderer = new MockRenderer(); //Mock.Of<IRenderer>();
        MouseDevice = new MouseDevice();
    }

    public TestRoot(Size clientSize)
        : this()
    {
        ClientSize = clientSize;
    }

	public Size ClientSize { get; } = new Size(100, 100);
	public double LayoutScaling { get; } = 1;
	public ILayoutManager LayoutManager { get; }
	public IAccessKeyHandler AccessKeyHandler => null;
	public IKeyboardNavigationHandler KeyboardNavigationHandler => null;
	public IInputElement PointerOverElement { get; set; }
	public bool ShowAccessKeys { get; set; }
	public IMouseDevice MouseDevice { get; }
	public IRenderer Renderer { get; }
	public double RenderScaling => 1;
	public IStyleHost StylingParent { get; set; }

	public IRenderTarget CreateRenderTarget()
	{
		return null;
	}

	public void Invalidate(Rect rect)
	{
	}

	public Point PointToClient(PixelPoint point) => point.ToPoint(1);

	public PixelPoint PointToScreen(Point point) => PixelPoint.FromPoint(point, 1);
}

public class MockRenderer : IRenderer
{
    public bool DrawFps { get; set; }
    public bool DrawDirtyRects { get; set; }

    public event EventHandler<SceneInvalidatedEventArgs> SceneInvalidated;

    public void AddDirty(IVisual visual)
    {

    }

    public void Dispose()
    {

    }

    public IEnumerable<IVisual> HitTest(Point p, IVisual root, Func<IVisual, bool> filter)
    {
        return root.Bounds.Contains(p) ? new IVisual[] { root } : new IVisual[0];
    }

    public IVisual HitTestFirst(Point p, IVisual root, Func<IVisual, bool> filter)
    {
        return HitTest(p, root, filter).FirstOrDefault();
    }

    public void Paint(Rect rect)
    {

    }

    public void RecalculateChildren(IVisual visual)
    {

    }

    public void Resized(Size size)
    {

    }

    public void Start()
    {

    }

    public void Stop()
    {

    }
}
