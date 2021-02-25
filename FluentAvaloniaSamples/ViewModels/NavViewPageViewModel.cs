using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.Pages.NVSamplePages;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FluentAvaloniaSamples.ViewModels
{
    public class NavViewPageViewModel : ViewModelBase
    {
        public NavViewPageViewModel()
        {
            Categories = new List<Category>();

            Categories.Add(new Category { Name = "Category 1", Icon = Symbol.Home, ToolTip = "This is category 1" });
            Categories.Add(new Category { Name = "Category 2", Icon = Symbol.Keyboard, ToolTip = "This is category 2" });
            Categories.Add(new Category { Name = "Category 3", Icon = Symbol.Library, ToolTip = "This is category 3" });
            Categories.Add(new Category { Name = "Category 4", Icon = Symbol.Mail, ToolTip = "This is category 4" });

            SelectedCategory = Categories[0];

            XElement xe = XElement.Parse(GetAssemblyResource("FluentAvaloniaInfo.txt"));
            var pages = xe.Elements("ControlPage").Where(x => x.Attribute("Name").Value == "NavigationView").First();

            Header = pages.Element("Header").Value;
            var controls = pages.Elements("Control");
            foreach (var ctrl in controls)
            {
                if (ctrl.Attribute("Name").Value == "NavigationViewDefault")
                {
                    NavViewDefaultXaml = ctrl.Element("XamlSource").Value;
                }
                else if (ctrl.Attribute("Name").Value == "NavigationViewAdaptive")
                {
                    NavViewAdaptiveXaml = ctrl.Element("XamlSource").Value;
                    NavViewAdaptiveNotes = ctrl.Element("UsageNotes").Value;
                }
                else if (ctrl.Attribute("Name").Value == "NavigationViewDataBinding")
                {
                    NavViewBindingXaml = ctrl.Element("XamlSource").Value;
                }
                else if (ctrl.Attribute("Name").Value == "NavigationViewDataHeirarchical")
                {
                    NavViewHierarchicalXaml = ctrl.Element("XamlSource").Value;
                }
            }
        }

        public string Header { get; }

        public string NavViewDefaultXaml { get; }

        public string NavViewAdaptiveXaml { get; }
        public string NavViewAdaptiveNotes { get; }

        public string NavViewBindingXaml { get; }

        public string NavViewHierarchicalXaml { get; }


        public List<Category> Categories { get; }   

        public NavigationViewPaneDisplayMode APIInActionNavViewPaneMode
        {
            get => _paneMode;
            set => this.RaiseAndSetIfChanged(ref _paneMode, value);
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedCategory, value);
                SetCurrentPage();
            }
        }

        public IControl CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        public bool APIInActionNavViewLeft
        {
            get => _leftMode;
            set
            {
                this.RaiseAndSetIfChanged(ref _leftMode, value);
                APIInActionNavViewPaneMode = value ? NavigationViewPaneDisplayMode.Left : NavigationViewPaneDisplayMode.Top;
            }
        }

        public bool APIInActionNavViewTop
        {
            get => !_leftMode;
            set
            {
                if (value) return;
                if (!this.RaiseAndSetIfChanged(ref _leftMode, !value))
                {
                    APIInActionNavViewPaneMode = value ? NavigationViewPaneDisplayMode.Top : NavigationViewPaneDisplayMode.Left;
                }
            }
        }

        private void SetCurrentPage()
        {
            //if (SelectedCategory)
            var index = Categories.IndexOf(SelectedCategory) + 1;
            var smpPage = $"FluentAvaloniaSamples.Pages.NVSamplePages.NVSamplePage{index}";
            var pg = Activator.CreateInstance(Type.GetType(smpPage));
            CurrentPage = (IControl)pg;
        }

        private NavigationViewPaneDisplayMode _paneMode = NavigationViewPaneDisplayMode.Left;
        private bool _leftMode = true;
        private Category _selectedCategory;
        private IControl _currentPage = new NVSamplePage1();
    }

    public abstract class CategoryBase { }

    public class Category : CategoryBase
    {
        public string Name { get; set; }
        public string ToolTip { get; set; }
        public Symbol Icon { get; set; }
    }

    public class Separator : CategoryBase
    {

    }
}
