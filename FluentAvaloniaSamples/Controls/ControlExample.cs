using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Indentation.CSharp;
using FluentAvalonia.Core;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FluentAvaloniaSamples.Controls;

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

public class ControlExample : HeaderedContentControl
{
    public ControlExample()
    {
        _substitutions = new List<ControlExampleSubstitution>();
        _flyoutOptions = new List<MenuFlyoutItemBase>();

        ResourcesChanged += OnResourcesChanged;
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

    public static readonly StyledProperty<IControl> OptionsProperty =
        AvaloniaProperty.Register<ControlExample, IControl>(nameof(Options));

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

    public IControl Options
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

    private static Regex SubstitutionPattern = new Regex(@"\$\(([^\)]+)\)");

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_copyXamlButton != null)
            _copyXamlButton.Click -= OnCopyXamlClick;

        if (_copyCSharpButton != null)
            _copyCSharpButton.Click -= OnCopyCSharpClick;

        if (_expandOptionsButton != null)
            _expandOptionsButton.Click -= OnExpandOptionsClick;

        base.OnApplyTemplate(e);

        _expandOptionsButton = e.NameScope.Find<Button>("ShowHideOptionsButton");
        _expandOptionsButton.Click += OnExpandOptionsClick;

        _previewAreaHost = e.NameScope.Find<Border>("ControlPreviewAreaHost");

        _optionsMenuButton = e.NameScope.Find<Button>("OptionsMenuButton");
        if (_optionsMenuButton != null)
        {
            if (!EnableShowDefinitionLink && !EnableShowDocsLink)
            {
                _optionsMenuButton.IsVisible = false;
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
                            Icon = new SymbolIcon { Symbol = Symbol.Link }
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
                        Icon = new SymbolIcon { Symbol = Symbol.CodeFilled }
                    };

                    defItem.Click += ShowControlDefintion;

                    l.Add(defItem);
                }

