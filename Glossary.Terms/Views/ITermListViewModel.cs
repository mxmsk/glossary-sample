using System;
using System.ComponentModel;
using System.Collections;
using System.Linq;
using System.Windows.Input;

namespace Glossary.Terms.Views
{
	/// <summary>
	/// Defines a model of a view that manages glossary terms.
	/// </summary>
	public interface ITermListViewModel
	{
		/// <summary>
		/// Gets the list of terms.
		/// </summary>
		ICollectionView Terms
		{
			get;
		}

		/// <summary>
		/// Gets the value indicating whether a view is busy.
		/// </summary>
		bool IsBusy
		{
			get;
		}

		/// <summary>
		/// Gets the value indicating whether edit mode is active.
		/// </summary>
		bool IsEditMode
		{
			get;
		}

		/// <summary>
		/// Gets the notification to be displayed for the user.
		/// </summary>
		string Notification
		{
			get;
		}

		/// <summary>
		/// Gets the model of view used to edit a term.
		/// </summary>
		ITermEditViewModel EditViewModel
		{
			get;
		}

		/// <summary>
		/// Gets the command that allows to add a new term.
		/// </summary>
		ICommand AddTerm
		{
			get;
		}

		/// <summary>
		/// Gets the command that allows to edit term.
		/// </summary>
		ICommand EditTerm
		{
			get;
		}

		/// <summary>
		/// Gets the command that allows to delete term.
		/// </summary>
		ICommand DeleteTerm
		{
			get;
		}

		/// <summary>
		/// Gets the command that accepts changes.
		/// </summary>
		ICommand Accept
		{
			get;
		}

		/// <summary>
		/// Gets the command that rejects changes.
		/// </summary>
		ICommand Cancel
		{
			get;
		}

		/// <summary>
		/// Gets the command that recreates terms storage.
		/// </summary>
		ICommand RecreateStorage
		{
			get;
		}
	}
}
