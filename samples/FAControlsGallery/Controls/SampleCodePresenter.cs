using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaEdit;
using TextMateSharp.Grammars;
using AvaloniaEdit.TextMate;
using AvaloniaEdit.Document;
using FAControlsGallery.Services;

namespace FAControlsGallery.Controls;

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
        ControlExample.SubstitutionsProperty.AddOwner<SampleCodePresenter>(x => x.Substitutions, 
            (x,v) => x.Substitutions = v);

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
                await ClipboardService.SetTextAsync(_textHost.Text);
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
    }

    private void ShowCopiedFlyout(Button host, string message = "Copied!", bool fail = false)
    {
        if (_confirmCopiedFlyout == null)
        {
            _confirmCopiedFlyout = new Flyout
            {
                Content = new TextBlock
                {
                    Text = message
                }
            };
        }
        else
        {
            (_confirmCopiedFlyout.Content as TextBlock).Text = message;
        }

        _confirmCopiedFlyout.ShowAt(host);

        DispatcherTimer.RunOnce(() =>
        {
            _confirmCopiedFlyout.Hide();
        }, TimeSpan.FromSeconds(1), DispatcherPriority.Background);
    }

    internal static RegistryOptions GetTextMateRegistryOptions()
    {
        if (_options == null)
        {
            _options = new RegistryOptions(ThemeName.Light);
            _cSharpLangId = _options.GetScopeByLanguageId(_options.GetLanguageByExtension(".cs").Id);
            _xamlLangId = _options.GetScopeByLanguageId(_options.GetLanguageByExtension(".xaml").Id);
        }

        return _options;
    }

    internal static TextMateSharp.Themes.IRawTheme GetDarkTheme()
    {
        if (_darkTheme == null)
        {
            _darkTheme = _options.LoadTheme(ThemeName.Dark);
        }

        return _darkTheme;
    }

    private Button _copyCodeButton;
    private static Flyout _confirmCopiedFlyout;

    private static readonly Regex SubstitutionPattern = new Regex(@"\$\(([^\)]+)\)");
    private IList<ControlExampleSubstitution> _substitutions;
    private bool _hasRegisteredSubstitutions;

    private TextEditor _textHost;
    private TextMate.Installation _textMateInstall;
    private static RegistryOptions _options;
    internal static string _cSharpLangId;
    private static string _xamlLangId;
    private static TextMateSharp.Themes.IRawTheme _darkTheme;
}
