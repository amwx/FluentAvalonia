using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaTests.Helpers;
using Xunit;

namespace FluentAvaloniaTests.ControlTests
{
    public class ContentDialogTestContext : IDisposable
    {
        public ContentDialogTestContext()
        {
            _appDisposable = UnitTestApplication.Start();

            Dialog = CreateDialog();
            Root.LayoutManager.ExecuteInitialLayoutPass();
            Root.LayoutManager.ExecuteLayoutPass();
        }

        public ContentDialog Dialog { get; }

        public TestRoot Root { get; private set; }

        public ContentDialog CreateDialog()
        {
            Root = new TestRoot(new Avalonia.Size(1280, 720));
            Root.StylingParent = UnitTestApplication.Current;

            //UnitTestApplication.Current.Styles.Add(
            //   (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/ContentControl.axaml")));
            UnitTestApplication.Current.Styles.Add(
               (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/Controls/ContentDialogStyles.axaml")));
            UnitTestApplication.Current.Styles.Add(
                (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/ScrollBarStyles.axaml")));
            UnitTestApplication.Current.Styles.Add(
                (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/ScrollViewerStyles.axaml")));
            UnitTestApplication.Current.Styles.Add(
               (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://FluentAvalonia/Styling/BasicControls/ButtonStyles.axaml")));

            // ContentDialog is an async process which is hard to unit test, so force create and add to tree here
            var cd = new ContentDialog();
            cd.Opacity = 1;
            cd.IsVisible = true;
            Root.Child = cd;

            return cd;
        }

        public void Dispose()
        {
            _appDisposable.Dispose();
        }

        private IDisposable _appDisposable;
    }

    public class ContentDialogTests : IClassFixture<ContentDialogTestContext>
    {
        public ContentDialogTests(ContentDialogTestContext ctx)
        {
            Context = ctx;
        }

        [Fact]
        public void NotSettingButtonTextShowsNoButtons()
        {
            ResetDialog();
            Context.Dialog.SetupDialog();

            var items = Context.Dialog.GetVisualDescendants().OfType<Button>();

            foreach(var item in items)
            {
                Assert.False(item.IsVisible);
            }
        }

        [Fact]
        public void SettingPrimaryButtonTextShowsPrimaryButton()
        {
            ResetDialog();
            Context.Dialog.PrimaryButtonText = "Primary";
            Context.Dialog.SetupDialog();

            var button = Context.Dialog.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "PrimaryButton").FirstOrDefault();
            Assert.NotNull(button);
            
            Assert.True(button.IsVisible);
            Assert.Equal("Primary", button.Content);
        }

        [Fact]
        public void SettingSecondaryButtonTextShowsSecondaryButton()
        {
            ResetDialog();
            Context.Dialog.SecondaryButtonText = "Secondary";
            Context.Dialog.SetupDialog();

            var button = Context.Dialog.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "SecondaryButton").FirstOrDefault();
            Assert.NotNull(button);

            Assert.True(button.IsVisible);
            Assert.Equal("Secondary", button.Content);
        }

        [Fact]
        public void SettingCloseButtonTextShowsCloseButton()
        {
            ResetDialog();
            Context.Dialog.CloseButtonText = "Close";
            Context.Dialog.SetupDialog();

            var button = Context.Dialog.GetVisualDescendants().OfType<Button>().Where(x => x.Name == "CloseButton").FirstOrDefault();
            Assert.NotNull(button);

            Assert.True(button.IsVisible);
            Assert.Equal("Close", button.Content);
        }

        private void ResetDialog()
        {
            Context.Dialog.PrimaryButtonText = null;
            Context.Dialog.SecondaryButtonText = null;
            Context.Dialog.CloseButtonText = null;
            Context.Dialog.Title = null;
            Context.Dialog.Content = null;
        }

        public ContentDialogTestContext Context { get; }
    }
}
