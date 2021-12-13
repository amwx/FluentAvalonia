using FluentAvalonia.UI.Controls;
using System;
using System.Runtime.InteropServices;
using Xunit;

namespace FluentAvaloniaTests.ControlTests
{
	public class CoreWindowTests
	{
		[Fact]
		public void CoreWindowImplCreatesOnWindowsOnly()
		{
			var cw = new CoreWindow();

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Assert.IsType<CoreWindowImpl>(cw.PlatformImpl);
			}
			else
			{
				Assert.IsNotType<CoreWindowImpl>(cw.PlatformImpl);
			}			
		}
	}
}
