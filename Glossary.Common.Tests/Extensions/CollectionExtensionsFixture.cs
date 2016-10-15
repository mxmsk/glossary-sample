using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Glossary.Extensions
{
	/// <summary>
	/// Contains unit tests of the <see cref="CollectionExtensions"/> class.
	/// </summary>
	[TestClass]
	public class CollectionExtensionsFixture
	{
		/// <summary>
		/// Tests adding of non-empty collection.
		/// </summary>
		[TestMethod]
		public void ShouldAddItems()
		{
			var source = new List<int> { 1, 2, 3 };
			var add = new List<int> { 4, 5, 6 };

			CollectionExtensions.AddRange(source, add);

			CollectionAssert.IsSubsetOf(add, source);
		}

		/// <summary>
		/// Tests adding of empty collection.
		/// </summary>
		[TestMethod]
		public void ShouldAddNoItems()
		{
			var source = new List<int> { 1, 2, 3 };
			var initial = source.ToList();

			CollectionExtensions.AddRange(source, Enumerable.Empty<int>());

			CollectionAssert.AreEqual(source, initial);
		}

		/// <summary>
		/// Tests that calling on <langword>null</langword> is not supported.
		/// </summary>
		[TestMethod]
		public void ShouldThrowWhenNoSourceSpecified()
		{
			try
			{
				CollectionExtensions.AddRange(null, new int[] { 1, 2, 3});
				Assert.Fail("Allowed to invoke from null");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
				Assert.AreEqual("collection", ((ArgumentNullException)ex).ParamName);
			}
		}

		/// <summary>
		/// Tests that adding of <langword>null</langword> is not supported.
		/// </summary>
		[TestMethod]
		public void ShouldThrowWhenNoCollectionToAddSpecified()
		{
			var source = new List<int> { 1, 2, 3 };
			try
			{
				CollectionExtensions.AddRange(source, null);
				Assert.Fail("Allowed to add from null");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
				Assert.AreEqual("items", ((ArgumentNullException)ex).ParamName);
			}
		}
	}
}
