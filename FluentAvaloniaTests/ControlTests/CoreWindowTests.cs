using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using System;
using System.Runtime.InteropServices;
using Xunit;

namespace FluentAvaloniaTests.ControlTests;

public class CoreWindowTests
{
	// Need more robust test infrastructure for this. Creating windows fails without proper setup...

	//[Fact]
	//public void CoreWindowImplCreatesOnWindowsOnly()
	//{
	//	var cw = new CoreWindow();

	//	if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
	//	{
	//		Assert.IsType<CoreWindowImpl>(cw.PlatformImpl);
	//	}
	//	else
	//	{
	//		Assert.IsNotType<CoreWindowImpl>(cw.PlatformImpl);
	//	}
	//}
}
