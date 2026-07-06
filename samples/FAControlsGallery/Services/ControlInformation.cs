using System.Text.Json;
using Avalonia.Platform;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Services;

public sealed class ControlInformation
{
    public static List<PageBaseViewModel> GetCoreControlInfo()
    {
        const string coreControls = "avares://FAControlsGallery/Assets/CoreControlsGroups.json";
       
        return _coreControlInfo ??= 
            JsonSerializer.Deserialize(GetControlsList(coreControls), FAControlsJsonSerializerContext.Default.ListPageBaseViewModel);
    }

    public static List<FAControlsGroupItem> GetFAControlInfo()
    {
        const string faControls = "avares://FAControlsGallery/Assets/FAControlsGroups.json";

        return _faControlsInfo ??=
            JsonSerializer.Deserialize(GetControlsList(faControls), FAControlsJsonSerializerContext.Default.ListFAControlsGroupItem);
    }

    private static string GetControlsList(string name)
    {
        using (var stream = AssetLoader.Open(new Uri(name)))
        using (StreamReader reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }

    private static List<PageBaseViewModel> _coreControlInfo;
    private static List<FAControlsGroupItem> _faControlsInfo;
}
