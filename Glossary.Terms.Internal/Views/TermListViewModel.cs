using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

using Glossary.Data;
using Glossary.Extensions;
using Glossary.Input;
using Glossary.Terms.Properties;
using Glossary.Terms.Services;

namespace Glossary.Terms.Views
{
	/// <summary>
	/// Represents a model of a view that manages glossary terms.
	/// </summary>
	internal class TermListViewModel : NotificationObject, ITermListViewModel
	{
		/// <summary>
		/// Service that allows to access a storage of glossary terms. 
		/// </summary>
		private readonly ITermsService _termsService;

		/// <summary>
		/// <see cref="SynchronizationContext"/> to use to update <see cref="Terms"/>.
		/// </summary>
		private readonly SynchronizationContext _termsViewContext;

		/// <summary>
		/// Counts busy mode requests.
		/// </summary>
		private int _busyModeCounter;

		/// <summary>
		/// The term that is currently being edited.
		/// </summary>
		private Term _editingTerm;

		/// <summary>
		/// Indicates that terms service signaled that storage is invalid.
		/// </summary>
		private bool _isInvalidStorage;

		/// <summary>
		/// Serves a constructor to test without a services.
		/// </summary>
		protected TermListViewModel()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TermListViewModel"/> class.
		/// </summary>
		/// <param name="termsService">A service that allows to access a storage of glossary terms.</param>
		/// <param name="editViewModel">A model of a view to edit terms.</param>
		public TermListViewModel(ITermsService termsService, ITermEditViewModel editViewModel)
		{
			if (termsService == null)
			{
				throw new ArgumentNullException("termsService");
			}
			if (editViewModel == null)
			{
				throw new ArgumentNullException("editViewModel");
			}
			this._termsService = termsService;
			this.EditViewModel = editViewModel;

			// Create view of term collection sorted by Name.
			var termsView = new ListCollectionView(this._terms);
			termsView.SortDescriptions.Add(new SortDescription(
				PropertyExpressionHelper.GetName<Term, string>(_ => _.Name),
				ListSortDirection.Ascending));

			this._termsView = termsView;
			this._termsViewContext = new DispatcherSynchronizationContext(termsView.Dispatcher);

			// Create commands.
			this._addTerm = new RelayCommand(
				_ => this.AddTermExecuted(),
				_ => !this.IsBusy && !this.IsEditMode);
			this._editTerm = new RelayCommand(
				p => this.EditTermExecuted(p),
				p => !this.IsBusy && !this.IsEditMode && this._terms.Contains(p));
			this._deleteTerm = new RelayCommand(
				_ => this.DeleteTermExecuted(),
				p => !this.IsBusy && this._editingTerm != null);
			this._accept = new RelayCommand(
				_ => this.AcceptExecuted(),
				_ => !this.IsBusy && this.IsEditMode);
			this._cancel = new RelayCommand(
				_ => this.CancelExecuted(),
				_ => !this.IsBusy && this.IsEditMode);
			this._recreateStorage = new RelayCommand(
				_ => this.RecreateStorageExecuted(),
				_ => this._isInvalidStorage);

			this.StartLoadTerms();
		}

		/// <summary>
		/// Starts loading of terms from a storage.
		/// </summary>
		private void StartLoadTerms()
		{
			Observable.Return(this._termsService)
				.Do(_ => this.EnterBusyMode())
				.SelectMany(s => s.LoadTerms(ThreadPoolScheduler.Instance))
				.ObserveOn(this._termsViewContext)
				.Finally(() => this.ExitBusyMode())
				.Subscribe(
					terms => this._terms.AddRange(terms),
					ex => this.HandleServiceException(ex));
		}

		/// <summary>
		/// Called when <see cref="AddTerm"/> command executed.
		/// </summary>
		private void AddTermExecuted()
		{
			this.EditViewModel.Name = String.Empty;
			this.EditViewModel.Definition = String.Empty;

			this.IsEditMode = true;
		}

		/// <summary>
		/// Called when <see cref="EditTerm"/> command executed.
		/// </summary>
		/// <param name="parameter">Data used by the command.</param>
		private void EditTermExecuted(object parameter)
		{
			this._editingTerm = (Term)parameter;

			this.EditViewModel.Name = this._editingTerm.Name;
			this.EditViewModel.Definition = this._editingTerm.Definition;

			this.IsEditMode = true;
		}

