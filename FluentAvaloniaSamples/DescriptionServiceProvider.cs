using Avalonia;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FluentAvaloniaSamples
{
	public class DescriptionServiceProvider
	{
		private DescriptionServiceProvider()
		{
			XElement xe = XElement.Parse(GetAssemblyResource("FluentAvaloniaInfo.txt"));
			Pages = xe.Elements("ControlPage");
		}

		static DescriptionServiceProvider()
		{
			Instance = new DescriptionServiceProvider();
		}

		public static DescriptionServiceProvider Instance { get; }

		private IEnumerable<XElement> Pages { get; }

		protected string GetAssemblyResource(string name)
		{
			var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
			using (var stream = assets.Open(new Uri($"avares://FluentAvaloniaSamples/DescriptionTexts/{name}")))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		public string GetInfo(string pageName, string itemName, string subItemName = null)
		{
			var page = Pages.Where(x => x.Attribute("Name").Value == pageName).First();
			if (page != null)
			{
				// first check to see if itemName is a tag, such as Header
				var item = page.Element(itemName);
				if (item != null)
				{
					// in this case, no more tags, just return the content:
					return item.Value;
				}
				else
				{
					item = page.Elements("Control").Where(x => x.Attribute("Name").Value == itemName).First();
					if (item != null)
					{
						//in this case, we're searching for UsageNotes, XamlSource, or CSharpSource (subItemName)
						var subItem = item.Element(subItemName);
						if (subItem != null)
						{
							return subItem.Value;
						}
					}
				}
			}

			return string.Empty;
		}
	}
}
