using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using Avalonia.Media.Immutable;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using System.Diagnostics;
using FAControlsGallery.Services;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using AvaloniaEdit;
using TextMateSharp.Grammars;
using AvaloniaEdit.TextMate;
using AvaloniaEdit.Document;

namespace FAControlsGallery.Controls;

public class ControlExample : HeaderedContentControl
{
    public ControlExample()
    {
        _substitutions = new List<ControlExampleSubstitution>();
        _flyoutOptions = new List<MenuFlyoutItemBase>();

        //ResourcesChanged += OnResourcesChanged;
        PseudoClasses.Add(":optionsfull");
    }

    public static readonly StyledProperty<Type> TargetTypeProperty =
        AvaloniaProperty.Register<ControlExample, Type>(nameof(TargetType));

    public static readonly StyledProperty<string> XamlSourceProperty =
        AvaloniaProperty.Register<ControlExample, string>(nameof(XamlSource));

    public static readonly StyledProperty<string> CSharpSourceProperty =
        AvaloniaProperty.Register<ControlExample, string>(nameof(CSharpSource));

    public static readonly StyledProperty<string> UsageNotesProperty =
        AvaloniaProperty.Register<ControlExample, string>(nameof(UsageNotes));

    public static readonly StyledProperty<Control> OptionsProperty =
        AvaloniaProperty.Register<ControlExample, Control>(nameof(Options));

    public static readonly DirectProperty<ControlExample, IList<ControlExampleSubstitution>> SubstitutionsProperty =
        AvaloniaProperty.RegisterDirect<ControlExample, IList<ControlExampleSubstitution>>(nameof(Substitutions),
            x => x.Substitutions);

    public static readonly StyledProperty<bool> IsOptionsExpandedProperty =
        AvaloniaProperty.Register<ControlExample, bool>(nameof(IsOptionsExpanded), true);

    public Type TargetType
    {
        get => GetValue(TargetTypeProperty);
        set => SetValue(TargetTypeProperty, value);
    }

    public string XamlSource
    {
        get => GetValue(XamlSourceProperty);
        set => SetValue(XamlSourceProperty, value);
    }

    public string CSharpSource
    {
        get => GetValue(CSharpSourceProperty);
        set => SetValue(CSharpSourceProperty, value);
    }

    public Control Options
    {
        get => GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public string UsageNotes
    {
        get => GetValue(UsageNotesProperty);
        set => SetValue(UsageNotesProperty, value);
    }

    public IList<ControlExampleSubstitution> Substitutions
    {
        get => _substitutions;
    }

    public bool IsOptionsExpanded
    {
        get => GetValue(IsOptionsExpandedProperty);
        set => SetValue(IsOptionsExpandedProperty, value);
    }

    public bool EnableShowDocsLink { get; set; }

    public bool EnableShowDefinitionLink { get; set; }

    public string DocsLinkHeader { get; set; } = "Avalonia Docs";

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_expandOptionsButton != null)
            _expandOptionsButton.Click -= OnExpandOptionsClick;

        base.OnApplyTemplate(e);

        _exampleThemeScopeProvider = e.NameScope.Find<ThemeVariantScope>("ThemeScopeProvider");

        _expandOptionsButton = e.NameScope.Find<Button>("ShowHideOptionsButton");
        _expandOptionsButton.Click += OnExpandOptionsClick;

        _previewAreaHost = e.NameScope.Find<Border>("ControlPreviewAreaHost");

        _moreButton = e.NameScope.Find<Button>("MoreButton");
        if (_moreButton != null)
        {
            BuildMoreButtonMenu();
        }        

        bool hasXaml = XamlSource != null;
        bool hasCSharp = CSharpSource != null;
        PseudoClasses.Set(":codepreview", hasXaml || hasCSharp || UsageNotes != null);

        PseudoClasses.Set(":xamlsource", hasXaml);
        PseudoClasses.Set(":csharpsource", hasCSharp);
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();

