using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace FluentAvalonia.UI.Data;

/// <summary>
/// Contains the data a user want to exchange
/// </summary>
public sealed class DataPackage : IDataTransfer, IAsyncDataTransfer
{
    public DataPackage()
    {
        _dt = new DataTransfer();
    }

    public IReadOnlyList<DataFormat> Formats => _dt.Formats;

    public IReadOnlyList<IAsyncDataTransferItem> Items => _dt.Items;

    /// <summary>
    /// Gets or sets the requested operation for the data object
    /// </summary>
    public DragDropEffects RequestedOperation { get; set; }

    /// <summary>
    /// Adds the specified text into the Data Transfer package
    /// </summary>
    public void SetText(string text)
    {
        _dt.Add(DataTransferItem.CreateText(text));
    }

    /// <summary>
    /// If present, synchronously retrieves the current text in the Data Transfer package
    /// </summary>
    public string GetText()
    {
        return _dt.TryGetText();
    }

    /// <summary>
    /// If present, asynchronously retrieves the current text in the Data Transfer package
    /// </summary>
    public Task<string> GetTextAsync()
    {
        return _dt.TryGetTextAsync();
    }

    /// <summary>
    /// Sets the specified <see cref="IStorageItem"/>s into the current
    /// Data Transfer package
    /// </summary>
    public void SetStorageItems(IEnumerable<IStorageItem> items)
    {
        foreach (var item in items)
        {
            _dt.Add(DataTransferItem.CreateFile(item));
        }
    }

    /// <summary>
    /// If present, synchronously retreives the <see cref="IStorageItem"/>s in
    /// the current Data Transfer package
    /// </summary>
    public IReadOnlyList<IStorageItem> GetStorageItems()
    {
        return _dt.TryGetFiles();
    }

    /// <summary>
    /// If present, asynchronously retreives the <see cref="IStorageItem"/>s in
    /// the current Data Transfer package
    /// </summary>
    public async Task<IReadOnlyList<IStorageItem>> GetStorageItemsAsync()
    {
        var result = await _dt.TryGetFilesAsync();
        return result;
    }

    /// <summary>
    /// Sets the specified <see cref="Bitmap"/> to the current Data Transfer package
    /// </summary>
    public void SetBitmap(Bitmap bmp)
    {
        _dt.Add(DataTransferItem.Create(DataFormat.Bitmap, bmp));
    }

    /// <summary>
    /// If present, synchronously retreives the <see cref="Bitmap"/>s in
    /// the current Data Transfer package
    /// </summary>
    public Bitmap GetBitmap()
    {
        return _dt.TryGetBitmap();
    }

    /// <summary>
    /// If present, asynchronously retreives the <see cref="Bitmap"/>s in
    /// the current Data Transfer package
    /// </summary>
    public Task<Bitmap> GetBitmapAsync()
    {
        return _dt.TryGetBitmapAsync();
    }

    /// <summary>
    /// Sets an unspecified object to the Data Transfer package with 
    /// the custom <see cref="DataFormat{T}"/>
    /// </summary>
    public void Set<T>(DataFormat<T> format, T value) where T : class
    {
        _dt.Add(DataTransferItem.Create(format, value));
    }

    /// <summary>
    /// If present, synchronously retreives the item in the Data Transfer package
    /// using the specified <see cref="DataFormat{T}"/>
    /// </summary>
    public T Get<T>(DataFormat<T> format) where T : class
    {
        return _dt.TryGetValue(format);
    }
    
    /// <summary>
    /// If present, asynchronously retreives the item in the Data Transfer package
    /// using the specified <see cref="DataFormat{T}"/>
    /// </summary>
    public Task<T> GetAsync<T>(DataFormat<T> format) where T : class
    {
        return _dt.TryGetValueAsync(format);
    }

    IReadOnlyList<DataFormat> IDataTransfer.Formats => _dt.Formats;

    IReadOnlyList<IDataTransferItem> IDataTransfer.Items => _dt.Items;

    void IDisposable.Dispose() { }

    private readonly DataTransfer _dt;
}
