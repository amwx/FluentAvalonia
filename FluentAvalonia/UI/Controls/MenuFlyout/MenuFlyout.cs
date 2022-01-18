using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections;

namespace FluentAvalonia.UI.Controls
{
	/// <summary>
	/// Represents a flyout that displays a menu of commands.
	/// </summary>
	public class MenuFlyout : FlyoutBase
	{
		public MenuFlyout()
		{
			_items = new AvaloniaList<object>();
		}

		/// <summary>
		/// Defines the <see cref="Items"/> property
		/// </summary>
		public static readonly DirectProperty<MenuFlyout, IEnumerable> ItemsProperty =
			ItemsControl.ItemsProperty.AddOwner<MenuFlyout>(x => x.Items,
				(x, v) => x.Items = v);

		/// <summary>
		/// Defines the <see cref="ItemTemplate"/> property
		/// </summary>
		public static readonly StyledProperty<IDataTemplate> ItemTemplateProperty =
			ItemsControl.ItemTemplateProperty.AddOwner<MenuFlyout>();

		/// <summary>
		/// Gets or sets the items of the MenuFlyout
		/// </summary>
		[Content]
		public IEnumerable Items
		{
			get => _items;
			set => SetAndRaise(ItemsProperty, ref _items, value);
		}

		/// <summary>
		/// Gets or sets the template used for the items
		/// </summary>
		public IDataTemplate ItemTemplate
		{
			get => GetValue(ItemTemplateProperty);
			set => SetValue(ItemTemplateProperty, value);
		}

		/// <summary>
		/// Sets the Classes used for styling the MenuFlyoutPresenter. This property
		/// takes the place of WinUI's MenuFlyoutPresenterStyle
		/// </summary>
		public Classes FlyoutPresenterClasses => _classes ??= new Classes();

		protected override Control CreatePresenter()
		{
			return new MenuFlyoutPresenter
			{
				[!ItemsControl.ItemsProperty] = this[!ItemsProperty],
				[!ItemsControl.ItemTemplateProperty] = this[!ItemTemplateProperty]
			};
		}

		protected override void OnOpened()
		{
			if (_classes != null)
			{
				SetPresenterClasses(Popup.Child, FlyoutPresenterClasses);
			}
			base.OnOpened();
		}

		private static void SetPresenterClasses(IControl presenter, Classes classes)
		{
			//Remove any classes no longer in use, ignoring pseudoclasses
			for (int i = presenter.Classes.Count - 1; i >= 0; i--)
			{
				if (!classes.Contains(presenter.Classes[i]) &&
					!presenter.Classes[i].Contains(":"))
				{
					presenter.Classes.RemoveAt(i);
				}
			}

			//Add new classes
			presenter.Classes.AddRange(classes);
		}

		private Classes _classes;
		private IEnumerable _items;
	}
}
