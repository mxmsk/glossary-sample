using System;
using System.Collections.Generic;

namespace Glossary.Extensions
{
	/// <summary>
	/// Contains extension methods for the <see cref="ICollection&lt;T&gt;"/> class.
	/// </summary>
	public static class CollectionExtensions
	{
		/// <summary>
		/// Adds a list of an items to the <see cref="ICollection&lt;T&gt;"/>.
		/// </summary>
		/// <typeparam name="T">Type of items to add.</typeparam>
		/// <param name="collection">A collection to add items to.</param>
		/// <param name="items">A list of an items to add to the <see cref="ICollection&lt;T&gt;"/>.</param>
		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}

			foreach (var item in items)
			{
				collection.Add(item);
			}
		}
	}
}
