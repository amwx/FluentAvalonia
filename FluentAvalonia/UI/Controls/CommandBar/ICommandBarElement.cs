namespace FluentAvalonia.UI.Controls
{
	public interface ICommandBarElement
	{
		int DynamicOverflowOrder { get; set; }
		bool IsCompact { get; set; }
		bool IsInOverflow { get; }
	}
}
