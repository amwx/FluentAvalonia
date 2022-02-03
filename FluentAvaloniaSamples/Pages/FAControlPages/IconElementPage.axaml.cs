using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Collections;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages
{
    public partial class IconElementPage : FAControlsPageBase
    {
        public IconElementPage()
        {
            InitializeComponent();

            TargetType = typeof(IconElement);
            WinUINamespace = "Microsoft.UI.Xaml.Controls.IconElement";
            WinUIDocsLink = new Uri("https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.iconelement?view=winui-3.0");
            WinUIGuidelinesLink = new Uri("https://docs.microsoft.com/en-us/windows/apps/design/style/icons");
            Description = "Represents the base class for icon controls that use different image types as its content";

            DataContext = new IconElementPageViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public AvaloniaList<Symbol> Symbols
        {
            get
            {
                if (_symbols.Count == 0)
                {

                }

                return _symbols;
            }
        }

        

        private AvaloniaList<Symbol> _symbols = new AvaloniaList<Symbol>();
    }
}
