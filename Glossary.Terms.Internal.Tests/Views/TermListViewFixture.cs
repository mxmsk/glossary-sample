using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Glossary.Terms.Views
{
	/// <summary>
	/// Contains unit tests of the <see cref="TermListView"/> class.
	/// </summary>
	[TestClass]
	public class TermListViewFixture
	{
		/// <summary>
		/// Tests that the constructor of the <see cref="TermListView"/> class
		/// successfully sets view model.
		/// </summary>
		[TestMethod]
		public void ConstructorShouldSetModel()
		{
			var vm = new Mock<ITermListViewModel>();
			vm.SetupGet(m => m.Terms).Returns(new CollectionView(new Term[] { }));
			var v = new TermListView(vm.Object);

			Assert.AreEqual(vm.Object, v.Model);
		}

		/// <summary>
		/// Tests that the constructor of the <see cref="TermListView"/> class
		/// successfully sets view model as DataContext.
		/// </summary>
		[TestMethod]
		public void ConstructorShouldSetDataContext()
		{
			var vm = new Mock<ITermListViewModel>();
			vm.SetupGet(m => m.Terms).Returns(new CollectionView(new Term[] { }));
			var v = new TermListView(vm.Object);

			Assert.AreEqual(vm.Object, v.DataContext);
		}
	}
}
