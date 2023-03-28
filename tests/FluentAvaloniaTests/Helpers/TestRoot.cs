using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Styling;

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
    public RendererDiagnostics Diagnostics { get; } = new();

    public event EventHandler<SceneInvalidatedEventArgs>? SceneInvalidated;

    public MockRenderer()
    {
    }

    public void AddDirty(Visual visual)
    {
    }

    public void Dispose()
    {
    }

    public IEnumerable<Visual> HitTest(Point p, Visual root, Func<Visual, bool> filter)
        => Enumerable.Empty<Visual>();

    public Visual? HitTestFirst(Point p, Visual root, Func<Visual, bool> filter)
        => null;

    public void Paint(Rect rect)
    {
    }

    public void RecalculateChildren(Visual visual)
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

    public ValueTask<object?> TryGetRenderInterfaceFeature(Type featureType)
        => new((object?)null);
}
