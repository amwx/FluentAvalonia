using Avalonia.Collections;
using FluentAvalonia.UI.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FluentAvaloniaTests.DataTests
{
	public class CollectionViewSourceTests
	{
		[Fact]
		public void SettingCVSSourceCreatesICollectionView()
		{
			var cvs = new CollectionViewSource();
			cvs.Source = new AvaloniaList<object>();

			Assert.True(cvs.View is ICollectionView);
		}

		[Fact]
		public void ChangingCVSSourceChangesView()
		{
			var cvs = new CollectionViewSource();
			cvs.Source = new List<object>();

			var firstView = cvs.View;

			cvs.Source = new List<int>();

			Assert.True(firstView != cvs.View);
		}

		[Fact]
		public void GroupedCVSFlattensCollection()
		{
			var groups = CreateGroups();

			var cvs = new CollectionViewSource();
			cvs.Source = groups;
			cvs.IsSourceGrouped = true;

			Assert.Equal(10, cvs.View.Count);
		}

		[Fact]
		public void CVSWithItemsPathResolvesItems()
		{
			var groups = CreateGroupsComplex();

			var cvs = new CollectionViewSource();
			cvs.Source = groups;
			cvs.IsSourceGrouped = true;
			cvs.ItemsPath = "Items";

			Assert.NotNull(cvs.View);
			Assert.Equal(10, cvs.View.Count);
		}

		[Fact]
		public void CollectionChangesToCVSGroupItemsAreProperlyReflected()
		{
			var groups = CreateGroups();

			var cvs = new CollectionViewSource();
			cvs.Source = groups;
			cvs.IsSourceGrouped = true;

			// Editing the View of the CVS while grouping is not supported, so all changes must be
			// done to the source collections. These tests ensure the CVS is correctly updated

			var ct = cvs.View.Count; // Force View creation since it's not created until needed

			var itemToAdd = new TestItem("AddedItem", "Group1");


			// Test adding an item into the first group
			groups[0].Add(itemToAdd);

			// Count should increase by 1
			Assert.Equal(11, cvs.View.Count);

			// Make sure the item is represented in the flattened collection
			Assert.Equal(itemToAdd, cvs.View[groups[0].Count - 1]);

			// Test removing an item
			groups[0].Remove(itemToAdd);

			// Count should return
			Assert.Equal(10, cvs.View.Count);

			//Make sure the item is gone
			Assert.False(cvs.View.Contains(itemToAdd));

			// Test clearing a group - the group itself should remain, but hold no items
			int group0ItemCount = groups[0].Count;
			groups[0].Clear();

			Assert.Equal(10 - group0ItemCount, cvs.View.Count);

			Assert.Equal(groups[0], cvs.View.CollectionGroups[0].Group);

			// Reset everything so we can continue
			groups = CreateGroups();
			cvs.Source = groups;


			// Test replacing an item in a group - we'll work in the second group now to make sure 
			// everything still works

			groups[1][1] = itemToAdd;

			// Count should still be the same
			Assert.Equal(10, cvs.View.Count);

			// Ensure it's in the right place from the CVS perspective
			Assert.Equal(itemToAdd, cvs.View[group0ItemCount + 1]);


			// Finally test moving an item
			groups[1].Move(1, 0);

			// Count should still be the same
			Assert.Equal(10, cvs.View.Count);

			// Ensure it's in the right place from the CVS perspective
			Assert.Equal(itemToAdd, cvs.View[group0ItemCount]);
		}

		[Fact]
		public void ChangesToCVSGroupsAreProperlyReflected()
		{
			bool fired = false;
			NotifyCollectionChangedEventArgs lastArgs = null;
			void OnViewChanged(object sender, NotifyCollectionChangedEventArgs args)
			{
				fired = true;
				lastArgs = args;
			}

			var groups = CreateGroups();

			var cvs = new CollectionViewSource();
			cvs.Source = groups;
			cvs.IsSourceGrouped = true;

			// Ensure view is created
			var ct = cvs.View.Count;

			var emptyGroup = new TestGroup(new List<TestItem>()) { Key = "Empty Group" };
			var groupToAdd = new TestGroup(new List<TestItem>
			{
				new TestItem("NewItem1", "AddedGroup"),
				new TestItem("NewItem2", "AddedGroup"),
				new TestItem("NewItem3", "AddedGroup"),
			})
			{ Key = "AddedGroup" };

			cvs.View.CollectionChanged += OnViewChanged;

			// Add the empty group, collection group should exist, but no collection changed notification should be sent
			groups.Add(emptyGroup);
			Assert.False(fired);
			Assert.Equal(emptyGroup, cvs.View.CollectionGroups[^1].Group);

			// Now add the group with items. We should get a collection changed notification for the 3 new items, and the 
			// item count of the ICollectionView should increase by 3

			groups.Add(groupToAdd);
			Assert.True(fired);
			Assert.Equal(groupToAdd, cvs.View.CollectionGroups[^1].Group);
			Assert.Equal(13, cvs.View.Count);
			Assert.Equal(groupToAdd[^1], cvs.View[^1]);

			// This makes sure the StartingIndex is reflected correctly
			Assert.Equal(10, lastArgs.NewStartingIndex);

			// reset
			fired = false;
			lastArgs = null;
			cvs.View.CollectionChanged -= OnViewChanged;
			groups = CreateGroups();
			cvs.Source = groups;
			cvs.View.CollectionChanged += OnViewChanged;

			// Now test removing a group (we'll remove group 2, which contains 5 items)
			var removedGroup = groups[1];
			groups.RemoveAt(1);
			Assert.True(fired);

			Assert.Equal(5, cvs.View.Count);
			Assert.NotEqual(removedGroup, cvs.View.CollectionGroups[1].Group);

			// Make sure the OldStartingIndex is reflected correctly
			// There are 3 items in "Group1", so OldStartingIndex should be 3
			Assert.Equal(3, lastArgs.OldStartingIndex);

			// reset
			fired = false;
			lastArgs = null;
			cvs.View.CollectionChanged -= OnViewChanged;
			groups = CreateGroups();
			cvs.Source = groups;
			cvs.View.CollectionChanged += OnViewChanged;

			// Clear is straightforward, so we'll skip for now

			// Test replacing a group
			groups[1] = groupToAdd;

			Assert.True(fired);

			Assert.Equal(8, cvs.View.Count);
			Assert.Equal(groupToAdd, cvs.View.CollectionGroups[1].Group);


			// reset
			fired = false;
			lastArgs = null;
			cvs.View.CollectionChanged -= OnViewChanged;
			groups = CreateGroups();
			cvs.Source = groups;
			cvs.View.CollectionChanged += OnViewChanged;

			var group2 = groups[1];
			var group3 = groups[2];

			// move the last group into index 1
			groups.Move(2, 1);

			Assert.True(fired);
			Assert.Equal(8, lastArgs.OldStartingIndex);
			Assert.Equal(3, lastArgs.NewStartingIndex);

			Assert.Equal(10, cvs.View.Count);

			Assert.Equal(group3, cvs.View.CollectionGroups[1].Group);
			Assert.Equal(group2, cvs.View.CollectionGroups[2].Group);
		}

		[Fact]
		public void NoIssuesWithItemsSourceView()
		{
			var groups = CreateGroups();
			var cvs = new CollectionViewSource();
			cvs.Source = groups;
			cvs.IsSourceGrouped = true;

			try
			{
				var isv = new Avalonia.Controls.ItemsSourceView(cvs.View);
				Assert.True(true);
			}
			catch
			{
				Assert.True(false);
			}
		}

		[Fact]
		public void GroupEnumeratorWorks()
		{
			var groups = CreateGroups();
			var cvs = new CollectionViewSource();
			cvs.Source = groups;
			cvs.IsSourceGrouped = true;

			List<string> flattened = new List<string>
			{
				"Item1", "Item5", "Item9",
				"Item2", "Item4", "Item6", "Item8", "Item10",
				"Item3", "Item7"
			};

			int idx = 0;
			foreach (var item in cvs.View)
			{
				Assert.Equal(flattened[idx++], ((TestItem)item).Name);
			}
		}

		[Fact]
		public void CanEditUngroupedCVSButNotGrouped()
		{
			var cvs = new CollectionViewSource();
			cvs.Source = new List<string> { "Test1", "Test2", "Test3" };

			bool threwException = false;
			try
			{
				cvs.View.Add("Added");
			}
			catch
			{
				threwException = true;
			}
			Assert.False(threwException);
			Assert.Equal(4, cvs.View.Count);


			cvs.Source = CreateGroups();
			cvs.IsSourceGrouped = true;

			try
			{
				cvs.View.Add("Added");
			}
			catch
			{
				threwException = true;
			}
			Assert.True(threwException);
			Assert.Equal(10, cvs.View.Count);
		}

		private AvaloniaList<TestGroup> CreateGroups()
		{
			// CHANGES TO THIS MUST BE UPDATED IN ChangesToCVSGroupsAreProperlyReflected() and any place
			// where item Count is expected to be 10!!
			var items = new List<TestItem>
			{
				new TestItem("Item1", "Group1"),
				new TestItem("Item2", "Group2"),
				new TestItem("Item3", "Group3"),
				new TestItem("Item4", "Group2"),
				new TestItem("Item5", "Group1"),
				new TestItem("Item6", "Group2"),
				new TestItem("Item7", "Group3"),
				new TestItem("Item8", "Group2"),
				new TestItem("Item9", "Group1"),
				new TestItem("Item10", "Group2"),
			};

			var query = from item in items
						group item by item.Group into g
						select new TestGroup(g) { Key = g.Key };

			return new AvaloniaList<TestGroup>(query);
		}

		private AvaloniaList<ComplexTestGroup> CreateGroupsComplex()
		{
			var items = new List<TestItem>
			{
				new TestItem("Item1", "Group1"),
				new TestItem("Item2", "Group2"),
				new TestItem("Item3", "Group3"),
				new TestItem("Item4", "Group2"),
				new TestItem("Item5", "Group1"),
				new TestItem("Item6", "Group2"),
				new TestItem("Item7", "Group3"),
				new TestItem("Item8", "Group2"),
				new TestItem("Item9", "Group1"),
				new TestItem("Item10", "Group2"),
			};

			var query = from item in items
						group item by item.Group into g
						select new ComplexTestGroup(g.Key, g);

			return new AvaloniaList<ComplexTestGroup>(query);
		}

	}

	internal class TestItem
	{
		public TestItem(string nm, string g)
		{
			Name = nm;
			Group = g;
		}

		public string Name { get; set; }
		public string Group { get; set; }
	}

	internal class TestGroup : AvaloniaList<TestItem>
	{
		public TestGroup(IEnumerable<TestItem> items)
			: base(items)
		{

		}

		public string Key { get; set; }
	}

	internal class ComplexTestGroup
	{
		public ComplexTestGroup(string h, IEnumerable<TestItem> items)
		{
			Header = h;
			Items = new AvaloniaList<TestItem>(items);
		}

		public string Header { get; set; }
		public AvaloniaList<TestItem> Items { get; set; }
	}
}
