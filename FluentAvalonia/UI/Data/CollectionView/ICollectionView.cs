using Avalonia.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Data
{
	/// <summary>
	/// Enables collections to support current record management, grouping, and incremental loading
	/// </summary>
	public interface ICollectionView : IEnumerable<object>, IList<object>, INotifyCollectionChanged
	{
		// WinUI/UWP docs say that these objects should be ICollectionViewGroup even though
		// it's specified as I[List]<object>, so to avoid needless casting all the time, 
		// I'm just gonna do that...
		/// <summary>
		/// Returns any collection groups that are associated with the view
		/// </summary>
		IAvaloniaList<ICollectionViewGroup> CollectionGroups { get; }

		/// <summary>
		/// Gets the current item in the view
		/// </summary>
		object CurrentItem { get; }

		/// <summary>
		/// Gets the ordinal position of the CurrentItem within the View
		/// </summary>
		int CurrentPosition { get; }

		/// <summary>
		/// Gets a sentinel value that supports incremental loading implementations. See also LoadMoreItemsAsync
		/// </summary>
		/// <remarks>
		/// Not implemented
		/// </remarks>
		bool HasMoreItems { get; }

		/// <summary>
		/// Gets a value that indicates whether the CurrentItem of the view is beyond the end of the collection
		/// </summary>
		bool IsCurrentAfterLast { get; }

		/// <summary>
		/// Gets a value that indicates whether the CurrentItem of the view is beyond the beginning of the collection
		/// </summary>
		bool IsCurrentBeforeFirst { get; }

		/// <summary>
		/// Initializes incremental loading from the view
		/// </summary>
		/// <param name="count">The number of items to load</param>
		/// <returns>The wrapped results of the load operation</returns>
		/// <remarks>Not implemented</remarks>
		Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count);

		/// <summary>
		/// Sets the specified item to be the CurrentItem in the view
		/// </summary>
		/// <param name="item">The item to set as the CurrentItem</param>
		/// <returns>True if the resulting CurrentItem is within the view; otherwise, False</returns>
		bool MoveCurrentTo(object item);

		/// <summary>
		/// Sets the first item in the view as the CurrentItem
		/// </summary>
		/// <returns>True if the resulting CurrentItem is an item within the view; otherwise, False</returns>
		bool MoveCurrentToFirst();

		/// <summary>
		/// Sets the last item in the view as the CurrentItem
		/// </summary>
		/// <returns>True if the resulting CurrentItem is an item within the View; otherwise, False</returns>
		bool MoveCurrentToLast();

		/// <summary>
		/// Sets the item after the CurrentItem in the view as the CurrentItem
		/// </summary>
		/// <returns>True if the resulting CurrentItem is an item within the View; otherwise, False</returns>
		bool MoveCurrentToNext();

		/// <summary>
		/// Sets the item at the specified index to be the CurrentItem in the view
		/// </summary>
		/// <param name="pos">The index of the item to move to</param>
		/// <returns>True if the resulting CurrentItem is an item within the view; otherwise, false</returns>
		bool MoveCurrentToPosition(int pos);

		/// <summary>
		/// Sets the item before the CurrentItem in the view as the CurrentItem
		/// </summary>
		/// <returns>True if the resulting CurrentItem is an item within the View; otherwise, False</returns>
		bool MoveCurrentToPrevious();

		/// <summary>
		/// When implementing this interface, fire this event after the current item has been changed
		/// </summary>
		event EventHandler<object> CurrentChanged;

		/// <summary>
		/// When implementing this interface, fire this event before changing the current item. The event handler can cancel this event
		/// </summary>
		event CurrentChangingEventHandler CurrentChanging;
	}
}
