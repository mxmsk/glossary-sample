using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Glossary.Terms
{
	/// <summary>
	/// Contains unit tests of the <see cref="Term"/> class.
	/// </summary>
	[TestClass]
	public class TermFixture
	{
		/// <summary>
		/// Tests that the constructor of the <see cref="Term"/> class
		/// successfully initialize properties.
		/// </summary>
		[TestMethod]
		public void ConstructorShouldAssignProperties()
		{
			const string Name = "Term1";
			const string Definition = "Def1";

			var term = new Term(Name, Definition);

			Assert.AreEqual(Name, term.Name);
			Assert.AreEqual(Definition, term.Definition);
		}
	}
}
