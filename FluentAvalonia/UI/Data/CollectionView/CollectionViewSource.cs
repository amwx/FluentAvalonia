using Avalonia;
using System.Collections;

namespace FluentAvalonia.UI.Data
{
	public class CollectionViewSource : AvaloniaObject
	{
		public static readonly DirectProperty<CollectionViewSource, bool> IsSourceGroupedProperty =
			AvaloniaProperty.RegisterDirect<CollectionViewSource, bool>(nameof(IsSourceGrouped),
				x => x.IsSourceGrouped, (x, v) => x.IsSourceGrouped = v);

		public static readonly DirectProperty<CollectionViewSource, string> ItemsPathProperty =
		 AvaloniaProperty.RegisterDirect<CollectionViewSource, string>(nameof(ItemsPath),
			 x => x.ItemsPath, (x, v) => x.ItemsPath = v);

		public static readonly DirectProperty<CollectionViewSource, IEnumerable> SourceProperty =
		 AvaloniaProperty.RegisterDirect<CollectionViewSource, IEnumerable>(nameof(Source),
			 x => x.Source, (x, v) => x.Source = v);

		public static readonly DirectProperty<CollectionViewSource, ICollectionView> ViewProperty =
		 AvaloniaProperty.RegisterDirect<CollectionViewSource, ICollectionView>(nameof(View),
			 x => x.View);

		public bool IsSourceGrouped
		{
			get => _isSourceGrouped;
			set
			{
				if (SetAndRaise(IsSourceGroupedProperty, ref _isSourceGrouped, value))
				{
					UpdateView();
				}
			}
		}

		public string ItemsPath
		{
			get => _itemsPath;
			set
			{
				if (SetAndRaise(ItemsPathProperty, ref _itemsPath, value))
				{
					UpdateView();
				}
			}
		}

		public IEnumerable Source
		{
			get => _source;
			set
			{
				if (SetAndRaise(SourceProperty, ref _source, value))
				{
					UpdateView();
				}
			}
		}

		public ICollectionView View
		{
			get
			{
				if (_view == null)
				{
					UpdateView();
				}

				return _view;
			}
			private set => SetAndRaise(ViewProperty, ref _view, value);
		}

		private void UpdateView()
		{
			if (_source == null)
				return;

			if (Source is IEnumerable ie)
			{
				View = new GroupedDataCollectionView(ie, _isSourceGrouped, _itemsPath);
			}
			else if (Source is ICollectionViewFactory factory)
			{
				View = factory.CreateView();
			}
			else
			{
				View = null;
			}
		}

		private bool _isSourceGrouped;
		private string _itemsPath;
		private IEnumerable _source;
		private ICollectionView _view;
	}
}
