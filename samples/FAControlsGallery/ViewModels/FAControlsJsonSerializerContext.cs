using System.Text.Json.Serialization;
using FAControlsGallery.ViewModels.DesignPages;

namespace FAControlsGallery.ViewModels;

[JsonSerializable(typeof(List<PageBaseViewModel>))]
[JsonSerializable(typeof(List<FontIconInfo>))]
[JsonSerializable(typeof(List<FAControlsGroupItem>))]
internal partial class FAControlsJsonSerializerContext : JsonSerializerContext
{
}
