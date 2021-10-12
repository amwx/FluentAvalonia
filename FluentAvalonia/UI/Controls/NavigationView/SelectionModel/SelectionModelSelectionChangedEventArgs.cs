using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class SelectionModelSelectionChangedEventArgs : EventArgs
    {
        public SelectionModelSelectionChangedEventArgs(
            IReadOnlyList<IndexPath> deselectedIndices,
            IReadOnlyList<IndexPath> selectedIndices,
            IReadOnlyList<object> deselectedItems,
            IReadOnlyList<object> selectedItems)
        {
            DeselectedIndices = deselectedIndices ?? Array.Empty<IndexPath>();
            SelectedIndices = selectedIndices ?? Array.Empty<IndexPath>();
            DeselectedItems = deselectedItems ?? Array.Empty<object>();
            SelectedItems = selectedItems ?? Array.Empty<object>();
        }

        /// <summary>
        /// Gets the indices of the items that were removed from the selection.
        /// </summary>
        public IReadOnlyList<IndexPath> DeselectedIndices { get; }

        /// <summary>
        /// Gets the indices of the items that were added to the selection.
        /// </summary>
        public IReadOnlyList<IndexPath> SelectedIndices { get; }

        /// <summary>
        /// Gets the items that were removed from the selection.
        /// </summary>
        public IReadOnlyList<object> DeselectedItems { get; }

        /// <summary>
        /// Gets the items that were added to the selection.
        /// </summary>
        public IReadOnlyList<object> SelectedItems { get; }
    }
}