        // Do this here rather than OnApplyTemplate, otherwise this will animate
        // on load and that isn't desired
        AttachOptionsHostAnimation();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OptionsProperty)
        {
            PseudoClasses.Set(":options", change.NewValue != null);
        }
        else if (change.Property == BoundsProperty)
        {
            var wid = change.GetNewValue<Rect>().Width;

            PseudoClasses.Set(":mediumWidth", wid < 725);
            PseudoClasses.Set(":smallWidth", wid < 500);
        }
        else if (change.Property == IsOptionsExpandedProperty)
        {
            PseudoClasses.Set(":optionsfull", change.GetNewValue<bool>());
        }
    }

    private void OnExpandOptionsClick(object sender, RoutedEventArgs e)
    {
        IsOptionsExpanded = !IsOptionsExpanded;
    }

    private void BuildMoreButtonMenu()
    {
        if (!EnableShowDefinitionLink && !EnableShowDocsLink)
        {
            _moreButton.IsVisible = false;
        }
        else
        {
            var l = new List<MenuFlyoutItem>();

            if (EnableShowDocsLink)
            {
                if (_availableAvaloniaDocs.Count == 0)
                    CreateDocsDictionary();

                // Only add if a docs link actually exists
                if (_availableAvaloniaDocs.Contains(TargetType.Name))
                {
                    var docsItem = new MenuFlyoutItem
                    {
                        Text = DocsLinkHeader,
                        IconSource = new SymbolIconSource { Symbol = Symbol.Link }
                    };

                    docsItem.Click += LaunchAvaloniaDocs;

                    l.Add(docsItem);
                }
            }

            if (EnableShowDefinitionLink)
            {
                var defItem = new MenuFlyoutItem
                {
                    Text = "Show Definition",
                    IconSource = new SymbolIconSource { Symbol = Symbol.CodeFilled }
                };

                defItem.Click += ShowControlDefintion;

                l.Add(defItem);
            }

            _moreButton.Flyout = new FAMenuFlyout
            {
                Items = l,
                Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft
            };
        }
    }

    private void AttachOptionsHostAnimation()
    {
        if (_optionsHost == null)
            return;

        var host = _optionsHost;
        var ec = ElementComposition.GetElementVisual(host);

        if (_optionsHostAnimation == null)
        {
            var comp = ec.Compositor;

            var offsetAni = comp.CreateVector3KeyFrameAnimation();
            offsetAni.InsertExpressionKeyFrame(1f, "this.FinalValue");
            offsetAni.Target = "Offset";
            offsetAni.Duration = TimeSpan.FromMilliseconds(250);

            var scaleAni = comp.CreateVector3KeyFrameAnimation();
            scaleAni.InsertExpressionKeyFrame(1f, "this.FinalValue");
            scaleAni.Target = "Scale";
            scaleAni.Duration = offsetAni.Duration;

            var group = comp.CreateAnimationGroup();
            group.Add(offsetAni);
            group.Add(scaleAni);

            _optionsHostAnimation = comp.CreateImplicitAnimationCollection();
            _optionsHostAnimation["Offset"] = group;
        }

        ec.ImplicitAnimations = _optionsHostAnimation;
    }

    public void SetExampleTheme()
    {
        var theme = _exampleThemeScopeProvider.ActualThemeVariant;

        if (theme == ThemeVariant.Light)
        {
            _exampleThemeScopeProvider.RequestedThemeVariant = ThemeVariant.Dark;
            // Hack force resource invalidation as that doesn't seem to want to happen
            // the first time toggle theme is set
            NotifyChildResourcesChanged(ResourcesChangedEventArgs.Empty);
        }
        else
        {
            _exampleThemeScopeProvider.RequestedThemeVariant = ThemeVariant.Light;
            NotifyChildResourcesChanged(ResourcesChangedEventArgs.Empty);
        }

        //if (_cSharpTextEditor != null)
        //{
        //    _xamlTextEditor.SyntaxHighlighting = isLightMode ? XamlHighlightingSource.LightModeXaml : XamlHighlightingSource.DarkModeXaml;
        //    _cSharpTextEditor.SyntaxHighlighting = isLightMode ? CSharpHighlightingSource.CSharpLightMode : CSharpHighlightingSource.CSharpDarkMode;

        //    if (this.TryFindResource("TextControlSelectionHighlightColor", out var value))
        //    {
        //        if (value is ISolidColorBrush sb)
        //        {
        //            var b = new ImmutableSolidColorBrush(sb.Color, 0.5);
        //            //_xamlTextEditor.TextArea.SelectionBrush = b;
        //            //_cSharpTextEditor.TextArea.SelectionBrush = b;
        //        }
        //    }
        //}
    }

    public void LaunchAvaloniaDocs(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo($"https://docs.avaloniaui.net/docs/controls/{TargetType.Name.ToLower()}") { UseShellExecute = true, Verb = "open" });
        }
        catch
        {
            // TODO: Display dialog indicating failure to launch browser
        }
    }

    public void ShowControlDefintion(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.ShowControlDefinitionOverlay(TargetType);
    }

    private void CreateDocsDictionary()
    {
        // Not all controls are included here, only controls I have examples for, since I don't include
        // anything that really doesn't look different with the Fluent v2 styles (panels, carousel, etc)
        _availableAvaloniaDocs.Add("AutoCompleteBox");
        _availableAvaloniaDocs.Add("Button");
        _availableAvaloniaDocs.Add("ButtonSpinner");
        _availableAvaloniaDocs.Add("Calendar");
        _availableAvaloniaDocs.Add("CheckBox");
        _availableAvaloniaDocs.Add("ComboBox");
        _availableAvaloniaDocs.Add("DataGrid");
        _availableAvaloniaDocs.Add("DatePicker");
        _availableAvaloniaDocs.Add("Expander");
        _availableAvaloniaDocs.Add("GridSplitter");
        _availableAvaloniaDocs.Add("ListBox");
        _availableAvaloniaDocs.Add("Menu");
        _availableAvaloniaDocs.Add("NumericUpDown");
        _availableAvaloniaDocs.Add("ProgressBar");
        _availableAvaloniaDocs.Add("RadioButton");
        _availableAvaloniaDocs.Add("RepeatButton");
        _availableAvaloniaDocs.Add("ScrollBar");
        _availableAvaloniaDocs.Add("ScrollViewer");
        _availableAvaloniaDocs.Add("Slider");
        _availableAvaloniaDocs.Add("SplitView");
        _availableAvaloniaDocs.Add("StackPanel");
        _availableAvaloniaDocs.Add("TabControl");
        _availableAvaloniaDocs.Add("TabStrip");
        _availableAvaloniaDocs.Add("TimePicker");
        _availableAvaloniaDocs.Add("TextBox");
        _availableAvaloniaDocs.Add("ToggleButton");
        _availableAvaloniaDocs.Add("ToolTip");
        _availableAvaloniaDocs.Add("TreeView");
    }


    private IList<ControlExampleSubstitution> _substitutions;

    public static Flyout _copiedNoticeFlyout;
    //private Button _copyXamlButton;
    //private Button _copyCSharpButton;
    private Border _previewAreaHost;
    private Button _expandOptionsButton;
    private ThemeVariantScope _exampleThemeScopeProvider;
    private Border _optionsHost;

    private Button _moreButton;
    //private TextEditor _xamlTextEditor;
    //private TextEditor _cSharpTextEditor;
    private TextBlock _usageNotesTextBlock;

    private IList<MenuFlyoutItemBase> _flyoutOptions;

    private static ImplicitAnimationCollection _optionsHostAnimation;

    private static readonly HashSet<string> _availableAvaloniaDocs = new HashSet<string>();
}