                _optionsMenuButton.Flyout = new FluentAvalonia.UI.Controls.FAMenuFlyout
                {
                    Items = l,
                    Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft
                };
            }
        }

        _copyXamlButton = e.NameScope.Find<Button>("CopyXamlButton");
        _copyXamlButton.Click += OnCopyXamlClick;

        _copyCSharpButton = e.NameScope.Find<Button>("CopyCSharpButton");
        _copyCSharpButton.Click += OnCopyCSharpClick;

        _xamlTextEditor = e.NameScope.Find<TextEditor>("XamlTextEditor");
        _cSharpTextEditor = e.NameScope.Find<TextEditor>("CSharpTextEditor");

        _usageNotesTextBlock = e.NameScope.Find<TextBlock>("UsageNotesTextBlock");

        SetUsageNotes();

        bool isLightMode = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().RequestedTheme == FluentAvaloniaTheme.LightModeString;

        _xamlTextEditor.SyntaxHighlighting = isLightMode ? XamlHighlightingSource.LightModeXaml : XamlHighlightingSource.DarkModeXaml;
        _cSharpTextEditor.SyntaxHighlighting = isLightMode ? CSharpHighlightingSource.CSharpLightMode : CSharpHighlightingSource.CSharpDarkMode;

        //_xamlTextEditor.TextArea.SelectionBrush = Brushes.Transparent;
        // _cSharpTextEditor.TextArea.SelectionBrush = Brushes.Transparent;

        _xamlTextEditor.Text = XamlSource;
        _cSharpTextEditor.Text = CSharpSource;

        _cSharpTextEditor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(_cSharpTextEditor.Options);
        _cSharpTextEditor.TextArea.IndentationStrategy.IndentLines(_cSharpTextEditor.Document, 0, _cSharpTextEditor.Document.LineCount);

        // HACK: Links apparently can't be turned off (if you see this and know how, pls open a PR and fix this =D), so force links
        //       to be the same color as attributes
        _cSharpTextEditor.Options.EnableHyperlinks = false;
        _xamlTextEditor.Options.EnableHyperlinks = false;

        //_xamlTextEditor.TextArea.TextView.LinkTextForegroundBrush = new ImmutableSolidColorBrush(Color.FromRgb(255, 160, 122));
        //_xamlTextEditor.TextArea.TextView.LinkTextUnderline = false;

        if (Substitutions.Count > 0 && !_hasRegisteredSubstitutions)
        {
            foreach (var sub in Substitutions)
            {
                sub.ValueChanged += OnSubstitutionValueChanged;
            }

            _hasRegisteredSubstitutions = true;
        }

        bool hasXaml = XamlSource != null;
        bool hasCSharp = CSharpSource != null;
        PseudoClasses.Set(":codepreview", hasXaml || hasCSharp || UsageNotes != null);

        PseudoClasses.Set(":xamlsource", hasXaml);
        PseudoClasses.Set(":csharpsource", hasCSharp);

        if (hasXaml)
            FormatAndRenderSampleFromString(XamlSource, false);

        if (hasCSharp)
            FormatAndRenderSampleFromString(CSharpSource, true);

        if (this.TryFindResource("TextControlSelectionHighlightColor", out var value))
        {
            if (value is ISolidColorBrush sb)
            {
                var b = new ImmutableSolidColorBrush(sb.Color, 0.5);
                //_xamlTextEditor.TextArea.SelectionBrush = b;
                //_cSharpTextEditor.TextArea.SelectionBrush = b;
            }
        }
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

            PseudoClasses.Set(":adaptiveW", wid < 725);
            PseudoClasses.Set(":small", wid < 500);
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

    private void OnResourcesChanged(object sender, ResourcesChangedEventArgs e)
    {
        if (_cSharpTextEditor != null)
        {
            bool isLightMode = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().RequestedTheme == FluentAvaloniaTheme.LightModeString;

            _xamlTextEditor.SyntaxHighlighting = isLightMode ? XamlHighlightingSource.LightModeXaml : XamlHighlightingSource.DarkModeXaml;
            _cSharpTextEditor.SyntaxHighlighting = isLightMode ? CSharpHighlightingSource.CSharpLightMode : CSharpHighlightingSource.CSharpDarkMode;
        }
    }

    public void SetExampleTheme(bool isLightMode)
    {
        if (_cSharpTextEditor != null)
        {
            _xamlTextEditor.SyntaxHighlighting = isLightMode ? XamlHighlightingSource.LightModeXaml : XamlHighlightingSource.DarkModeXaml;
            _cSharpTextEditor.SyntaxHighlighting = isLightMode ? CSharpHighlightingSource.CSharpLightMode : CSharpHighlightingSource.CSharpDarkMode;

            if (this.TryFindResource("TextControlSelectionHighlightColor", out var value))
            {
                if (value is ISolidColorBrush sb)
                {
                    var b = new ImmutableSolidColorBrush(sb.Color, 0.5);
                    //_xamlTextEditor.TextArea.SelectionBrush = b;
                    //_cSharpTextEditor.TextArea.SelectionBrush = b;
                }
            }
        }
    }

    private void OnSubstitutionValueChanged(ControlExampleSubstitution sender, object args)
    {
        FormatAndRenderSampleFromString(XamlSource, false);
        FormatAndRenderSampleFromString(CSharpSource, false);
    }

    private void FormatAndRenderSampleFromString(string sampleString, bool isCSharpSample)
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

        if (isCSharpSample && _cSharpTextEditor != null)
        {
            TrimAndSubstitute();

            _cSharpTextEditor.Text = sampleString;
            _cSharpTextEditor.TextArea.IndentationStrategy.IndentLines(_cSharpTextEditor.Document, 0, _cSharpTextEditor.Document.LineCount);
        }
        else if (_xamlTextEditor != null)
        {
            TrimAndSubstitute();

            _xamlTextEditor.Text = sampleString;
        }
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

    private async void OnCopyCSharpClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_cSharpTextEditor.SelectionLength > 0)
            {
                // Copy the selected text
                _cSharpTextEditor.Copy();
            }
            else
            {
                // Copy everything
                await Application.Current.Clipboard.SetTextAsync(_cSharpTextEditor.Text);
            }


            ShowCopiedFlyout(sender as Button);
        }
        catch
        {
            ShowCopiedFlyout(sender as Button, "Failed to copy code", true);
        }
    }

    private async void OnCopyXamlClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_xamlTextEditor.SelectionLength > 0)
            {
                // Copy the selected text
                _xamlTextEditor.Copy();
            }
            else
            {
                // Copy everything
                await Application.Current.Clipboard.SetTextAsync(_xamlTextEditor.Text);
            }

            ShowCopiedFlyout(sender as Button);
        }
        catch
        {
            ShowCopiedFlyout(sender as Button, "Failed to copy code", true);
        }
    }

    private void ShowCopiedFlyout(Button host, string message = "Copied!", bool fail = false)
    {
        if (_copiedNoticeFlyout == null)
        {
            _copiedNoticeFlyout = new Flyout
            {
                Content = new TextBlock
                {
                    Text = message,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                },
                FlyoutPresenterClasses =
                {
                    "CodeSampleCopiedFlyout"
                }
            };
        }
        else
        {
            var tb = _copiedNoticeFlyout.Content as TextBlock;
            tb.Text = message;
        }

        if (fail)
        {
            _copiedNoticeFlyout.FlyoutPresenterClasses.Add("Fail");
        }
        else
        {
            _copiedNoticeFlyout.FlyoutPresenterClasses.Remove("Fail");
        }

        _copiedNoticeFlyout.ShowAt(host);

        Dispatcher.UIThread.Post(async () =>
        {
            await Task.Delay(1000);
            _copiedNoticeFlyout.Hide();

        }, DispatcherPriority.Background);
    }

    private void SetUsageNotes()
    {
        if (_usageNotesTextBlock == null || string.IsNullOrEmpty(UsageNotes))
        {
            return;
        }

        string notes = UsageNotes;
        if (Uri.TryCreate(notes, UriKind.Absolute, out Uri result))
        {
            // Uri may be created even when it shouldn't so Try-Catch this
            try
            {
                using (var s = AvaloniaLocator.Current.GetService<IAssetLoader>().Open(result))
                using (var sr = new StreamReader(s))
                {
                    notes = sr.ReadToEnd();
                }
            }
            catch { }
        }

        _usageNotesTextBlock.Text = notes;

        PseudoClasses.Set(":usagenotes", true);
        PseudoClasses.Set(":codepreview", true);
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


    public static Flyout _copiedNoticeFlyout;
    private Button _copyXamlButton;
    private Button _copyCSharpButton;
    private Border _previewAreaHost;
    private Button _expandOptionsButton;

    private Button _optionsMenuButton;
    private TextEditor _xamlTextEditor;
    private TextEditor _cSharpTextEditor;
    private TextBlock _usageNotesTextBlock;

    private IList<ControlExampleSubstitution> _substitutions;
    private IList<MenuFlyoutItemBase> _flyoutOptions;
    private bool _hasRegisteredSubstitutions;

    private static readonly HashSet<string> _availableAvaloniaDocs = new HashSet<string>();
}

