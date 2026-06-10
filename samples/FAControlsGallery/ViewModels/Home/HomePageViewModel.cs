using Avalonia.Collections;
using FAControlsGallery.Services;
using FAControlsGallery.ViewModels.DesignPages;
using FluentAvalonia.Collections;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.ViewModels;

public sealed class HomePageViewModel : MainPageViewModelBase
{
    public HomePageViewModel()
    {
        Pages = new List<HomeNavPageViewModel>
        {
            new HomeNavPageViewModel("Documentation", new Uri("https://amwx.github.io/FluentAvaloniaDocs/"))
            {
                ImageUri = "avares://FAControlsGallery/Assets/Images/Documentation.png"
            },
            new HomeNavPageViewModel("Github Repo", new Uri("https://www.github.com/amwx/FluentAvalonia"))
            {
                ImageUri = "avares://FAControlsGallery/Assets/Images/Github.png"
            },
            new HomeNavPageViewModel("Avalonia Repo", new Uri("https://www.github.com/AvaloniaUI/Avalonia"))
            {
                ImageUri = "avares://FAControlsGallery/Assets/Images/AvGithub.png"
            },
            new HomeNavPageViewModel("Fluent Design", new Uri("https://learn.microsoft.com/en-us/windows/apps/design/"))
            {
                ImageUri = "avares://FAControlsGallery/Assets/Images/FluentDesign.png"
            }
        };

        RecentItems = new AvaloniaList<RecentItemViewModel>();
        //RefreshRecentItems();

        Favorites = new AvaloniaList<RecentItemViewModel>();
        //RefreshFavorites();
    }

    public List<HomeNavPageViewModel> Pages { get; set; }

    public bool HasRecentItems { get; private set; }

    public AvaloniaList<RecentItemViewModel> RecentItems { get; }

    public bool HasFavorites { get; private set; }

    public AvaloniaList<RecentItemViewModel> Favorites { get; }

    public string CurrentVersion =>
        typeof(FANavigationView).Assembly.GetName().Version?.ToString();

    public async void RefreshRecentItems()
    {
        var recentItems = await RecentFavoriteService.Instance.GetRecentItems();
        RecentItems.Clear();

        if (recentItems != null && recentItems.Count > 0)
        {
            using var recents = new PooledList<RecentItemViewModel>(recentItems.Count);
            foreach (var item in recentItems)
            {
                var page = FindPageForRecentItem(item.AsSpan().Trim());

                if (page == null)
                    continue;

                if (page is PageBaseViewModel pbvm)
                {
                    recents.Add(new RecentItemViewModel
                    {
                        Header = pbvm.Header,
                        Description = page is FAControlsPageItem ? pbvm.Description : null,
                        IconResourceKey = pbvm.IconResourceKey,
                        Page = pbvm
                    });
                }
                else if (page is DesignPageViewModel dpvm)
                {
                    recents.Add(new RecentItemViewModel
                    {
                        Header = item,
                        IconResourceKey = "TextPageIcon",
                        Page = dpvm
                    });
                }
            }

            RecentItems.AddRange(recents);
        }
        
        HasRecentItems = RecentItems.Count > 0;
        RaisePropertyChanged(nameof(HasRecentItems));
    }

    public async void RefreshFavorites()
    {
        var favs = await RecentFavoriteService.Instance.GetFavoriteItems();
        Favorites.Clear();

        if (favs != null && favs.Count > 0)
        {
            using var favItems = new PooledList<RecentItemViewModel>(favs.Count);
            foreach (var item in favs)
            {
                var page = FindPageForRecentItem(item.AsSpan().Trim());

                if (page == null)
                    continue;

                if (page is PageBaseViewModel pbvm)
                {
                    favItems.Add(new RecentItemViewModel
                    {
                        Header = pbvm.Header,
                        Description = page is FAControlsPageItem ? pbvm.Description : null,
                        IconResourceKey = pbvm.IconResourceKey,
                        Page = pbvm
                    });
                }
                else if (page is DesignPageViewModel dpvm)
                {
                    favItems.Add(new RecentItemViewModel
                    {
                        Header = item,
                        IconResourceKey = "",
                        Page = dpvm
                    });
                }
            }

            Favorites.AddRange(favItems);
        }

        HasFavorites = Favorites.Count > 0;
        RaisePropertyChanged(nameof(HasFavorites));
    }

    private ViewModelBase FindPageForRecentItem(ReadOnlySpan<char> header)
    {
        var fa = ControlInformation.GetFAControlInfo();
        foreach (var group in fa)
        {
            foreach (var item in group.Controls)
            {
                if (item.Header.AsSpan().SequenceEqual(header))
                {
                    return item;
                }
            }
        }

        var cc = ControlInformation.GetCoreControlInfo();
        foreach (var item in cc)
        {
            if (item.Header.AsSpan().SequenceEqual(header))
            {
                return item;
            }
        }

        var designPage = Parent.MainNavPages
            .FirstOrDefault(x => x is DesignPageViewModel) as DesignPageViewModel;
        var typo = "Typography".AsSpan();
        var ico = "Icons".AsSpan();
        var col = "Colors".AsSpan();

        if (header.SequenceEqual(typo))
        {
            return designPage;
        }
        else if (header.SequenceEqual(ico))
        {
            return designPage;
        }
        else if (header.SequenceEqual(col))
        {
            return designPage;
        }
                
        return null;
    }
}

public sealed class RecentItemViewModel : ViewModelBase
{
    public string Header { get; set; }

    public string Description { get; set; }

    public string IconResourceKey { get; set; }

    public ViewModelBase Page { get; set; }
}
