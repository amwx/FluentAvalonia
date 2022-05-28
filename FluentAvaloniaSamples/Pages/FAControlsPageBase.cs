using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.Controls;
using FluentAvaloniaSamples.Services;

namespace FluentAvaloniaSamples.Pages
{
    public class FAControlsPageBase : UserControl, IStyleable
    {
        public FAControlsPageBase()
        {
            PageXamlSourceLink = new Uri($"{GithubPrefixString}/{GetType().Name}.axaml");
            PageCSharpSourceLink = new Uri($"{GithubPrefixString}/{GetType().Name}.cs");
        }

        public static readonly StyledProperty<Type> TargetTypeProperty =
            ControlExample.TargetTypeProperty.AddOwner<FAControlsPageBase>();

        public static readonly StyledProperty<string> DescriptionProperty =
            AvaloniaProperty.Register<FAControlsPageBase, string>(nameof(Description));

        public static readonly StyledProperty<IImage> PreviewImageProperty =
            AvaloniaProperty.Register<FAControlsPageBase, IImage>(nameof(PreviewImage));

        public static readonly StyledProperty<string> WinUINamespaceProperty =
            AvaloniaProperty.Register<FAControlsPageBase, string>(nameof(WinUINamespace));

        public static readonly StyledProperty<Uri> WinUIDocsLinkProperty =
            AvaloniaProperty.Register<FAControlsPageBase, Uri>(nameof(WinUIDocsLink));

        public static readonly StyledProperty<Uri> WinUIGuidelinesLinkProperty =
            AvaloniaProperty.Register<FAControlsPageBase, Uri>(nameof(WinUIGuidelinesLink));

        public static readonly StyledProperty<Uri> PageXamlSourceLinkProperty =
            AvaloniaProperty.Register<FAControlsPageBase, Uri>(nameof(PageXamlSourceLink));

        public static readonly StyledProperty<Uri> PageCSharpSourceLinkProperty =
            AvaloniaProperty.Register<FAControlsPageBase, Uri>(nameof(PageCSharpSourceLink));

        public Type TargetType
        {
            get => GetValue(TargetTypeProperty);
            set => SetValue(TargetTypeProperty, value);
        }

        public string Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public IImage PreviewImage
        {
            get => GetValue(PreviewImageProperty);
            set => SetValue(PreviewImageProperty, value);
        }

        public string WinUINamespace
        {
            get => GetValue(WinUINamespaceProperty);
            set => SetValue(WinUINamespaceProperty, value);
        }

        public Uri WinUIDocsLink
        {
            get => GetValue(WinUIDocsLinkProperty);
            set => SetValue(WinUIDocsLinkProperty, value);
        }

        public Uri WinUIGuidelinesLink
        {
            get => GetValue(WinUIGuidelinesLinkProperty);
            set => SetValue(WinUIGuidelinesLinkProperty, value);
        }

        public Uri PageXamlSourceLink
        {
            get => GetValue(PageXamlSourceLinkProperty);
            set => SetValue(PageXamlSourceLinkProperty, value);
        }

        public Uri PageCSharpSourceLink
        {
            get => GetValue(PageCSharpSourceLinkProperty);
            set => SetValue(PageCSharpSourceLinkProperty, value);
        }

        public static string GithubPrefixString => "https://github.com/amwx/FluentAvalonia/tree/master/FluentAvaloniaSamples/Pages";

        Type IStyleable.StyleKey => typeof(FAControlsPageBase);

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _rootGrid = e.NameScope.Get<Grid>("RootGrid");
            _contentRoot = e.NameScope.Get<StackPanel>("ContentRoot");
            _headerElement = e.NameScope.Find<FAControlsPageHeader>("HeaderElement");
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == WinUINamespaceProperty)
            {
                PseudoClasses.Set(":winui", WinUINamespace != null);
            }
            else if (change.Property == WinUIDocsLinkProperty)
            {
                PseudoClasses.Set(":doclink", WinUIDocsLink != null);
            }
            else if (change.Property == WinUIGuidelinesLinkProperty)
            {
                PseudoClasses.Set(":guidelink", WinUIGuidelinesLink != null);
            }
            else if (change.Property == PageXamlSourceLinkProperty)
            {
                var value = change.GetNewValue<Uri>();
                PseudoClasses.Set(":pagesrc", value != null || PageCSharpSourceLink != null);
                PseudoClasses.Set(":xamlsrc", value != null);
            }
            else if (change.Property == PageCSharpSourceLinkProperty)
            {
                var value = change.GetNewValue<Uri>();
                PseudoClasses.Set(":pagesrc", value != null || PageXamlSourceLink != null);
                PseudoClasses.Set(":csharpsrc", value != null);
            }
            else if (change.Property == BoundsProperty)
            {
                UpdateState(_hasPageInitialized);
                _hasPageInitialized = true;
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e); 
            if (_pageTheme != null)
            {
                var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
                if (thm.RequestedTheme == _pageTheme)
                {
                    SetThemeOnExamples(_pageTheme == FluentAvaloniaTheme.LightModeString);
                    Resources.MergedDictionaries.Clear();
                    Background = Brushes.Transparent;
                    _pageTheme = null;
                }
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _hasPageInitialized = false;
            _adaptiveWidthStage = -1;
        }

