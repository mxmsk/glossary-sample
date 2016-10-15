using System;
using System.ComponentModel;
using System.Linq.Expressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Glossary.Data;

namespace Glossary.Terms.Views
{
	/// <summary>
	/// Contains unit tests of the <see cref="TermEditViewModel"/> class.
	/// </summary>
	[TestClass]
	public partial class TermEditViewModelFixture
	{
		/// <summary>
		/// Tests that an instance of the <see cref="TermEditViewModel"/> sets
		/// error info when term name is null.
		/// </summary>
		[TestMethod]
		public void ModelSetsErrorWhenTermNameHasNoName()
		{
			var nameProperty = PropertyExpressionHelper.GetName<ITermEditViewModel, string>(_ => _.Name);

			var model = new TermEditViewModel();
			TermEditViewModelFixture.AssertDataErrorInfo(model, true);

			model.Name = "term0";
			TermEditViewModelFixture.AssertDataErrorInfo(model, false);

			model.Name = null;
			TermEditViewModelFixture.AssertDataErrorInfo(model, true);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermEditViewModel"/> sets
		/// error info when term name is whitespace.
		/// </summary>
		[TestMethod]
		public void ModelSetsErrorWhenTermNameIsWhitespace()
		{
			var nameProperty = PropertyExpressionHelper.GetName<ITermEditViewModel, string>(_ => _.Name);

			var model = new TermEditViewModel();
			TermEditViewModelFixture.AssertDataErrorInfo(model, true);

			model.Name = "term0";
			TermEditViewModelFixture.AssertDataErrorInfo(model, false);

			model.Name = " ";
			TermEditViewModelFixture.AssertDataErrorInfo(model, true);
		}

		#region Helpers

		/// <summary>
		/// Asserts <see cref="IDataErrorInfo"/> properties of the specified <see cref="TermEditViewModel"/>.
		/// </summary>
		/// <param name="model"><see cref="IDataErrorInfo"/> to assert.</param>
		/// <param name="isError">Indicates that <see cref="IDataErrorInfo"/> properties shouldn't be blank.</param>
		private static void AssertDataErrorInfo(IDataErrorInfo model, bool isError)
		{
			var nameProperty = PropertyExpressionHelper.GetName<ITermEditViewModel, string>(_ => _.Name);

			Assert.IsNotNull(model.Error);
			Assert.IsNotNull(model[nameProperty]);

			if (isError)
			{
				Assert.AreNotEqual(String.Empty, model.Error);
				Assert.AreNotEqual(String.Empty, model[nameProperty]);
			}
			else
			{
				Assert.AreEqual(String.Empty, model.Error);
				Assert.AreEqual(String.Empty, model[nameProperty]);
			}
		}

		#endregion
	}
}