public class XamlHighlightingSource : IHighlightingDefinition
{
    public XamlHighlightingSource(bool isDark)
    {
        MainRuleSet = new HighlightingRuleSet
        {
            Spans =
            {
                new HighlightingSpan // Comment
                {
                    StartExpression = new System.Text.RegularExpressions.Regex(@"<!--"),
                    EndExpression = new System.Text.RegularExpressions.Regex(@"-->"),
                    //SpanColor = new HighlightingColor { Foreground =new SimpleHighlightingBrush(isDark ? Color.FromRgb(107,142,35) : Colors.Green) }
                },
                new HighlightingSpan // XML tag
                {
                    StartExpression = new System.Text.RegularExpressions.Regex("<"),
                    EndExpression = new System.Text.RegularExpressions.Regex(@">"),
                    //SpanColor =new HighlightingColor { Foreground =new SimpleHighlightingBrush(isDark ? Color.FromRgb(121,121,121) : Colors.Blue) },
                    SpanColorIncludesEnd = true,
                    SpanColorIncludesStart = true,
                    RuleSet = new HighlightingRuleSet
                    {
                        Rules =
                        {
                            new HighlightingRule // Attribute Name
                            {
                                 Regex = new System.Text.RegularExpressions.Regex(@"[\d\w_\-\.]+(?=(\s*=))"),
                                 //Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(isDark ? Color.FromRgb(135,206,250) : Colors.Red) }
                            },
                            new HighlightingRule // Tag Name
                            {
                                 Regex = new System.Text.RegularExpressions.Regex(@"(?!(\/|<))[\w\d\#]+"),
                                 //Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(isDark ? Color.FromRgb(87,117,202) : Color.FromRgb(163, 21, 21)) }
                            },
                            new HighlightingRule
                            {
                                Regex = new System.Text.RegularExpressions.Regex("/"),
                                //Color =new HighlightingColor { Foreground =new SimpleHighlightingBrush(isDark ? Color.FromRgb(121,121,121) : Colors.Blue) },
                            }
                        },
                        Spans =
                        {
                            new HighlightingSpan
                            {
                                StartExpression = new System.Text.RegularExpressions.Regex("\""),
                                EndExpression = new System.Text.RegularExpressions.Regex("\"|(?=<)"),
                                //SpanColor = new HighlightingColor { Foreground =new SimpleHighlightingBrush(isDark ? Color.FromRgb(255,160,122) : Colors.Blue) },
                            }
                        }
                    }
                },
                new HighlightingSpan
                {
                    StartExpression = new System.Text.RegularExpressions.Regex("\""),
                    EndExpression = new System.Text.RegularExpressions.Regex("\""),
                    //SpanColor = new HighlightingColor { Foreground =new SimpleHighlightingBrush(isDark ? Color.FromRgb(255,160,122) : Colors.Blue) },
                    SpanColorIncludesEnd = true,
                    SpanColorIncludesStart = true
                },
            }
        };

    }