public enum SampleCodePresenterType
{
    XAML,
    CSharp,
    Text
}

public class SampleCodePresenter : HeaderedContentControl
{
    public static readonly StyledProperty<string> CodeProperty =
        AvaloniaProperty.Register<SampleCodePresenter, string>(nameof(Code));

    public static readonly StyledProperty<SampleCodePresenterType> SampleTypeProperty =
        AvaloniaProperty.Register<SampleCodePresenter, SampleCodePresenterType>(nameof(SampleType));

    public static readonly DirectProperty<SampleCodePresenter, IList<ControlExampleSubstitution>> SubstitutionsProperty =
        ControlExample.SubstitutionsProperty.AddOwner<SampleCodePresenter>(x => x.Substitutions, (x,v) => x.Substitutions = v);

    public string Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    public SampleCodePresenterType SampleType
    {
        get => GetValue(SampleTypeProperty);
        set => SetValue(SampleTypeProperty, value);
    }

    public IList<ControlExampleSubstitution> Substitutions
    {
        get => _substitutions;
        set => SetAndRaise(SubstitutionsProperty, ref _substitutions, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_copyCodeButton != null)
            _copyCodeButton.Click -= OnCopyCodeButtonClick;

        base.OnApplyTemplate(e);

        // Don't initialize the TextEditor stuff if we aren't going to use it
        // AvaloniaEdit w/ TextMate is stupid slow
        var code = Code;
        if (string.IsNullOrEmpty(code) || code.Length == 0)
            return;

        _copyCodeButton = e.NameScope.Find<Button>("CopyCodeButton");
        _copyCodeButton.Click += OnCopyCodeButtonClick;

        _textHost = e.NameScope.Find<TextEditor>("TextHost");

        // HACK: Links apparently can't be turned off (if you see this and know how, pls open a PR and fix this =D), so force links
        //       to be the same color as attributes
        _textHost.Options.EnableHyperlinks = false;
        
        //_xamlTextEditor.TextArea.TextView.LinkTextForegroundBrush = new ImmutableSolidColorBrush(Color.FromRgb(255, 160, 122));
        //_xamlTextEditor.TextArea.TextView.LinkTextUnderline = false;


        if (_options == null)
        {
            _options = new RegistryOptions(ThemeName.Light);
            _cSharpLangId = _options.GetScopeByLanguageId(_options.GetLanguageByExtension(".cs").Id);
            _xamlLangId = _options.GetScopeByLanguageId(_options.GetLanguageByExtension(".xaml").Id);
        }

        _textMateInstall = _textHost.InstallTextMate(_options, false);
        
        switch (SampleType)
        {
            case SampleCodePresenterType.CSharp:
                {
                    _textMateInstall.SetGrammar(_cSharpLangId);
                }                
                break;

            case SampleCodePresenterType.XAML:
                {
                    _textMateInstall.SetGrammar(_xamlLangId);
                }
                break;
        }

        if (ActualThemeVariant == ThemeVariant.Dark)
        {
            if (_darkTheme == null)
            {
                _darkTheme = _options.LoadTheme(ThemeName.Dark);
            }
            _textMateInstall.SetTheme(_darkTheme);
        }

        if ((Substitutions != null && Substitutions.Count > 0) && !_hasRegisteredSubstitutions)
        {
            foreach (var sub in Substitutions)
            {
                sub.ValueChanged += OnSubstitutionValueChanged;
            }

            _hasRegisteredSubstitutions = true;
        }

        FormatAndRenderSampleFromString(Code);

        if (this.TryFindResource("TextControlSelectionHighlightColor", out var value))
        {
            if (value is ISolidColorBrush sb)
            {
                var b = new ImmutableSolidColorBrush(sb.Color, 0.5);
                _textHost.TextArea.SelectionBrush = b;
            }
        }
    }

