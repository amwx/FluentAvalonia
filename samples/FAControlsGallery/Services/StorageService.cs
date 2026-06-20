using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace FAControlsGallery.Services;

public sealed class StorageService
{
    private StorageService(TopLevel tl)
    {
        _topLevel = tl;
        string root = null;

        // Ref: https://github.com/AvaloniaUI/Avalonia/discussions/7190
        // Note Avalonia StorageProvider doesn't give AppData/LocalAppData folders
        if (OperatingSystem.IsWindows())
        {
            root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
        {
            // Mobile platforms use sandboxed storage, no need for CompanyName/AppName subfolders
            // Android sandbox:
            // both resolve to: /data/user/0/<package.name>/files
            // iOS sandbox:
            // both resolve to: /var/mobile/Containers/Data/Application/<GUID>/Library/Application Support
            root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else if (OperatingSystem.IsBrowser())
        {
            // TODO
        }

        _rootAppDataPath = root;
    }

    public static StorageService Instance { get; private set; }

    public async Task WriteToFile(string fileName, string contents)
    {
        var folder = await EnsureFAControlsGalleryFolder();

        if (folder == null)
            return;

        var file = await folder.GetFileAsync(fileName);
        if (file == null)
        {
            file = await folder.CreateFileAsync(fileName);
        }    

        using var stream = await file.OpenWriteAsync();
        using var writer = new StreamWriter(stream);
        await writer.WriteAsync(contents);
    }

    public async Task<string> OpenFile(string fileName)
    {
        var folder = await EnsureFAControlsGalleryFolder();

        if (folder == null)
            return null;

        var file = await folder.GetFileAsync(fileName);
        if (file == null)
            return null;

        using var stream = await file.OpenReadAsync();
        using var streamReader = new StreamReader(stream);

        return await streamReader.ReadToEndAsync();
    }

    internal static void Create(TopLevel toplevel)
    {
        Instance = new StorageService(toplevel);
    }

    private async Task<IStorageFolder> EnsureFAControlsGalleryFolder()
    {
        if (_rootAppDataPath == null)
            return await Task.FromResult<IStorageFolder>(null);

        var sp = _topLevel.StorageProvider;
        var rootFolder = await sp.TryGetFolderFromPathAsync(new Uri(_rootAppDataPath));
        if (rootFolder == null)
            return await Task.FromResult<IStorageFolder>(null); // This shouldn't happen b/c this is the AppData folder

        var faFolder = await rootFolder.GetFolderAsync(FAControlsGallery);
        if (faFolder == null)
        {
            faFolder = await rootFolder.CreateFolderAsync(FAControlsGallery);
        }

        return faFolder;
    }

    private readonly TopLevel _topLevel;
    private readonly string _rootAppDataPath;
    private const string FAControlsGallery = "FAControlsGallery";
}
