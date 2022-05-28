using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Logging;
using System.Collections.Generic;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Element factory for the ItemsRepeaters in a NavigationView
    /// </summary>
    internal class NavigationViewItemsFactory : ElementFactory
    {
        public NavigationViewItemBase SettingsItem { set => _settingsItem = value; }

        public void UserElementFactory(object newValue)
        {
            _itemTemplateWrapper = newValue as IElementFactory;
            if (_itemTemplateWrapper == null)
            {
                // ItemTemplate set does not implement IElementFactoryShim. We also 
                // want to support DataTemplate and DataTemplateSelectors automagically.
                if (newValue is IDataTemplate dt)
                {
                    _itemTemplateWrapper = new ItemTemplateWrapper(dt);
                }
                else if (newValue is DataTemplateSelector dts)
                {
                    _itemTemplateWrapper = new ItemTemplateWrapper(dts);
                }
            }

            _navViewPool = new List<NavigationViewItem>(4);
        }

        // Retrieve the element that will be displayed for a specific data item.
        // If the resolved element is not derived from NavigationViewItemBase, wrap in a NavigationViewItem before returning.
        protected override IControl GetElementCore(ElementFactoryGetArgs args)
        {
            object newContent = args.Data;
            if (_settingsItem != null && _settingsItem == args.Data)
            {
				//This is the settings item, return it directly
				return args.Data as NavigationViewItem;
            }

            if (_itemTemplateWrapper != null)
            {
                newContent = _itemTemplateWrapper.GetElement(args);
            }

            // Element is already of expected type, just return it
            if (newContent is NavigationViewItemBase nvib)
            {
                return nvib;
            }

			//If no template is provided _navViewPool will never initialize
			//check here in case
			if (_navViewPool == null)
				_navViewPool = new List<NavigationViewItem>();

            // Get or create a wrapping container for the data
            NavigationViewItem nvi;
            if (_navViewPool.Count > 0)
            {
                nvi = _navViewPool[_navViewPool.Count - 1];
                _navViewPool.RemoveAt(_navViewPool.Count - 1);
            }
            else
            {
                nvi = new NavigationViewItem();
            }

            nvi.CreatedByNavigationViewItemsFactory = true;

            if (_itemTemplateWrapper != null)
            {
                if (_itemTemplateWrapper is ItemTemplateWrapper itw)
                {
                    var tempArgs = new ElementFactoryRecycleArgs();
                    tempArgs.Element = newContent as IControl;
                    _itemTemplateWrapper.RecycleElement(tempArgs);

                    nvi.Content = args.Data;
                    nvi.ContentTemplate = itw;
                    return nvi;
                }
            }

            nvi.Content = newContent;
            return nvi;
        }

        protected override void RecycleElementCore(ElementFactoryRecycleArgs args)
        {
            if (args.Element != null)
            {
                if (args.Element is NavigationViewItem nvi)
                {
                    // Check whether we wrapped the element in a NavigationViewItem ourselves.
                    // If yes, we are responsible for recycling it.
                    if (nvi.CreatedByNavigationViewItemsFactory)
                    {
                        nvi.CreatedByNavigationViewItemsFactory = false;
                        UnlinkElementFromParent(args);
                        args.Element = null;

                        _navViewPool.Add(nvi);

                        // Retrieve the proper element that requires recycling for a user defined item template
                        // and update the args correspondingly
                        if (_itemTemplateWrapper != null)
                        {
                            // TODO: Retrieve the element and add to the args
                        }
                    }
                }

                // Do not recycle SettingsItem
                bool isSettingsItem = _settingsItem != null && _settingsItem == args.Element;

                if (_itemTemplateWrapper != null && !isSettingsItem)
                {
                    _itemTemplateWrapper.RecycleElement(args);
                }
                else
                {
                    UnlinkElementFromParent(args);
                }
            }
        }

        private void UnlinkElementFromParent(ElementFactoryRecycleArgs args)
        {
            // We want to unlink the containers from the parent repeater
            // in case we are required to move it to a different repeater.
            if (args.Parent is Panel p)
            {
                p.Children.Remove(args.Element);
            }
        }

        private IElementFactory _itemTemplateWrapper;
        private NavigationViewItemBase _settingsItem;
        private List<NavigationViewItem> _navViewPool;
    }

    
    internal class ItemTemplateWrapper : IElementFactory
    {
		// Internal property to RecyclePool, we'll expose here
		public static readonly AttachedProperty<IDataTemplate> OriginTemplateProperty =
			AvaloniaProperty.RegisterAttached<ItemTemplateWrapper, IControl, IDataTemplate>("OriginTemplate");

        private readonly IDataTemplate _dataTemplate;
		private readonly DataTemplateSelector _dataTemplateSelector;

        public ItemTemplateWrapper(IDataTemplate dataTemplate) => _dataTemplate = dataTemplate;

		public ItemTemplateWrapper(DataTemplateSelector dts) => _dataTemplateSelector = dts;

		// These can be safely ignored since this is internal & we only call
		// GetElement and RecycleElement
        public IControl Build(object param) => null;
		public bool Match(object data) => false;

        public IControl GetElement(ElementFactoryGetArgs args)
        {
			var selectedTemplate = _dataTemplate ?? _dataTemplateSelector.SelectTemplate(args.Data);

			// Check if selected template we got is valid
			if (selectedTemplate == null)
			{
				// Null template, use other SelectTemplate method
				selectedTemplate = _dataTemplateSelector.SelectTemplate(args.Data, null);

				// WinUI errors out here, we'll just use FuncDataTemplate.Default
				if (selectedTemplate == null)
				{
					selectedTemplate = FuncDataTemplate.Default;
					Logger.TryGet(LogEventLevel.Information, "NavigationViewItemsFactory")?
						.Log("", $"No DataTemplate found for type {args.Data.GetType()}. Using default instead");
				}
			}

			var recPool = RecyclePool.GetPoolInstance(selectedTemplate);
			IControl element = null;

			if (recPool != null)
			{
				element = recPool.TryGetElement(string.Empty, args.Parent);
			}
			
			if (element == null)
			{
				// no element was found in recycle pool, create a new element
				element = selectedTemplate.Build(args.Data);

				// Template returned null, so insert empty element to render nothing
				// We shouldn't encounter this here, but just in case
				if (element == null)
				{
					element = new Rectangle();
				}

				element.SetValue(OriginTemplateProperty, selectedTemplate);
			}

			// I believe DataTemplate.LoadContent() in WinUI also applies the DataContext, so we'll do
			// that here. If we don't, for some reason, we can get additional elements in the ItemsRepeater
			// For example, comment out the line below & run the sample app, scroll to the DataBinding
			// NavView example, open dev tools and scope to one of the NVIs to find the parent ItemsRepeater
			// You'll see there are 4 NVIs, which is correct, but you'll see more than one NVISeparator
			// only one is correctly arranged, any additional are arranged offscreen
			// I cannot figure out why this is happening, and I have no idea whether its a bug in WinUI,
			// the Avalonia port, or something I'm doing. This seems to fix it though, so YEET
			element.DataContext = args.Data;
			return element;
		}

		public void RecycleElement(ElementFactoryRecycleArgs args)
        {
			var element = args.Element;
			IDataTemplate selectedTemplate = _dataTemplate ?? element.GetValue<IDataTemplate>(OriginTemplateProperty);

			var recPool = RecyclePool.GetPoolInstance(selectedTemplate);

			if (recPool == null)
			{
				recPool = new RecyclePool();
				RecyclePool.SetPoolInstance(selectedTemplate, recPool);
			}

			recPool.PutElement(args.Element, string.Empty, args.Parent);
        }
    }
}