    static XamlHighlightingSource()
    {
        LightModeXaml = new XamlHighlightingSource(false);
        DarkModeXaml = new XamlHighlightingSource(true);
    }


    public static readonly XamlHighlightingSource LightModeXaml;
    public static readonly XamlHighlightingSource DarkModeXaml;

    public string Name => "XamlHighlightRules";
    public HighlightingRuleSet MainRuleSet { get; }
    public IEnumerable<HighlightingColor> NamedHighlightingColors { get; }
    public IDictionary<string, string> Properties { get; }

    public HighlightingColor GetNamedColor(string name)
    {
        throw new NotImplementedException();
    }

    public HighlightingRuleSet GetNamedRuleSet(string name)
    {
        throw new NotImplementedException();
    }
}

public class CSharpHighlightingSource : IHighlightingDefinition
{
    public CSharpHighlightingSource(bool isDark)
    {
        MainRuleSet = new HighlightingRuleSet
        {
            Rules =
            {
                new HighlightingRule // blue keywords
                {
                    Regex = new System.Text.RegularExpressions.Regex("\\b(this|base|as|is|new|sizeof|typeof|stackalloc|default|true|false|get|set|add|remove|ref|out)\\b"),
                    //Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(isDark ? Color.FromRgb(86,156,214) : Colors.Blue) },
                },
                new HighlightingRule // purple keywords
                {
                    Regex = new System.Text.RegularExpressions.Regex("\\b(switch|else|if|case|break|return|for|foreach|while|do|continue|try|catch|finally)\\b"),
                    //Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Color.FromRgb(216,160,223)) }, // TODO...
                },
                new HighlightingRule // value types (also blue)
                {
                    Regex = new System.Text.RegularExpressions.Regex("\\b(bool|byte|char|decimal|double|enum|float|int|long|sbyte|short|struct|uint|ushort|ulong)\\b"),
                    //Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(isDark ? Color.FromRgb(86,156,214) : Colors.Blue) },
                },
                new HighlightingRule // other blue stuff
                {
                    Regex = new System.Text.RegularExpressions.Regex("\\b(class|delegate|object|string|void|abstract|const|override|readonly|sealed|static|partial|virtual|async|public|protected|private|internal|namespace|using|null|nameof|explicit|implicit|operator)\\b"),
                   // Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(isDark ? Color.FromRgb(86,156,214) : Colors.Blue) },
                },
            },
            Spans =
            {
                new HighlightingSpan // Comment
                {
                    StartExpression = new System.Text.RegularExpressions.Regex(@"//"),
                    EndExpression = new Regex("$"),
                    SpanColorIncludesStart = true,
                    SpanColorIncludesEnd = true,
                    //SpanColor = new HighlightingColor { Foreground = new SimpleHighlightingBrush(isDark ? Color.FromRgb(89,166,74) : Colors.Green) }
                },
                new HighlightingSpan // String or carh
                {
                    StartExpression = new System.Text.RegularExpressions.Regex("(\"|')"),
                    EndExpression = new System.Text.RegularExpressions.Regex("(\"|')"),
                    //SpanColor = new HighlightingColor { Foreground = new SimpleHighlightingBrush(isDark ? Color.FromRgb(214,157,133) : Color.FromRgb(162,21,21)) }, 
                    SpanColorIncludesEnd = true,
                    SpanColorIncludesStart = true
                }
            }
        };
    }

    static CSharpHighlightingSource()
    {
        CSharpLightMode = new CSharpHighlightingSource(false);
        CSharpDarkMode = new CSharpHighlightingSource(true);
    }

    public static readonly CSharpHighlightingSource CSharpLightMode;
    public static readonly CSharpHighlightingSource CSharpDarkMode;

    public string Name => "C#Highlighting";
    public HighlightingRuleSet MainRuleSet { get; }
    public IEnumerable<HighlightingColor> NamedHighlightingColors { get; }
    public IDictionary<string, string> Properties { get; }

    public HighlightingColor GetNamedColor(string name)
    {
        throw new NotImplementedException();
    }

    public HighlightingRuleSet GetNamedRuleSet(string name)
    {
        throw new NotImplementedException();
    }
}
