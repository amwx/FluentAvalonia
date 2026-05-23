using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using FluentAvalonia.UI.Controls;
using FAControlsGallery.Pages.NVSamplePages;

namespace FAControlsGallery.ViewModels;

public class NavViewPageViewModel : ViewModelBase
{
    public NavViewPageViewModel()
    {
        Categories = new List<CategoryBase>();

        Categories.Add(new Category { Name = "Category 1", Icon = FASymbol.Home, ToolTip = "This is category 1" });
        Categories.Add(new Category { Name = "Category 2", Icon = FASymbol.Keyboard, ToolTip = "This is category 2" });
        Categories.Add(new Separator());
        Categories.Add(new Category { Name = "Category 3", Icon = FASymbol.Library, ToolTip = "This is category 3" });
        Categories.Add(new Category { Name = "Category 4", Icon = FASymbol.Mail, ToolTip = "This is category 4" });

        SelectedCategory = Categories[0];
    }

    public List<CategoryBase> Categories { get; }

    public object SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedCategory, value);
            SetCurrentPage();
        }
    }
    public Control CurrentPage
    {
        get => _currentPage;
        set => RaiseAndSetIfChanged(ref _currentPage, value);
    }

    private void SetCurrentPage()
    {
        if (SelectedCategory is Category cat)
        {
            var index = Categories.IndexOf(cat) + 1;
            var smpPage = $"FAControlsGallery.Pages.NVSamplePages.NVSamplePage{index}";
            var pg = Activator.CreateInstance(Type.GetType(smpPage));
            CurrentPage = (Control)pg;
        }
        else if (SelectedCategory is FANavigationViewItem nvi)
        {
            var smpPage = $"FAControlsGallery.Pages.NVSamplePages.NVSamplePageSettings";
            var pg = Activator.CreateInstance(Type.GetType(smpPage));
            CurrentPage = (Control)pg;
        }
    }

    private object _selectedCategory;
    private Control _currentPage = new NVSamplePage1();
}

public abstract class CategoryBase { }

public class Category : CategoryBase
{
    public string Name { get; set; }
    public string ToolTip { get; set; }
    public FASymbol Icon { get; set; }
}

public class Separator : CategoryBase
{

}

public class MenuItemTemplateSelector : FADataTemplateSelector
{
    [Content]
    public IDataTemplate ItemTemplate { get; set; }

    public IDataTemplate SeparatorTemplate { get; set; }

    protected override IDataTemplate SelectTemplateCore(object item)
    {
        return item is Separator ? SeparatorTemplate : ItemTemplate;
    }
}
