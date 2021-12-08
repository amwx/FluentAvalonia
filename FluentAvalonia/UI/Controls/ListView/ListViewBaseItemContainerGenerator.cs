using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
	public class ListViewBaseItemContainerGenerator : IItemContainerGenerator
	{
		public ListViewBaseItemContainerGenerator(ListViewBase owner)
		{
			_owner = owner;
		}

		public IEnumerable<ItemContainerInfo> Containers => _containers?.Values ?? Enumerable.Empty<ItemContainerInfo>();
		IDataTemplate IItemContainerGenerator.ItemTemplate { get; set; }
		Type IItemContainerGenerator.ContainerType => null;

		public event EventHandler<ItemContainerEventArgs> Materialized;
		public event EventHandler<ItemContainerEventArgs> Dematerialized;
		public event EventHandler<ItemContainerEventArgs> Recycled;

		public ItemContainerInfo Materialize(int index, object item)
		{
			if (_containers == null)
				_containers = new SortedDictionary<int, ItemContainerInfo>();

			// If we call Materialize again for whatever reason, let's do better than the default ICG which will
			// throw an error. Instead let's return the already created item
			if (_containers.TryGetValue(index, out var existing))
			{
				if (existing.Item != item)
					throw new InvalidOperationException("Trying to create a container at an existing index with a different item");

				return existing;
			}

			IControl container = _owner.IsItemItsOwnContainerCore(item) ? item as IControl :
				_recyclePool != null && _recyclePool.Count > 0 ? _recyclePool.Dequeue() : _owner.GetContainerForItemCore();

			_owner.PrepareItemContainerCore(container, item);

			var ici = new ItemContainerInfo(container, item, index);

			_containers.Add(index, ici);

			Materialized?.Invoke(this, new ItemContainerEventArgs(ici));

			return ici;
		}

		public IEnumerable<ItemContainerInfo> Dematerialize(int startingIndex, int count)
		{
			var result = new List<ItemContainerInfo>();

			if (_recyclePool == null)
				_recyclePool = new Queue<IControl>();

			for (int i = startingIndex; i < startingIndex + count; i++)
			{
				_owner.ClearItemContainerCore(_containers[i].ContainerControl, _containers[i].Item);
				_recyclePool.Enqueue(_containers[i].ContainerControl);
				result.Add(_containers[i]);
				_containers.Remove(i);
			}

			Dematerialized?.Invoke(this, new ItemContainerEventArgs(startingIndex, result));

			return result;
		}

		public void InsertSpace(int index, int count)
		{
			if (count <= 0)
				return;

			var toMove = _containers.Where(x => x.Key >= index)
				.OrderByDescending(x => x.Key)
				.ToArray();

			for (int i = 0; i < toMove.Length; i++)
			{
				_containers.Remove(toMove[i].Key);
				toMove[i].Value.Index += count;
				_containers.Add(toMove[i].Value.Index, toMove[i].Value);
			}
		}

		public IEnumerable<ItemContainerInfo> RemoveRange(int startingIndex, int count)
		{
			if (count <= 0)
				return Enumerable.Empty<ItemContainerInfo>();

			if (_recyclePool == null)
				_recyclePool = new Queue<IControl>();

			var result = new List<ItemContainerInfo>();

			for (int i = startingIndex; i < startingIndex + count; i++)
			{
				if (_containers.TryGetValue(i, out var found))
					result.Add(found);

				_owner.ClearItemContainerCore(_containers[i].ContainerControl, _containers[i].Item);
				_recyclePool.Enqueue(_containers[i].ContainerControl);

				_containers.Remove(i);
			}

			var toMove = _containers.Where(x => x.Key >= startingIndex)
				.OrderBy(x => x.Key).ToArray();

			for (int i = 0; i < toMove.Length; i++)
			{
				_containers.Remove(toMove[i].Key);
				toMove[i].Value.Index -= count;
				_containers.Add(toMove[i].Value.Index, toMove[i].Value);
			}

			Dematerialized?.Invoke(this, new ItemContainerEventArgs(startingIndex, result));

			return result;
		}

		public IEnumerable<ItemContainerInfo> Clear()
		{
			if (_containers == null)
				return Enumerable.Empty<ItemContainerInfo>();

			var result = Containers.ToArray();
			_containers.Clear();

			for (int i = 0; i < result.Length; i++)
			{
				_owner.ClearItemContainerCore(result[i].ContainerControl, result[i].Item);
			}

			Dematerialized?.Invoke(this, new ItemContainerEventArgs(0, result));

			return result;
		}

		public IControl ContainerFromIndex(int index) => _containers != null ?
			(_containers.TryGetValue(index, out var result) ? result.ContainerControl : null) : null;

		public int IndexFromContainer(IControl container)
		{
			if (_containers == null)
				return -1;

			foreach (var item in _containers)
			{
				if (item.Value.ContainerControl == container)
					return item.Key;
			}

			return -1;
		}

		public bool TryRecycle(int oldIndex, int newIndex, object item)
		{
			// Recycling logic is inherently built into the ListViewBase & this impl of the ICG
			// so a dedicated method really isn't necessary
			throw new NotImplementedException();
		}

		private ListViewBase _owner;
		private Queue<IControl> _recyclePool;
		private SortedDictionary<int, ItemContainerInfo> _containers;
	}
}
