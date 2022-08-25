using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentAvaloniaSamples.Pages;

namespace FluentAvaloniaSamples.Controls
{
    public class FAControlsPageHeader : TemplatedControl
    {
        public static readonly StyledProperty<Type> TargetTypeProperty =
            ControlExample.TargetTypeProperty.AddOwner<FAControlsPageBase>();

        public static readonly StyledProperty<string> DescriptionProperty =
            FAControlsPageBase.DescriptionProperty.AddOwner<FAControlsPageHeader>();

        public static readonly StyledProperty<IImage> PreviewImageProperty =
            FAControlsPageBase.PreviewImageProperty.AddOwner<FAControlsPageHeader>();

        public static readonly StyledProperty<string> WinUINamespaceProperty =
            FAControlsPageBase.WinUINamespaceProperty.AddOwner<FAControlsPageHeader>();

        public static readonly StyledProperty<Uri> WinUIDocsLinkProperty =
            FAControlsPageBase.WinUIDocsLinkProperty.AddOwner<FAControlsPageHeader>();

        public static readonly StyledProperty<Uri> WinUIGuidelinesLinkProperty =
            FAControlsPageBase.WinUIGuidelinesLinkProperty.AddOwner<FAControlsPageHeader>();

        public static readonly StyledProperty<Uri> PageXamlSourceLinkProperty =
            FAControlsPageBase.PageXamlSourceLinkProperty.AddOwner<FAControlsPageHeader>();

        public static readonly StyledProperty<Uri> PageCSharpSourceLinkProperty =
            FAControlsPageBase.PageCSharpSourceLinkProperty.AddOwner<FAControlsPageHeader>();

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

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            if (_toggleThemeButton != null)
                _toggleThemeButton.Click -= OnToggleThemeButtonClick;

            if (_showDefinitionButton != null)
                _showDefinitionButton.Click -= OnShowDefinitionButtonClick;

            base.OnApplyTemplate(e);

            _owningPage = this.FindAncestorOfType<FAControlsPageBase>();

            _toggleThemeButton = e.NameScope.Find<Button>("ChangeThemeButton");
            _showDefinitionButton = e.NameScope.Find<Button>("ShowDefinitionButton");

            _toggleThemeButton.Click += OnToggleThemeButtonClick;
            _showDefinitionButton.Click += OnShowDefinitionButtonClick;

            _descriptionTextBlock = e.NameScope.Find<TextBlock>("DescriptionTextBlock");

            _winUIPanel = e.NameScope.Find<Control>("WinUIRelationHost");
            _linksPanel = e.NameScope.Find<Control>("SamplePageLinksHost");
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var size = base.MeasureOverride(availableSize);

            if (_descriptionTextBlock != null && _descriptionTextBlock.TextLayout != null)
            {
                var lines = _descriptionTextBlock.TextLayout.TextLines;
                if (lines.Count == _descriptionTextBlock.MaxLines && lines[^1].HasCollapsed)
                {
                    ToolTip.SetTip(_descriptionTextBlock, Description);
                }
            }

            return size;
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
                if (_suppressAnimation)
                {
                    _suppressAnimation = false;
                }
            }
        }

        private void OnShowDefinitionButtonClick(object sender, RoutedEventArgs e)
        {
            _owningPage.ShowPageControlDefinition();
        }

        private void OnToggleThemeButtonClick(object sender, RoutedEventArgs e)
        {
            _owningPage.TogglePageTheme();
        }

        internal void GoToHeightState(int state)
        {
            PseudoClasses.Set("smallHeight", state >= 1);
            PseudoClasses.Set("smallestHeight", state == 2);
        }

        internal void GoToWidthState(int state, bool animate = false)
        {
            PseudoClasses.Set("smallWidth", state == 1);
            PseudoClasses.Set("smallestWidth", state == 2);

            if (animate)
            {
                if (state == 1)
                {
                    AnimateHeaderElementsToSmallWidth();
                }
                else
                {
                    AnimateHeaderElementsFromSmallWidth();
                }
            }
        }

        private async void AnimateHeaderElementsToSmallWidth()
        {
            if (_winUIPanel == null || _linksPanel == null)
                return;

            if (_cts != null)
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
                _winUIPanel.Opacity = 1;
                _winUIPanel.RenderTransform = new TranslateTransform();
                _linksPanel.RenderTransform = new TranslateTransform();
                _linksPanel.Opacity = 1;
            }

            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(OpacityProperty, 0d),
                            new Setter(TranslateTransform.XProperty, -250d),
                            new Setter(TranslateTransform.YProperty, 150d)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters =
                        {
                            new Setter(OpacityProperty, 1d),
                            new Setter(TranslateTransform.XProperty, 0d),
                            new Setter(TranslateTransform.YProperty, 0d)
                        },
                        KeySpline = new KeySpline(0,0,0, 1)
                    },
                }
            };

            _cts = new CancellationTokenSource();

            await Task.WhenAll(ani.RunAsync(_winUIPanel, null, _cts.Token), ani.RunAsync(_linksPanel, null, _cts.Token));

            _cts.Dispose();
            _cts = null;
        }

        private async void AnimateHeaderElementsFromSmallWidth()
        {
            if (_winUIPanel == null || _linksPanel == null)
                return;

            if (_cts != null)
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
                _winUIPanel.Opacity = 1;
                _winUIPanel.RenderTransform = new TranslateTransform();
                _linksPanel.RenderTransform = new TranslateTransform();
                _linksPanel.Opacity = 1;
            }

            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(OpacityProperty, 0d),
                            new Setter(TranslateTransform.XProperty, 150d),
                            new Setter(TranslateTransform.YProperty, -250d)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters =
                        {
                            new Setter(OpacityProperty, 1d),
                            new Setter(TranslateTransform.XProperty, 0d),
                            new Setter(TranslateTransform.YProperty, 0d)
                        },
                        KeySpline = new KeySpline(0,0,0, 1)
                    },
                }
            };

            _cts = new CancellationTokenSource();

            await Task.WhenAll(ani.RunAsync(_winUIPanel, null, _cts.Token), ani.RunAsync(_linksPanel, null, _cts.Token));

            _cts.Dispose();
            _cts = null;
        }


        private CancellationTokenSource _cts;

        private bool _suppressAnimation = true;
        private Button _toggleThemeButton;
        private Button _showDefinitionButton;
        private TextBlock _descriptionTextBlock;
        private FAControlsPageBase _owningPage;

        private Control _winUIPanel;
        private Control _linksPanel;
    }
}
