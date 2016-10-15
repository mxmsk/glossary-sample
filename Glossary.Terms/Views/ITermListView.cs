using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glossary.Terms.Views
{
	/// <summary>
	/// Defines a view that manages glossary terms.
	/// </summary>
	public interface ITermListView
	{
		/// <summary>
		/// Gets the model of this view.
		/// </summary>
		ITermListViewModel Model
		{
			get;
		}
	}
}
