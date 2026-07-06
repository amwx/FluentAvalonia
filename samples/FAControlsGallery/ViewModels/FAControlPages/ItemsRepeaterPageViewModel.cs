using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.ViewModels;

public sealed class ItemsRepeaterPageViewModel : ViewModelBase
{
    public ItemsRepeaterPageViewModel()
    {
        var rnd = new Random(7);
        BarItems = new List<Bar>(50);
        for (int i = 0; i < 50; i++)
        {
            BarItems.Add(new Bar(rnd.NextDouble() * 300, 300));
        }

        ItemsStretchItems = Enum.GetValues<FAUniformGridLayoutItemsStretch>();
        ItemsJustificationItems = Enum.GetValues<FAUniformGridLayoutItemsJustification>();
        OrientationItems = Enum.GetValues<Orientation>();
        LineAlignmentItems = Enum.GetValues<FAFlowLayoutLineAlignment>();

        MixedDataSource = new List<object>
        {
            64,
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
            128,
            "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
            256,
            "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.",
            512,
            "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
            1024
        };

        NestedCategories = new List<NestedCategory>
        {
            new NestedCategory("Fruits", GetFruits()),
            new NestedCategory("Vegetables", GetVegetables()),
            new NestedCategory("Grains", GetGrains()),
            new NestedCategory("Proteins", GetProteins())
        };

        Numbers = new ObservableCollection<int>(Enumerable.Range(0, 500));
    }

    public List<Bar> BarItems { get; }

    public FAUniformGridLayoutItemsStretch[] ItemsStretchItems { get; }

    public FAUniformGridLayoutItemsJustification[] ItemsJustificationItems { get; }

    public FAFlowLayoutLineAlignment[] LineAlignmentItems { get; }

    public Orientation[] OrientationItems { get; }

    public List<object> MixedDataSource { get; }

    public List<NestedCategory> NestedCategories { get; }

    public ObservableCollection<int> Numbers { get; }

    private ObservableCollection<string> GetFruits()
    {
        return new ObservableCollection<string> { "Apricots", "Bananas", "Grapes", "Strawberries", "Watermelon", "Plums", "Blueberries" };
    }

    private ObservableCollection<string> GetVegetables()
    {
        return new ObservableCollection<string> { "Broccoli", "Spinach", "Sweet potato", "Cauliflower", "Onion", "Brussels sprouts", "Carrots" };
    }
    private ObservableCollection<string> GetGrains()
    {
        return new ObservableCollection<string> { "Rice", "Quinoa", "Pasta", "Bread", "Farro", "Oats", "Barley" };
    }
    private ObservableCollection<string> GetProteins()
    {
        return new ObservableCollection<string> { "Steak", "Chicken", "Tofu", "Salmon", "Pork", "Chickpeas", "Eggs" };
    }
}

public sealed class Bar
{
    public Bar(double length, int max)
    {
        Length = length;
        MaxLength = max;

        Height = length / 4;
        MaxHeight = max / 4;

        Diameter = length / 6;
        MaxDiameter = max / 6;
    }
    public double Length { get; set; }
    public int MaxLength { get; set; }

    public double Height { get; set; }
    public double MaxHeight { get; set; }

    public double Diameter { get; set; }
    public double MaxDiameter { get; set; }
}

public sealed class MyDataTemplateSelector : AvaloniaObject, IDataTemplate
{
    public static readonly StyledProperty<IDataTemplate> NormalProperty = 
        AvaloniaProperty.Register<MyDataTemplateSelector, IDataTemplate>(nameof(Normal));

    public static readonly StyledProperty<IDataTemplate> AccentProperty = 
        AvaloniaProperty.Register<MyDataTemplateSelector, IDataTemplate>(nameof(Accent));

    public IDataTemplate Normal
    {
        get => GetValue(NormalProperty);
        set => SetValue(NormalProperty, value);
    }

    public IDataTemplate Accent
    {
        get => GetValue(AccentProperty);
        set => SetValue(AccentProperty, value);
    }

    public Control Build(object param)
    {
        if ((int)param % 2 == 0)
        {
            return Normal.Build(param);
        }
        else
        {
            return Accent.Build(param);
        }
    }

    public bool Match(object data) => true;
}

