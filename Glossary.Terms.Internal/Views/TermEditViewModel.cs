using System;
using System.Linq.Expressions;

using Glossary.Data;
using Glossary.Terms.Properties;

namespace Glossary.Terms.Views
{
	/// <summary>
	/// Represents a model of a view that allows to edit a term.
	/// </summary>
	internal sealed class TermEditViewModel : NotificationObject, ITermEditViewModel
	{
		/// <summary>
		/// The name of term.
		/// </summary>
		private string _name;

		/// <summary>
		/// The definition of term.
		/// </summary>
		private string _definition;

		/// <summary>
		/// Gets or sets the name of term.
		/// </summary>
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				if (this._name != value)
				{
					this._name = value;
					this.RaisePropertyChanged<ITermEditViewModel, string>(_ => _.Name);
				}
			}
		}

		/// <summary>
		/// Gets or sets the definition of term.
		/// </summary>
		public string Definition
		{
			get
			{
				return this._definition;
			}
			set
			{
				if (this._definition != value)
				{
					this._definition = value;
					this.RaisePropertyChanged<ITermEditViewModel, string>(_ => _.Definition);
				}
			}
		}

		/// <summary>
		/// Gets an error message indicating what is wrong with this object.
		/// </summary>
		public string Error
		{
			get
			{
				if (String.IsNullOrWhiteSpace(this.Name))
				{
					return Resources.TermEditNameCannotBeBlank;
				}

				return String.Empty;
			}
		}

		/// <summary>
		/// Gets the error message for the property with the given name.
		/// </summary>
		/// <param name="columnName">The name of the property whose error message to get.</param>
		public string this[string columnName]
		{
			get
			{
				var nameColumn = PropertyExpressionHelper.GetName<ITermEditViewModel, string>(_ => _.Name);
				if (nameColumn == columnName)
				{
					return this.Error;
				}

				return String.Empty;
			}
		}
	}
}
