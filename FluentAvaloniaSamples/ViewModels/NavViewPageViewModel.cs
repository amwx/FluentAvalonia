using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.Pages.NVSamplePages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FluentAvaloniaSamples.ViewModels
{
    public class NavViewPageViewModel : ViewModelBase
    {
        public NavViewPageViewModel()
        {
            Categories = new List<CategoryBase>();

			Categories.Add(new Category { Name = "Category 1", Icon = Symbol.Home, ToolTip = "This is category 1" });
			Categories.Add(new Category { Name = "Category 2", Icon = Symbol.Keyboard, ToolTip = "This is category 2" });
			Categories.Add(new Separator());
			Categories.Add(new Category { Name = "Category 3", Icon = Symbol.Library, ToolTip = "This is category 3" });
			Categories.Add(new Category { Name = "Category 4", Icon = Symbol.Mail, ToolTip = "This is category 4" });

			SelectedCategory = Categories[0];			
        }

		public string Header => DescriptionServiceProvider.Instance.GetInfo("NavigationView", "Header");

        public string NavViewDefaultXaml => DescriptionServiceProvider.Instance.GetInfo("NavigationView", "NavigationViewDefault", "XamlSource");

		public string NavViewAdaptiveXaml => DescriptionServiceProvider.Instance.GetInfo("NavigationView", "NavigationViewAdaptive", "XamlSource");
		public string NavViewAdaptiveNotes => DescriptionServiceProvider.Instance.GetInfo("NavigationView", "NavigationViewAdaptive", "UsageNotes");

		public string NavViewSelectionFocusXaml => DescriptionServiceProvider.Instance.GetInfo("NavigationView", "NavigationViewSelectionFollowsFocus", "XamlSource");
		public string NavViewSelectionFocusNotes => DescriptionServiceProvider.Instance.GetInfo("NavigationView", "NavigationViewSelectionFollowsFocus", "UsageNotes");


		public string NavViewBindingXaml => DescriptionServiceProvider.Instance.GetInfo("NavigationView", "NavigationViewDataBinding", "XamlSource");
		public string NavViewBindingNotes => DescriptionServiceProvider.Instance.GetInfo("NavigationView", "NavigationViewDataBinding", "UsageNotes");

		public string NavViewHierarchicalXaml => DescriptionServiceProvider.Instance.GetInfo("NavigationView", "NavigationViewDataHierarchical", "UsageNotes");


		public List<CategoryBase> Categories { get; }   

        public NavigationViewPaneDisplayMode APIInActionNavViewPaneMode
        {
            get => _paneMode;
            set => this.RaiseAndSetIfChanged(ref _paneMode, value);
        }

        public object SelectedCategory
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
			if (SelectedCategory is Category cat)
			{
				var index = Categories.IndexOf(cat) + 1;
				var smpPage = $"FluentAvaloniaSamples.Pages.NVSamplePages.NVSamplePage{index}";
				var pg = Activator.CreateInstance(Type.GetType(smpPage));
				CurrentPage = (IControl)pg;
			}
			else if (SelectedCategory is NavigationViewItem nvi)
			{
				var smpPage = $"FluentAvaloniaSamples.Pages.NVSamplePages.NVSamplePageSettings";
				var pg = Activator.CreateInstance(Type.GetType(smpPage));
				CurrentPage = (IControl)pg;
			}
        }

        private NavigationViewPaneDisplayMode _paneMode = NavigationViewPaneDisplayMode.Left;
        private bool _leftMode = true;
        private object _selectedCategory;
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

	public class MenuItemTemplateSelector : DataTemplateSelector
	{
		[Content]
		public IDataTemplate ItemTemplate { get; set; }

		public IDataTemplate SeparatorTemplate { get; set; }

		protected override IDataTemplate SelectTemplateCore(object item)
		{
			return item is Separator ? SeparatorTemplate : ItemTemplate;
		}
	}
}
