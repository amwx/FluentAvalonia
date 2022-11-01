using System;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using FluentAvalonia.Styling;
using System.Text.Json;

namespace FluentAvaloniaSamples.ViewModels;

public class MainViewViewModel : ViewModelBase
{
    public MainViewViewModel()
    {
        var faTheme = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
        faTheme.RequestedThemeChanged += OnAppThemeChanged;

        _currentAppTheme = faTheme.RequestedTheme;

        if (faTheme.TryGetResource("SystemAccentColor", out var value))
        {
            _customAccentColor = (Color)value;
            _listBoxColor = _customAccentColor;
        }

        GetPredefColors();

        GetSearchTerms();
    }


    public string[] AppThemes { get; } =
        new[] { FluentAvaloniaTheme.LightModeString, FluentAvaloniaTheme.DarkModeString, FluentAvaloniaTheme.HighContrastModeString };

    public FlowDirection[] AppFlowDirections { get; } =
        new[] { FlowDirection.LeftToRight, FlowDirection.RightToLeft };

    public string CurrentAppTheme
    {
        get => _currentAppTheme;
        set
        {
            if (RaiseAndSetIfChanged(ref _currentAppTheme, value))
            {
                var faTheme = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
                faTheme.RequestedTheme = value;
            }
        }
    }

    public FlowDirection CurrentFlowDirection
    {
        get => _currentFlowDirection;
        set
        {
            if (RaiseAndSetIfChanged(ref _currentFlowDirection, value))
            {
                var mainWindow = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;
                mainWindow.FlowDirection = value;
            }
        }
    }

    public bool UseCustomAccent
    {
        get => _useCustomAccentColor;
        set
        {
            if (RaiseAndSetIfChanged(ref _useCustomAccentColor, value))
            {
                if (value)
                {
                    var faTheme = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
                    if (faTheme.TryGetResource("SystemAccentColor", out var curColor))
                    {
                        _customAccentColor = (Color)curColor;
                        _listBoxColor = _customAccentColor;

                        RaisePropertyChanged(nameof(CustomAccentColor));
                        RaisePropertyChanged(nameof(ListBoxColor));
                    }

                    AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().CustomAccentColor = CustomAccentColor;
                }
                else
                {
                    CustomAccentColor = default;
                    AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().CustomAccentColor = null;
                }
            }
        }
    }

    public Color ListBoxColor
    {
        get => _listBoxColor;
        set
        {
            RaiseAndSetIfChanged(ref _listBoxColor, value);

            if (!_ignoreSetListBoxColor)
                CustomAccentColor = value;
        }
    }

    public Color CustomAccentColor
    {
        get => _customAccentColor;
        set
        {
            if (RaiseAndSetIfChanged(ref _customAccentColor, value))
            {
                AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().CustomAccentColor = value;

                _ignoreSetListBoxColor = true;
                ListBoxColor = value;
                _ignoreSetListBoxColor = false;
            }
        }
    }

    public List<Color> PredefinedColors { get; private set; }

    public string CurrentVersion
    {
        get
        {
            return typeof(FluentAvalonia.UI.Controls.NavigationView).Assembly.GetName().Version?.ToString();
        }
    }

    public string CurrentAvaloniaVersion
    {
        get
        {
            return typeof(Avalonia.Application).Assembly.GetName().Version?.ToString();
        }
    }

    public List<MainAppSearchItem> MainSearchItems { get; private set; }

    private void OnAppThemeChanged(FluentAvaloniaTheme sender, RequestedThemeChangedEventArgs args)
    {
        if (_currentAppTheme != args.NewTheme)
        {
            _currentAppTheme = args.NewTheme;
            RaisePropertyChanged(nameof(CurrentAppTheme));
        }
    }

