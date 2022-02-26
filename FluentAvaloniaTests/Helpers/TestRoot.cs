using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Styling;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvaloniaTests
{
    public class TestRoot : Decorator, IFocusScope, ILayoutRoot, IInputRoot, IRenderRoot, IStyleHost, ILogicalRoot
	{
		public TestRoot()
		{
			//Renderer = Mock.Of<IRenderer>();
			LayoutManager = new LayoutManager(this);
			IsVisible = true;
            Renderer = Mock.Of<IRenderer>();
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
}
