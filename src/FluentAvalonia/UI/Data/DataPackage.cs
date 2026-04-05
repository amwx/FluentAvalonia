using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace FluentAvalonia.UI.Data;

/// <summary>
/// This class is part of the ListView logic, which has been suspended for now
/// </summary>
public sealed class DataPackage : IDataTransfer, IAsyncDataTransfer
{
    public DataPackage()
    {
        _dt = new DataTransfer();
    }

    /// <summary>
    /// Gets or sets the requested operation for the data object
    /// </summary>
    public DragDropEffects RequestedOperation { get; set; }

    public void SetText(string text)
    {
        _dt.Add(DataTransferItem.CreateText(text));
    }

    public string GetText()
    {
        return _dt.TryGetText();
    }

    public void SetStorageItems(IEnumerable<IStorageItem> items)
    {
        foreach (var item in items)
        {
            _dt.Add(DataTransferItem.CreateFile(item));
        }
    }

    public IReadOnlyList<IStorageItem> GetStorageItems()
    {
        return _dt.TryGetFiles();
    }

    public void SetBitmap(Bitmap bmp)
    {
        throw new NotImplementedException("Avalonia doesn't currently support this");
    }

    IReadOnlyList<DataFormat> IDataTransfer.Formats => _dt.Formats;

    IReadOnlyList<IDataTransferItem> IDataTransfer.Items => _dt.Items;

    public IReadOnlyList<DataFormat> Formats => _dt.Formats;
    public IReadOnlyList<IAsyncDataTransferItem> Items => _dt.Items;

    public void Dispose()
    {

    }

    private readonly DataTransfer _dt;
}
