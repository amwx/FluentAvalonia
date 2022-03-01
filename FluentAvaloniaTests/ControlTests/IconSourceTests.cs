using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaTests.Helpers;
using Xunit;

namespace FluentAvaloniaTests.ControlTests
{
    public class IconSourceTextContext : IDisposable
    {
        public IconSourceTextContext()
        {
            _appDisposable = UnitTestApplication.Start();

            Root = new TestRoot(new Size(1280, 720));
            Root.StylingParent = UnitTestApplication.Current;

            Root.Resources.Add("SymbolIcon", new SymbolIconSource { Symbol = Symbol.Save });            
        }

        public TestRoot Root { get; private set; }

        public void Dispose()
        {
            _appDisposable.Dispose();
        }

        private IDisposable _appDisposable;
    }

    public class IconSourceTests : IClassFixture<IconSourceTextContext>
    {
        public IconSourceTests(IconSourceTextContext ctx)
        {
            Context = ctx;
        }

        public IconSourceTextContext Context { get; }

        [Fact]
        public void IconSourceWithUnsetForegroundUsesInherited()
        {
            Context.Root.Child = new IconSourceElement
            {
                IconSource = (SymbolIconSource)(Context.Root.Resources["SymbolIcon"])
            };

            Context.Root.SetValue(TextBlock.ForegroundProperty, Brushes.Red);

            var sym = Context.Root.FindDescendantOfType<SymbolIcon>();

            Assert.NotNull(sym);

            Assert.Equal(Brushes.Red, sym.GetValue(TextBlock.ForegroundProperty));

            Context.Root.Child = null;
        }

        [Fact]
        public void IconSourceUsesSetForeground()
        {
            var ico = (SymbolIconSource)(Context.Root.Resources["SymbolIcon"]);
            ico.Foreground = Brushes.DarkSlateBlue;
            Context.Root.Child = new IconSourceElement
            {
                IconSource = ico
            };

            Context.Root.SetValue(TextBlock.ForegroundProperty, Brushes.Red);

            var sym = Context.Root.FindDescendantOfType<SymbolIcon>();

            Assert.NotNull(sym);

            Assert.Equal(Brushes.DarkSlateBlue, sym.GetValue(TextBlock.ForegroundProperty));

            Context.Root.Child = null;
        }

        [Fact]
        public void IconUpdatesForegroundIfChangedOnIconSource()
        {
            var ico = (SymbolIconSource)(Context.Root.Resources["SymbolIcon"]);
            ico.Foreground = Brushes.DarkSlateBlue;
            Context.Root.Child = new IconSourceElement
            {
                IconSource = ico
            };

            Context.Root.SetValue(TextBlock.ForegroundProperty, Brushes.Red);

            var sym = Context.Root.FindDescendantOfType<SymbolIcon>();

            Assert.NotNull(sym);

            Assert.Equal(Brushes.DarkSlateBlue, sym.GetValue(TextBlock.ForegroundProperty));

            ico.Foreground = Brushes.Yellow;

            Assert.Equal(Brushes.Yellow, sym.GetValue(TextBlock.ForegroundProperty));

            Context.Root.Child = null;
        }
    }
}
