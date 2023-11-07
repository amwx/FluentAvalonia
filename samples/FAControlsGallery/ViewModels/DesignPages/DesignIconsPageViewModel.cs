using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Platform;

namespace FAControlsGallery.ViewModels.DesignPages;

public class DesignIconsPageViewModel : ViewModelBase
{
    public string FilterText
    {
        get => _filterText;
        set
        {
            if (RaiseAndSetIfChanged(ref _filterText, value))
            {
                RaisePropertyChanged(nameof(Filter));
            }
        }
    }

    public Predicate<object> Filter
    {
        get
        {
            return (obj) =>
            {
                if (string.IsNullOrEmpty(_filterText))
                    return true;

                return obj is FontIconInfo fii &&
                    fii.Name.Contains(_filterText, StringComparison.OrdinalIgnoreCase);
            };
        }
    }

    public Task<List<FontIconInfo>> Icons => GetIcons();

    private async Task<List<FontIconInfo>> GetIcons()
    {
        return await Task.Run(() =>
        {
            using var s = AssetLoader.Open(new Uri("avares://FAControlsGallery/Assets/FASymbolFontList.json"));
            var icons = JsonSerializer.Deserialize(s, FAControlsJsonSerializerContext.Default.ListFontIconInfo);

            return icons;
        });
    }

    private string _filterText;
}

public class FontIconInfo
{
    [JsonConstructor]
    public FontIconInfo(string name, string codepoint)
    {
        Name = name;
        Codepoint = codepoint;
        XamlGlyph = $"&#x{codepoint};";
        CSharpGlyph = $"\\u{codepoint}";

        XamlExample = @"<ui:FontIcon Glyph=""" + XamlGlyph + @""" />";
        CSharpExample = @"FontIcon fontIcon = new FontIcon()" + Environment.NewLine +
            @"fontIcon.Glyph = """ + CSharpGlyph + @""";";

        Glyph = char.ConvertFromUtf32((int)Convert.ToUInt32(codepoint, 16)).ToString();
    }

    public string Name { get; set; }

    public string Codepoint { get; set; }

    public string Glyph { get; }

    [JsonIgnore]
    public string XamlGlyph { get; }

    [JsonIgnore]
    public string CSharpGlyph { get; }

    [JsonIgnore]
    public string XamlExample { get; }

    [JsonIgnore]
    public string CSharpExample { get; }
}
