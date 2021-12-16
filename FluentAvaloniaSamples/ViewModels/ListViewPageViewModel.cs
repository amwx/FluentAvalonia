using Avalonia;
using Avalonia.Platform;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentAvaloniaSamples.ViewModels
{
	public class Contact
	{
		public Contact(string firstName, string lastName, string company)
		{
			FirstName = firstName;
			LastName = lastName;
			Company = company;
		}

		public string FirstName { get; }
		public string LastName { get; }

		public string Name => $"{FirstName} {LastName}";

		public string Company { get; }
	}

	public class ContactGroup : List<Contact>
	{
		public ContactGroup(IEnumerable<Contact> items, string groupName)
			:base(items)
		{
			GroupName = groupName;
		}

		public string GroupName { get; }
	}

	public class ListViewPageViewModel : ViewModelBase
	{
		public ListViewPageViewModel()
		{
			using var stream = AvaloniaLocator.Current.GetService<IAssetLoader>().Open(
				new Uri("avares://FluentAvaloniaSamples/Assets/ContactList.txt"));

			List<Contact> contacts = new List<Contact>();

			using (var sr = new StreamReader(stream))
			{
				while(sr.Peek() != -1)
				{
					contacts.Add(new Contact(sr.ReadLine(), sr.ReadLine(), sr.ReadLine()));
				}
			}

			Contacts = new ObservableCollection<Contact>(contacts);

			var query = from item in contacts
						group item by item.LastName.Substring(0, 1).ToUpper() into g
						orderby g.Key
						select new ContactGroup(g, g.Key);

			ContactGroups = new List<ContactGroup>(query);
		}

		public ObservableCollection<Contact> Contacts { get; }

		public List<ContactGroup> ContactGroups { get; }

		public ListViewSelectionMode[] SelectionModes => Enum.GetValues<ListViewSelectionMode>();

		public bool AreStickyHeadersEnabled
		{
			get => _stickyHeaders;
			set => RaiseAndSetIfChanged(ref _stickyHeaders, value);
		}

		private bool _stickyHeaders = true; // This is default true for now, will be default false in future
	}
}