    private void GetPredefColors()
    {
        PredefinedColors = new List<Color>
        {
            Color.FromRgb(255,185,0),
            Color.FromRgb(255,140,0),
            Color.FromRgb(247,99,12),
            Color.FromRgb(202,80,16),
            Color.FromRgb(218,59,1),
            Color.FromRgb(239,105,80),
            Color.FromRgb(209,52,56),
            Color.FromRgb(255,67,67),
            Color.FromRgb(231,72,86),
            Color.FromRgb(232,17,35),
            Color.FromRgb(234,0,94),
            Color.FromRgb(195,0,82),
            Color.FromRgb(227,0,140),
            Color.FromRgb(191,0,119),
            Color.FromRgb(194,57,179),
            Color.FromRgb(154,0,137),
            Color.FromRgb(0,120,212),
            Color.FromRgb(0,99,177),
            Color.FromRgb(142,140,216),
            Color.FromRgb(107,105,214),
            Color.FromRgb(135,100,184),
            Color.FromRgb(116,77,169),
            Color.FromRgb(177,70,194),
            Color.FromRgb(136,23,152),
            Color.FromRgb(0,153,188),
            Color.FromRgb(45,125,154),
            Color.FromRgb(0,183,195),
            Color.FromRgb(3,131,135),
            Color.FromRgb(0,178,148),
            Color.FromRgb(1,133,116),
            Color.FromRgb(0,204,106),
            Color.FromRgb(16,137,62),
            Color.FromRgb(122,117,116),
            Color.FromRgb(93,90,88),
            Color.FromRgb(104,118,138),
            Color.FromRgb(81,92,107),
            Color.FromRgb(86,124,115),
            Color.FromRgb(72,104,96),
            Color.FromRgb(73,130,5),
            Color.FromRgb(16,124,16),
            Color.FromRgb(118,118,118),
            Color.FromRgb(76,74,72),
            Color.FromRgb(105,121,126),
            Color.FromRgb(74,84,89),
            Color.FromRgb(100,124,100),
            Color.FromRgb(82,94,84),
            Color.FromRgb(132,117,69),
            Color.FromRgb(126,115,95)
        };
    }

    private void GetSearchTerms()
    {
        MainSearchItems = new List<MainAppSearchItem>();

        Type TryResolveAvaloniaType(string type)
        {
            return Type.GetType($"Avalonia.Controls.{type}") ??
                Type.GetType($"Avalonia.Controls.Primitives.{type}");
        }

        var file = GetAssemblyResource("avares://FluentAvaloniaSamples/Assets/CoreControlsGroups.json");
        var coreControls = JsonSerializer.Deserialize<List<CoreControlsGroupItem>>(file);
        for (int i = 1; i < coreControls.Count; i++)
        {
            var desc = coreControls[i].Description.Split(',');

            foreach (var item in desc)
            {
                MainSearchItems.Add(new MainAppSearchItem(item.Trim(), Type.GetType($"FluentAvaloniaSamples.Pages.{coreControls[i].PageType}")));
            }
        }

        // Get all FluentAvalonia pages
        file = GetAssemblyResource("avares://FluentAvaloniaSamples/Assets/FAControlsGroups.json");
        var controls = JsonSerializer.Deserialize<List<FAControlsGroupItem>>(file);
        foreach (var group in controls)
        {
            foreach (var ctrl in group.Controls)
            {
                // Differentiate between the Avalonia MenuFlyout and my own
                if (ctrl.Header == "MenuFlyout")
                {
                    MainSearchItems.Add(new MainAppSearchItem($"{ctrl.Header} (FluentAvalonia)", Type.GetType($"FluentAvaloniaSamples.Pages.{ctrl.PageType}")));
                }
                else
                {
                    MainSearchItems.Add(new MainAppSearchItem(ctrl.Header, Type.GetType($"FluentAvaloniaSamples.Pages.{ctrl.PageType}")));
                }

            }
        }

    }


    private bool _useCustomAccentColor;
    private Color _customAccentColor = Colors.SlateBlue;
    private string _currentAppTheme;
    private FlowDirection _currentFlowDirection;
    private Color _listBoxColor;
    private bool _ignoreSetListBoxColor = false;
}

public class MainAppSearchItem
{
    public MainAppSearchItem(string pageHeader, Type pageType)
    {
        Header = pageHeader;
        PageType = pageType;
    }

    public string Header { get; set; }

    public Type PageType { get; set; }
}
