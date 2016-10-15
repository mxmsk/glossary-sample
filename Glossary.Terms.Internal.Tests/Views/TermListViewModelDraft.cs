using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Glossary.Terms.Services;

namespace Glossary.Terms.Views
{
	/// <summary>
	/// Provides a way to test features hidden by <see cref="TermListViewModel"/> encapsulation.
	/// </summary>
	internal sealed class TermListViewModelDraft : TermListViewModel
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TermListViewModelDraft"/> class.
		/// </summary>
		public TermListViewModelDraft()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TermListViewModelDraft"/> class.
		/// </summary>
		/// <param name="termsService">A service that allows to access a storage of glossary terms.</param>
		/// <param name="editViewModel">A model of a view to edit terms.</param>
		public TermListViewModelDraft(ITermsService termsService, ITermEditViewModel editViewModel)
			: base(termsService, editViewModel)
		{
		}

		/// <summary>
		/// Enters busy mode taking nested calls into account.
		/// </summary>
		public void InvokeEnterBusyMode()
		{
			base.EnterBusyMode();
		}

		/// <summary>
		/// Exits busy mode if it is last.
		/// </summary>
		public void InvokeExitBusyMode()
		{
			base.ExitBusyMode();
		}

		/// <summary>
		/// Fills notification with not-null value.
		/// </summary>
		public void FillNotification()
		{
			this.Notification = typeof(TermListViewModelDraft).ToString();
		}
	}
}