public sealed class StringOrIntTemplateSelector : AvaloniaObject, IDataTemplate
{
    public static readonly StyledProperty<IDataTemplate> StringTemplateProperty =
        AvaloniaProperty.Register<MyDataTemplateSelector, IDataTemplate>(nameof(StringTemplate));

    public static readonly StyledProperty<IDataTemplate> IntTemplateProperty =
        AvaloniaProperty.Register<MyDataTemplateSelector, IDataTemplate>(nameof(IntTemplate));

    public IDataTemplate StringTemplate
    {
        get => GetValue(StringTemplateProperty);
        set => SetValue(StringTemplateProperty, value);
    }

    public IDataTemplate IntTemplate
    {
        get => GetValue(IntTemplateProperty);
        set => SetValue(IntTemplateProperty, value);
    }

    public Control Build(object param)
    {
        if (param is string)
        {
            return StringTemplate.Build(param);
        }
        else if (param is int)
        {
            return IntTemplate.Build(param);
        }

        return null;
    }

    public bool Match(object data) => true;
}

public sealed class Recipe
{
    public int Num { get; set; }
    public string Ingredients { get; set; } = string.Empty;
    public List<string> IngList { get; set; } = [];
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int NumIngredients
    {
        get
        {
            return IngList.Count();
        }
    }

    public void RandomizeIngredients()
    {
        // To give the items different heights, give recipes random numbers of random ingredients
        Random rndNum = new Random();
        Random rndIng = new Random();

        ObservableCollection<string> extras = new ObservableCollection<string>{
                                                     "Garlic",
                                                     "Lemon",
                                                     "Butter",
                                                     "Lime",
                                                     "Feta Cheese",
                                                     "Parmesan Cheese",
                                                     "Breadcrumbs"};
        for (int i = 0; i < rndNum.Next(0, 4); i++)
        {
            string newIng = extras[rndIng.Next(0, 6)];
            if (!IngList.Contains(newIng))
            {
                Ingredients += "\n" + newIng;
                IngList.Add(newIng);
            }
        }

    }
}

// Custom data source class that assigns elements unique IDs, making filtering easier
public sealed class MyItemsSource : IList, IFAKeyIndexMapping, INotifyCollectionChanged
{
    private List<Recipe> inner = new List<Recipe>();

    public MyItemsSource(IEnumerable<Recipe> collection)
    {
        InitializeCollection(collection);
    }

    public void InitializeCollection(IEnumerable<Recipe> collection)
    {
        inner.Clear();
        if (collection != null)
        {
            inner.AddRange(collection);
        }

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    #region IReadOnlyList<T>
    public int Count => this.inner != null ? this.inner.Count : 0;

    public object this[int index]
    {
        get
        {
            return inner[index] as Recipe;
        }

        set
        {
            if (value is not Recipe recipe)
            {
                throw new ArgumentException("Value must be of type Recipe.", nameof(value));
            }

            inner[index] = recipe;
        }
    }

    public IEnumerator<Recipe> GetEnumerator() => this.inner.GetEnumerator();

    #endregion

    #region INotifyCollectionChanged
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    #endregion

    #region IKeyIndexMapping
    public string KeyFromIndex(int index)
    {
        return inner[index].Num.ToString();
    }

    public int IndexFromKey(string key)
    {
        foreach (Recipe item in inner)
        {
            if (item.Num.ToString() == key)
            {
                return inner.IndexOf(item);
            }
        }
        return -1;
    }

    #endregion

    #region Unused List methods
    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public int Add(object value)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(object value)
    {
        throw new NotImplementedException();
    }

    public int IndexOf(object value)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, object value)
    {
        throw new NotImplementedException();
    }

    public void Remove(object value)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(Array array, int index)
    {
        throw new NotImplementedException();
    }

    public bool IsFixedSize => throw new NotImplementedException();

    public bool IsReadOnly => throw new NotImplementedException();

    public bool IsSynchronized => throw new NotImplementedException();

    public object SyncRoot => throw new NotImplementedException();

    #endregion
}

public sealed class NestedCategory
{
    public string CategoryName { get; set; }
    public ObservableCollection<string> CategoryItems { get; set; }
    public NestedCategory(string catName, ObservableCollection<string> catItems)
    {
        CategoryName = catName;
        CategoryItems = catItems;
    }
}