    private async void OnCopyCodeButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_textHost.SelectionLength > 0)
            {
                // Copy the selected text
                _textHost.Copy();
            }
            else
            {
                // Copy everything
                await Application.Current.Clipboard.SetTextAsync(_textHost.Text);
            }


            ShowCopiedFlyout(sender as Button);
        }
        catch
        {
            ShowCopiedFlyout(sender as Button, "Failed to copy code", true);
        }
    }

    private void OnSubstitutionValueChanged(ControlExampleSubstitution sender, object args)
    {
        FormatAndRenderSampleFromString(Code);
    }

    private void FormatAndRenderSampleFromString(string sampleString)
    {
        if (string.IsNullOrEmpty(sampleString))
            return;

        if (Uri.TryCreate(sampleString, UriKind.Absolute, out Uri result))
        {
            using (var s = AvaloniaLocator.Current.GetService<IAssetLoader>().Open(result))
            using (var sr = new StreamReader(s))
            {
                sampleString = sr.ReadToEnd();
            }
        }

        void TrimAndSubstitute()
        {
            // Trim out stray blank lines at start and end.
            sampleString = sampleString.TrimStart('\n').TrimEnd();

            // Also trim out spaces at the end of each line
            sampleString = string.Join('\n', sampleString.Split('\n').Select(s => s.TrimEnd()));

            sampleString = SubstitutionPattern.Replace(sampleString, match =>
            {
                foreach (var substitution in Substitutions)
                {
                    if (substitution.Key == match.Groups[1].Value)
                    {
                        return substitution.ValueAsString();
                    }
                }
                throw new KeyNotFoundException(match.Groups[1].Value);
            });
        }

        TrimAndSubstitute();
        _textHost.Document = new TextDocument(new StringTextSource(sampleString));
        _textHost.TextArea.IndentationStrategy.IndentLines(_textHost.Document, 0, _textHost.Document.LineCount);

        //if (isCSharpSample && _cSharpTextEditor != null)
        //{
        //    TrimAndSubstitute();

        //    _cSharpTextEditor.Text = sampleString;
        //    _cSharpTextEditor.TextArea.IndentationStrategy.IndentLines(_cSharpTextEditor.Document, 0, _cSharpTextEditor.Document.LineCount);
        //}
        //else if (_xamlTextEditor != null)
        //{
        //    TrimAndSubstitute();

        //    _xamlTextEditor.Text = sampleString;
        //}
    }

    private void ShowCopiedFlyout(Button host, string message = "Copied!", bool fail = false)
    {
        if (_confirmCopyTeachingTip == null)
        {
            _confirmCopyTeachingTip = new TeachingTip
            {
                Subtitle = message,
                IsLightDismissEnabled = true,
                Target = host
            };
        }
        else
        {
            _confirmCopyTeachingTip.Subtitle = message;
            _confirmCopyTeachingTip.Target = host;
        }

        _confirmCopyTeachingTip.IsOpen = true;

        Dispatcher.UIThread.Post(async () =>
        {
            await Task.Delay(1000);
            _confirmCopyTeachingTip.IsOpen = true;
            // Make sure we don't hold a ref to the control
            _confirmCopyTeachingTip.Target = null;

        }, DispatcherPriority.Background);
    }


    private Button _copyCodeButton;
    private static TeachingTip _confirmCopyTeachingTip;

    private static readonly Regex SubstitutionPattern = new Regex(@"\$\(([^\)]+)\)");
    private IList<ControlExampleSubstitution> _substitutions;
    private bool _hasRegisteredSubstitutions;

    private TextEditor _textHost;
    private TextMate.Installation _textMateInstall;
    private static RegistryOptions _options;
    private static string _cSharpLangId;
    private static string _xamlLangId;
    private static TextMateSharp.Themes.IRawTheme _darkTheme;
}