		/// <summary>
		/// Called when <see cref="Accept"/> command executed.
		/// </summary>
		private void AcceptExecuted()
		{
			// If edit model contains error, show notification and return.
			if (!String.IsNullOrEmpty(this.EditViewModel.Error))
			{
				this.Notification = this.EditViewModel.Error;
				return;
			}

			// Start observable to modify storage and apply
			// changes to view when sequence has been completed.
			Observable
				.Return(new Term(this.EditViewModel.Name, this.EditViewModel.Definition))
				.Do(_ => this.EnterBusyMode())
				.SelectMany(
					term =>
					{
						if (this._editingTerm == null)
						{
							return this._termsService.AddTerm(term, ThreadPoolScheduler.Instance);
						}
						else
						{
							return this._termsService.UpdateTerm(this._editingTerm, term, ThreadPoolScheduler.Instance);
						}
					})
				.ObserveOn(this._termsViewContext)
				.Finally(() => this.ExitBusyMode())
				.Subscribe(
					term =>
					{
						if (this._editingTerm != null)
						{
							this._terms.Remove(this._editingTerm);
							this._editingTerm = null;
						}

						this._terms.Add(term);
						this._termsView.MoveCurrentTo(term);

						this.IsEditMode = false;
					},
					ex => this.HandleServiceException(ex));
		}

		/// <summary>
		/// Called when <see cref="Cancel"/> command executed.
		/// </summary>
		private void CancelExecuted()
		{
			this._editingTerm = null;
			this.Notification = null;

			this.IsEditMode = false;
		}

		/// <summary>
		/// Called when <see cref="DeleteTerm"/> command executed.
		/// </summary>
		private void DeleteTermExecuted()
		{
			// Start observable to remove term from storage and apply
			// changes to view when sequence has been completed.

			Observable
				.Return(this._editingTerm)
				.Do(_ => this.EnterBusyMode())
				.SelectMany(term => this._termsService.RemoveTerm(term, ThreadPoolScheduler.Instance))
				.ObserveOn(this._termsViewContext)
				.Finally(() => this.ExitBusyMode())
				.Subscribe(
					term =>
					{
						this._terms.Remove(term);
						this._editingTerm = null;

						this.IsEditMode = false;
					},
					ex => this.HandleServiceException(ex));
		}

		/// <summary>
		/// Called when <see cref="RecreateStorage"/> command executed.
		/// </summary>
		private void RecreateStorageExecuted()
		{
			// Start observable that recreates storage.

			Observable
				.Return(this._terms.ToList())
				.Do(_ => this.EnterBusyMode())
				.SelectMany(terms => this._termsService.RecreateStorage(terms, ThreadPoolScheduler.Instance))
				.ObserveOn(this._termsViewContext)
				.Finally(() => this.ExitBusyMode())
				.Subscribe(_ => { }, ex => this.HandleServiceException(ex));
		}

		/// <summary>
		/// Enters busy mode taking nested calls into account.
		/// </summary>
		protected void EnterBusyMode()
		{
			this._busyModeCounter++;

			if (this._busyModeCounter == 1)
			{
				this.Notification = null;
				this._isInvalidStorage = false;

				this.OnBusyModeChanged();
			}
		}

		/// <summary>
		/// Exits busy mode if it is last.
		/// </summary>
		protected void ExitBusyMode()
		{
			if (this._busyModeCounter > 0)
			{
				this._busyModeCounter--;

				if (this._busyModeCounter == 0)
				{
					this.OnBusyModeChanged();
				}
			}
		}

		/// <summary>
		/// Processes change of the <see cref="IsBusy"/> property value.
		/// </summary>
		private void OnBusyModeChanged()
		{
			CommandManager.InvalidateRequerySuggested();
			this.RaisePropertyChanged<ITermListViewModel, bool>(_ => _.IsBusy);
		}

		/// <summary>
		/// Handles exception raised by the terms service.
		/// </summary>
		/// <param name="ex">Exception raised by the terms service.</param>
		private void HandleServiceException(Exception ex)
		{
			var ise = ex as InvalidTermsStorageException;
			if (ise != null)
			{
				// Build custom notification when storage is invalid.
				this._isInvalidStorage = true;
				this.Notification = Resources.InvalidTermsStorage;
				CommandManager.InvalidateRequerySuggested();

				return;
			}

			this.Notification = ex.Message;
		}

