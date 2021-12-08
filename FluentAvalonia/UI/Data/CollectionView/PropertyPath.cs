using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace FluentAvalonia.UI.Data
{
	internal class PropertyPath
	{
		public PropertyPath(string path)
		{
			_path = !string.IsNullOrEmpty(path) ? path : throw new ArgumentNullException(nameof(path));
		}

		public string Path => _path;

		public object ResolvePath(object item)
		{
			item = item ?? throw new ArgumentNullException(nameof(item));

			if (_propInfo == null || _itemsPathResolver == null)
			{
				_propInfo = item.GetType().GetNestedProperty(_path, out Exception ex);
				if (ex != null)
					throw ex;
				if (_propInfo == null)
					throw new Exception($"Property path {_path} not found on object of type {item.GetType()}");

				_itemsPathResolver = x => _propInfo.GetGetMethod().Invoke(x, null);
			}

			return _itemsPathResolver(item);
		}

		private string _path;
		private PropertyInfo _propInfo;
		private Func<object, object> _itemsPathResolver;
	}

	// Taken from Avalonia
	internal static class ReflectionHelpers
	{
		internal const char LeftIndexerToken = '[';
		internal const char PropertyNameSeparator = '.';
		internal const char RightIndexerToken = ']';

		internal static PropertyInfo GetNestedProperty(this Type parentType, string propertyPath, out Exception exception)
		{
			exception = null;
			if (parentType == null || string.IsNullOrEmpty(propertyPath))
			{
				return null;
			}

			Type type = parentType;
			PropertyInfo propertyInfo = null;
			List<string> propertyNames = SplitPropertyPath(propertyPath);
			for (int i = 0; i < propertyNames.Count; i++)
			{
				// if we can't find the property or it is not of the correct type,
				// treat it as a null value
				propertyInfo = type.GetPropertyOrIndexer(propertyNames[i], out object[] index);
				if (propertyInfo == null)
				{
					return null;
				}

				if (!propertyInfo.CanRead)
				{
					exception =
						new InvalidOperationException(
							$"The property named '{propertyNames[i]}' on type '{type.GetTypeName()}' cannot be read.");

					return null;
				}

				type = propertyInfo.PropertyType.GetNonNullableType();
			}

			return propertyInfo;
		}

		internal static PropertyInfo GetPropertyOrIndexer(this Type type, string propertyPath, out object[] index)
		{
			index = null;
			// Return the default value of GetProperty if the first character is not an indexer token.
			if (string.IsNullOrEmpty(propertyPath) || propertyPath[0] != LeftIndexerToken)
			{
				var property = type.GetProperty(propertyPath);
				if (property != null)
				{
					return property;
				}

				// GetProperty does not return inherited interface properties,
				// so we need to enumerate them manually.
				if (type.IsInterface)
				{
					foreach (var typeInterface in type.GetInterfaces())
					{
						property = type.GetProperty(propertyPath);
						if (property != null)
						{
							return property;
						}
					}
				}

				return null;
			}

			if (propertyPath.Length < 2 || propertyPath[propertyPath.Length - 1] != RightIndexerToken)
			{
				// Return null if the indexer does not meet the standard format (i.e. "[x]").
				return null;
			}

			string stringIndex = propertyPath.Substring(1, propertyPath.Length - 2);
			var indexer = FindIndexerInMembers(type.GetDefaultMembers(), stringIndex, out index);
			if (indexer != null)
			{
				// We found the indexer, so return it.
				return indexer;
			}

			var elementType = type.GetElementType();
			if (elementType == null)
			{
				var genericArguments = type.GetGenericArguments();
				if (genericArguments.Length == 1)
				{
					elementType = genericArguments[0];
				}
			}

			if (elementType != null)
			{
				// If the object is of type IList, try to use its default indexer.
				if (typeof(IList<>).MakeGenericType(elementType) is Type genericList
					&& genericList.IsAssignableFrom(type))
				{
					indexer = FindIndexerInMembers(genericList.GetDefaultMembers(), stringIndex, out index);
				}
				if (typeof(IReadOnlyList<>).MakeGenericType(elementType) is Type genericReadOnlyList
				   && genericReadOnlyList.IsAssignableFrom(type))
				{
					indexer = FindIndexerInMembers(genericReadOnlyList.GetDefaultMembers(), stringIndex, out index);
				}
			}

			return indexer;
		}

		private static PropertyInfo FindIndexerInMembers(MemberInfo[] members, string stringIndex, out object[] index)
		{
			index = null;
			ParameterInfo[] parameters;
			PropertyInfo stringIndexer = null;

			foreach (PropertyInfo pi in members)
			{
				if (pi == null)
				{
					continue;
				}

				// Only a single parameter is supported and it must be a string or Int32 value.
				parameters = pi.GetIndexParameters();
				if (parameters.Length > 1)
				{
					continue;
				}

				if (parameters[0].ParameterType == typeof(int))
				{
					int intIndex = -1;
					if (Int32.TryParse(stringIndex.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out intIndex))
					{
						index = new object[] { intIndex };
						return pi;
					}
				}

				// If string indexer is found save it, in case there is an int indexer.
				if (parameters[0].ParameterType == typeof(string))
				{
					index = new object[] { stringIndex };
					stringIndexer = pi;
				}
			}

			return stringIndexer;
		}

		internal static string GetTypeName(this Type type)
		{
			Type baseType = type.GetNonNullableType();
			string s = baseType.Name;
			if (type != baseType)
			{
				s += '?';
			}
			return s;
		}

		internal static Type GetNonNullableType(this Type type)
		{
			if (IsNullableType(type))
			{
				return type.GetGenericArguments()[0];
			}
			return type;
		}

		internal static bool IsNullableType(this Type type)
		{
			return (((type != null) && type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
		}

		internal static List<string> SplitPropertyPath(string propertyPath)
		{
			List<string> propertyPaths = new List<string>();
			if (!string.IsNullOrEmpty(propertyPath))
			{
				int startIndex = 0;
				for (int index = 0; index < propertyPath.Length; index++)
				{
					if (propertyPath[index] == PropertyNameSeparator)
					{
						propertyPaths.Add(propertyPath.Substring(startIndex, index - startIndex));
						startIndex = index + 1;
					}
					else if (startIndex != index && propertyPath[index] == LeftIndexerToken)
					{
						propertyPaths.Add(propertyPath.Substring(startIndex, index - startIndex));
						startIndex = index;
					}
					else if (index == propertyPath.Length - 1)
					{
						propertyPaths.Add(propertyPath.Substring(startIndex));
					}
				}
			}
			return propertyPaths;
		}
	}
}
