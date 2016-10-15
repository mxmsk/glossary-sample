using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glossary.Terms.Views
{
	/// <summary>
	/// Defines a model of a view that allows to edit a term.
	/// </summary>
	public interface ITermEditViewModel : IDataErrorInfo
	{
		/// <summary>
		/// Gets or sets the name of term.
		/// </summary>
		string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the definition of term.
		/// </summary>
		string Definition
		{
			get;
			set;
		}
	}
}