		#region Terms Property

		/// <summary>
		/// Collection of glossary terms.
		/// </summary>
		private readonly ObservableCollection<Term> _terms = new ObservableCollection<Term>();

		/// <summary>
		/// Represents collection of glossary terms in alphabet order.
		/// </summary>
		private readonly ICollectionView _termsView;

		/// <summary>
		/// Gets the list of terms.
		/// </summary>
		public ICollectionView Terms
		{
			get
			{
				return this._termsView;
			}
		}

		#endregion

		#region IsBusy Property

		/// <summary>
		/// Gets the value indicating whether a view is busy.
		/// </summary>
		public bool IsBusy
		{
			get
			{
				return this._busyModeCounter > 0;
			}
		}

		#endregion

		#region IsEditMode Property

		/// <summary>
		/// Indicates that edit mode is active.
		/// </summary>
		private bool _isEditMode;

		/// <summary>
		/// Gets the value indicating whether edit mode is active.
		/// </summary>
		public bool IsEditMode
		{
			get
			{
				return this._isEditMode;
			}
			private set
			{
				if (this._isEditMode != value)
				{
					this._isEditMode = value;
					this.RaisePropertyChanged<ITermListViewModel, bool>(_ => _.IsEditMode);

					CommandManager.InvalidateRequerySuggested();
				}
			}
		}

		#endregion

		#region EditViewModel Property

		/// <summary>
		/// Gets the model of view used to edit a term.
		/// </summary>
		public ITermEditViewModel EditViewModel
		{
			get;
			private set;
		}

		#endregion

		#region Notification Property

		/// <summary>
		/// Contains notification to be displayed for the user.
		/// </summary>
		private string _notification;

		/// <summary>
		/// Gets the notification to be displayed for the user.
		/// </summary>
		public string Notification
		{
			get
			{
				return this._notification;
			}
			protected set
			{
				if (this._notification != value)
				{
					this._notification = value;
					this.RaisePropertyChanged<ITermListViewModel, string>(_ => _.Notification);
				}
			}
		}

		#endregion

		#region AddTerm Command

		/// <summary>
		/// Command that allows to add a new term.
		/// </summary>
		private readonly RelayCommand _addTerm;

		/// <summary>
		/// Gets the command that allows to add a new term.
		/// </summary>
		public ICommand AddTerm
		{
			get
			{
				return this._addTerm;
			}
		}

		#endregion

		#region EditTerm Command

		/// <summary>
		/// Command that allows to edit term.
		/// </summary>
		private readonly RelayCommand _editTerm;

		/// <summary>
		/// Gets the command that allows to edit term.
		/// </summary>
		public ICommand EditTerm
		{
			get
			{
				return this._editTerm;
			}
		}

		#endregion

		#region DeleteTerm Command

		/// <summary>
		/// Command that allows to delete term.
		/// </summary>
		private readonly RelayCommand _deleteTerm;

		/// <summary>
		/// Gets the command that allows to delete term.
		/// </summary>
		public ICommand DeleteTerm
		{
			get
			{
				return this._deleteTerm;
			}
		}

		#endregion

		#region Accept Command

		/// <summary>
		/// Command that accepts changes.
		/// </summary>
		private readonly RelayCommand _accept;

		/// <summary>
		/// Gets the command that accepts changes.
		/// </summary>
		public ICommand Accept
		{
			get
			{
				return this._accept;
			}
		}

		#endregion

		#region Cancel Command

		/// <summary>
		/// Command that rejects changes.
		/// </summary>
		private readonly RelayCommand _cancel;

		/// <summary>
		/// Gets the command that rejects changes.
		/// </summary>
		public ICommand Cancel
		{
			get
			{
				return this._cancel;
			}
		}

		#endregion

		#region RecreateStorage Command

		/// <summary>
		/// Command that recreates terms storage.
		/// </summary>
		private readonly RelayCommand _recreateStorage;

		/// <summary>
		/// Gets the command that recreates terms storage.
		/// </summary>
		public ICommand RecreateStorage
		{
			get
			{
				return this._recreateStorage;
			}
		}

		#endregion
	}
}
