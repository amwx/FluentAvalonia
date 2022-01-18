using System;
using System.Collections;
using System.Linq;

namespace FluentAvalonia.Core
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Gets the item count of the IEnumerable
        /// </summary>
        public static int Count(this IEnumerable items)
        {
            if (items == null)
                return 0;
            
            if(items is ICollection collec)
            {
                return collec.Count;
            }
            else
            {
                return Enumerable.Count(items.Cast<object>());
            }
        }

        /// <summary>
        /// Gets the index of an item from an IEnumerable
        /// </summary>
        public static int IndexOf(this IEnumerable items, object item)
        {
            var list = items as IList;

            if (list != null)
            {
                return list.IndexOf(item);
            }
            else
            {
                int index = 0;

                foreach (var i in items)
                {
                    if (ReferenceEquals(i, item))
                    {
                        return index;
                    }

                    ++index;
                }

                return -1;
            }
        }

        /// <summary>
        /// Retreives the element at the specified index from the IEnumerable
        /// </summary>
        /// <param name="items"></param>
        /// <param name="reqIndex"></param>
        /// <returns></returns>
        public static object ElementAt(this IEnumerable items, int reqIndex)
        {
            if (items.Count() == 0)
                return null;
            
            if(items is IList list)
            {
                return list[reqIndex];
            }
            else
            {
                return Enumerable.ElementAt(items.Cast<object>(), reqIndex);
            }

        }

        /// <summary>
        /// Checks of the IEnumerable contains the given item
        /// </summary>
		public static bool Contains(this IEnumerable items, object item)
		{
			if (items is IList list)
			{
				return list.Contains(item);
			}
			else
			{
				foreach (var i in items)
				{
					if (ReferenceEquals(i, item))
					{
						return true;
					}
				}

				return false;
			}
		}
	}
}
