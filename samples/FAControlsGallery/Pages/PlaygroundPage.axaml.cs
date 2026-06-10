using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace FAControlsGallery.Pages;

public partial class PlaygroundPage : UserControl
{
    public PlaygroundPage()
    {
        InitializeComponent();

        LoadButton.Click += LoadButtonClick;
        ResetButton.Click += ResetButtonClick;
        SwapThemeButton.Click += SwapThemeButtonClick;
        TextEditorHost.Document = null;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        InitializeEditor();
    }

    private void ResetButtonClick(object sender, RoutedEventArgs e)
    {
        if (TextEditorHost != null)
        {
            _textMateInstall.SetGrammar(null);
            TextEditorHost.Document = new TextDocument(new StringTextSource(_defaultText));
            HostScrollViewer?.Content = null;
            _textMateInstall.SetGrammar(_xamlId);
        }
    }

    private void LoadButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var fullString = $"{_prefix} {TextEditorHost.Document.Text} {_postfix}";

            var loaded = AvaloniaRuntimeXamlLoader.Load(fullString,
                Assembly.GetExecutingAssembly());
            HostScrollViewer.Content = loaded;
            ErrorMessage.IsOpen = false;
        }
        catch (Exception ex)
        {
            ErrorMessage.Content = $"An error occured parsing the xaml.\n{ex.Message}";
            ErrorMessage.IsOpen = true;            
        }        
    }

    private void InitializeEditor()
    {
        if (_options == null)
        {
            _options = new RegistryOptions(ThemeName.Light);
            _xamlId = _options.GetScopeByLanguageId(_options.GetLanguageByExtension(".xaml").Id);
        }

        _textMateInstall ??= TextEditorHost.InstallTextMate(_options, false);

        _textMateInstall.SetTheme(ActualThemeVariant == ThemeVariant.Dark ?
            _options.LoadTheme(ThemeName.Dark) : _options.LoadTheme(ThemeName.Light));

        if (TextEditorHost.Document == null)
        {
            ResetButtonClick(null, null);
        }
    }
    private void SwapThemeButtonClick(object sender, RoutedEventArgs e)
    {
        if (ThemeScope.ActualThemeVariant == ThemeVariant.Dark)
        {
            ThemeScope.RequestedThemeVariant = ThemeVariant.Light;
        }
        else
        {
            ThemeScope.RequestedThemeVariant = ThemeVariant.Dark;
        }
    }

    private TextMate.Installation _textMateInstall;
    private static RegistryOptions _options;
    private string _xamlId;
    private const string _defaultText = 
"""
<Border BorderThickness="1" BorderBrush="Green" CornerRadius="4" Padding="3">
    <StackPanel>
        <!-- Note: Bindings are not available in the playground -->
        <TextBlock>This is a sample TextBlock.</TextBlock>
        <Button Content="Click me!"/>

    </StackPanel>
</Border>
""";

    private const string _prefix =
"""
<UserControl xmlns="https://github.com/avaloniaui"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
            xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
            xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
            xmlns:ui="using:FluentAvalonia.UI.Controls"
            xmlns:uip="using:FluentAvalonia.UI.Controls.Primitives"
            xmlns:data="using:FluentAvalonia.UI.Data"
            xmlns:input="using:FluentAvalonia.UI.Input"
            xmlns:media="using:FluentAvalonia.UI.Media"
            xmlns:wnd="using:FluentAvalonia.UI.Windowing"
            Foreground="{DynamicResource TextFillColorPrimaryBrush}">
""";

    private const string _postfix = "</UserControl>";
}