// From WinUI XamlControlsGallery
public class ControlExampleSubstitution : AvaloniaObject
{
    public static readonly DirectProperty<ControlExampleSubstitution, bool> IsEnabledProperty =
        AvaloniaProperty.RegisterDirect<ControlExampleSubstitution, bool>(nameof(IsEnabled),
             x => x.IsEnabled, (x, v) => x.IsEnabled = v);

    public static readonly DirectProperty<ControlExampleSubstitution, object> ValueProperty =
        AvaloniaProperty.RegisterDirect<ControlExampleSubstitution, object>(nameof(Value),
            x => x.Value, (x, v) => x.Value = v);

    public string Key { get; set; }

    private object _value = null;
    public object Value
    {
        get => _value;
        set
        {
            SetAndRaise(ValueProperty, ref _value, value);
            ValueChanged?.Invoke(this, null);
        }
    }

    private bool _enabled = true;
    public bool IsEnabled
    {
        get => _enabled;
        set
        {
            SetAndRaise(IsEnabledProperty, ref _enabled, value);
            ValueChanged?.Invoke(this, null);
        }
    }

    public event TypedEventHandler<ControlExampleSubstitution, object> ValueChanged;

    public string ValueAsString()
    {
        if (!IsEnabled)
        {
            return string.Empty;
        }

        object value = Value;

        // For solid color brushes, use the underlying color.
        if (value is SolidColorBrush)
        {
            value = ((SolidColorBrush)value).Color;
        }

        if (value == null)
        {
            return string.Empty;
        }

        return value.ToString();
    }
}
