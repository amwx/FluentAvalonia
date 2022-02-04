namespace FluentAvalonia.Core.ApplicationModel
{
	public interface ICoreApplicationView
	{
        /// <summary>
        /// Gets the <see cref="CoreApplicationViewTitleBar"/> associated with this Window
        /// </summary>
		CoreApplicationViewTitleBar TitleBar { get; }
	}
}
