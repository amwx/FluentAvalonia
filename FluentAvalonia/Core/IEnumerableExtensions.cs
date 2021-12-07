using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentAvalonia.Core
{
    public static class IEnumerableExtensions
    {
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

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
    (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

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