        internal void ShowPageControlDefinition()
        {
            NavigationService.Instance.ShowControlDefinitionOverlay(TargetType);
        }

        internal void TogglePageTheme()
        {
            var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();

            if (_pageTheme == null)
            {
                if (thm.RequestedTheme == FluentAvaloniaTheme.LightModeString)
                {
                    _pageTheme = FluentAvaloniaTheme.DarkModeString;
                }
                else if (thm.RequestedTheme == FluentAvaloniaTheme.DarkModeString)
                {
                    _pageTheme = FluentAvaloniaTheme.LightModeString;
                }

                // Will still be null if using HighContrast theme
                if (_pageTheme != null)
                {
                    Resources.MergedDictionaries.Add(
                        (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV2/{_pageTheme}Resources.axaml"))
                    );
                    SetThemeOnExamples(_pageTheme == FluentAvaloniaTheme.LightModeString);
                    // To ensure all still looks ok (primarily because of the transparency effects on the window),
                    // if we toggle the theme here, the page background needs to go opaque so all controls look
                    // ok, since many of the control colors are semi-transparent.
                    if (this.TryFindResource("ApplicationPageBackgroundThemeBrush", out var value))
                    {
                        if (value is ISolidColorBrush s)
                        {
                            Background = s;
                        }
                    }
                }
            }
            else if (_pageTheme == FluentAvaloniaTheme.LightModeString)
            {
                // If _pageTheme was set to light, it means app is in dark mode, just remove the resources here
                _pageTheme = null;
                Resources.MergedDictionaries.Clear();
                Background = Brushes.Transparent;
                SetThemeOnExamples(false);
            }
            else if (_pageTheme == FluentAvaloniaTheme.DarkModeString)
            {
                // If _pageTheme was set to dark, it means app in in light mode, just remove the resource here
                _pageTheme = null;
                Resources.MergedDictionaries.Clear();
                Background = Brushes.Transparent;
                SetThemeOnExamples(true);
            }            
        }

        private void SetThemeOnExamples(bool isLightMode)
        {
            // While the control examples themselves will inherit the resource changes,
            // the text editors for code samples won't since we're not changing the theme
            // at the app level. So notify them they should switch modes
            foreach (var item in this.GetVisualDescendants().OfType<ControlExample>())
            {
                item.SetExampleTheme(isLightMode);
            }
        }

        private void UpdateState(bool animate)
        {
            var size = Bounds.Size;

            // Width has precedence over height adaptive behavior
            if (size.Width >= 1110)
            {
                if (_adaptiveWidthStage != 0)
                {
                    _headerElement.GoToWidthState(0, animate);
                    _adaptiveWidthStage = 0;

                    MoveHeaderToRootGrid();
                }

                if (size.Height >= 525 && size.Height < 665)
                {
                    if (_adaptiveHeightStage == 1)
                        return;

                    _adaptiveHeightStage = 1;
                    _headerElement.GoToHeightState(1);
                }
                else if (size.Height < 525)
                {
                    if (_adaptiveHeightStage == 2)
                        return;

                    _adaptiveHeightStage = 2;
                    _headerElement.GoToHeightState(2);
                }
                else
                {
                    if (_adaptiveHeightStage == 0)
                        return;

                    _adaptiveHeightStage = 0;
                    _headerElement.GoToHeightState(0);
                }
            }
            else
            {
                if (_adaptiveHeightStage != 0)
                {
                    _headerElement.GoToHeightState(0);
                    _adaptiveHeightStage = 0;
                }
                
                if ((size.Width >= 800 && size.Height < 1110))
                {
                    if (_adaptiveWidthStage == 1)
                        return;

                    MoveHeaderToContentRoot();
                    _adaptiveWidthStage = 1;
                    _headerElement.GoToWidthState(1, animate);
                }
                else if (size.Width < 800 && _adaptiveWidthStage != 2)
                {
                    if (_adaptiveWidthStage == 2)
                        return;

                    MoveHeaderToContentRoot();
                    _adaptiveWidthStage = 2;
                    _headerElement.GoToWidthState(2, animate);
                }
            }
        }

        private void MoveHeaderToRootGrid()
        {
            if (_headerElement.Parent == _rootGrid)
                return;

            _contentRoot.Children.Remove(_headerElement);

            _rootGrid.Children.Insert(0, _headerElement);
        }

        private void MoveHeaderToContentRoot()
        {
            if (_headerElement.Parent == _contentRoot)
                return;

            _rootGrid.Children.Remove(_headerElement);

            _contentRoot.Children.Insert(0, _headerElement);
        }


        private string _pageTheme = null;
        private StackPanel _contentRoot;
        private Grid _rootGrid;
        private FAControlsPageHeader _headerElement;
        private bool _hasPageInitialized = false;

        // HEIGHT
        // -1, unset
        // 0, Normal
        // 1, SmallHeight
        // 2, SmallestHeight
        private int _adaptiveHeightStage = -1;

        // WIDTH
        // -1, unset
        // 0, Normal
        // 1, SmallWidth
        // 2, SmallestWidth
        private int _adaptiveWidthStage = -1;
    }
}
