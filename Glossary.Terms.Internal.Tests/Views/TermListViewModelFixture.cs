using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Glossary.Terms.Services;
using Glossary.Terms.Utility;

namespace Glossary.Terms.Views
{
	/// <summary>
	/// Contains unit tests of the <see cref="TermListViewModel"/> class.
	/// </summary>
	[TestClass]
	public partial class TermListViewModelFixture
	{
		/// <summary>
		/// Collection of valid sample terms.
		/// </summary>
		private static ReadOnlyCollection<Term> SampleTerms;

		/// <summary>
		/// <see cref="ITermsService"/> mock that fetch <see cref="SampleTerms"/>.
		/// </summary>
		private Mock<ITermsService> _termsServiceMock;

		/// <summary>
		/// Contains code that must be used before any of the tests have run.
		/// </summary>
		/// <param name="context"><see cref="TestContext"/> that stores information about the current test.</param>
		[ClassInitialize]
		public static void ClassInit(TestContext context)
		{
			TermListViewModelFixture.SampleTerms = new ReadOnlyCollection<Term>(
				new Term[]
				{
					new Term("term0", "def 0"),
					new Term("term1", "def 1"),
					new Term("term2", "def 2"),
				});
		}

		/// <summary>
		/// Contains code to allocate and configure resources needed by all tests in the test class.
		/// </summary>
		[TestInitialize]
		public void TestInit()
		{
			var service = new Mock<ITermsService>();

			service
				.Setup(s => s.LoadTerms(ThreadPoolScheduler.Instance))
				.Returns(() => Observable.Return(TermListViewModelFixture.SampleTerms, Scheduler.Immediate));

			this._termsServiceMock = service;
		}

		#region Busy Mode

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> toggles
		/// <see cref="ITermListViewModel.IsBusy"/> property by calling Enter/ExitBusyMode.
		/// </summary>
		[TestMethod]
		public void EnterExitBusyModeTogglesIsBusy()
		{
			var model = new TermListViewModelDraft();
			Assert.AreEqual(false, model.IsBusy);

			model.InvokeEnterBusyMode();
			Assert.AreEqual(true, model.IsBusy);

			model.InvokeExitBusyMode();
			Assert.AreEqual(false, model.IsBusy);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class allows
		/// to enter nested busy mode.
		/// </summary>
		[TestMethod]
		public void EnterExitBusyModeConsiderNesting()
		{
			var model = new TermListViewModelDraft();

			var busyModeChangeCount = 0;
			model.PropertyChanged += (sender, e) => busyModeChangeCount++;

			// Verify that IsBusy changed only once.
			model.InvokeEnterBusyMode();
			model.InvokeEnterBusyMode();
			model.InvokeEnterBusyMode();
			Assert.AreEqual(1, busyModeChangeCount);

			busyModeChangeCount = 0;

			// Verify that IsBusy changed only once.
			model.InvokeExitBusyMode();
			model.InvokeExitBusyMode();
			model.InvokeExitBusyMode();
			Assert.AreEqual(1, busyModeChangeCount);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// empties notification when enter busy mode.
		/// </summary>
		[TestMethod]
		public void EnterBusyModeEmptiesNotification()
		{
			var model = new TermListViewModelDraft();

			model.FillNotification();
			Assert.IsNotNull(model.Notification);

			model.InvokeEnterBusyMode();
			Assert.IsNull(model.Notification);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// disables <see cref="ITermListViewModel.RecreateStorage"/> command
		/// when enter busy mode.
		/// </summary>
		[TestMethod]
		public void EnterBusyModeDisablesRecreateStorageCommand()
		{
			var service = this._termsServiceMock;
			service
				.Setup(s => s.LoadTerms(ThreadPoolScheduler.Instance))
				.Returns(() => Observable.Throw<IEnumerable<Term>>(new InvalidTermsStorageException("Invalid")));

			var editModel = Mock.Of<ITermEditViewModel>();
			var listModel = new TermListViewModelDraft(service.Object, editModel);
			DispatcherHelper.ProcessCurrentQueue();

			Assert.IsTrue(listModel.RecreateStorage.CanExecute(null));

			listModel.InvokeEnterBusyMode();

			Assert.IsFalse(listModel.RecreateStorage.CanExecute(null));
		}

		#endregion

		#region Loading Of Terms

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// subscribes to <see cref="ITermsService.LoadTerms"/> in the constructor.
		/// </summary>
		[TestMethod]
		public void ModelSubscibesLoadTermsOnStartup()
		{
			var observable = new Mock<IObservable<IEnumerable<Term>>>();
			observable.Setup(o => o.Subscribe(It.IsAny<IObserver<IEnumerable<Term>>>())).Verifiable();

			var service = new Mock<ITermsService>();
			service.Setup(s => s.LoadTerms(ThreadPoolScheduler.Instance)).Returns(() => observable.Object);

			TermListViewModelFixture.CreateViewModelLoaded(service.Object, Mock.Of<ITermEditViewModel>());

			observable.Verify();
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// exposes terms after <see cref="ITermsService.LoadTerms"/> sequence
		/// completed.
		/// </summary>
		[TestMethod]
		public void ModelExposesTermsAfterLoadTerms()
		{
			var model = TermListViewModelFixture.CreateViewModelLoaded(this._termsServiceMock.Object, Mock.Of<ITermEditViewModel>());

			ReactiveAssert.AreElementsEqual(TermListViewModelFixture.SampleTerms, model.Terms.Cast<Term>());
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// sets notification when loading of terms failed.
		/// </summary>
		[TestMethod]
		public void ModelNotifiesWhenLoadTermsFailed()
		{
			var msg = "Failed";

			var service = this._termsServiceMock;
			service
				.Setup(s => s.LoadTerms(ThreadPoolScheduler.Instance))
				.Returns(() => Observable.Throw<IEnumerable<Term>>(new Exception(msg)));

			var editModel = Mock.Of<ITermEditViewModel>();
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);
			
			Assert.AreEqual(msg, listModel.Notification);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// enables <see cref="ITermListViewModel.RecreateStorage"/> command when loading of terms
		/// signaled that storage is invalid.
		/// </summary>
		[TestMethod]
		public void ModelEnablesRecreateStorageForInvalidStorageOnStartup()
		{
			var service = this._termsServiceMock;
			service
				.Setup(s => s.LoadTerms(ThreadPoolScheduler.Instance))
				.Returns(() => Observable.Throw<IEnumerable<Term>>(new InvalidTermsStorageException("Invalid")));

			var editModel = Mock.Of<ITermEditViewModel>();
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			Assert.IsTrue(listModel.RecreateStorage.CanExecute(null));
		}

		#endregion

		#region AddTerm Command

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class enters
		/// edit mode with empty values after <see cref="ITermListViewModel.AddTerm"/> executed.
		/// </summary>
		[TestMethod]
		public void AddTermCommandEntersEmptyEditMode()
		{
			var editModel = new Mock<ITermEditViewModel>();
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(this._termsServiceMock.Object, editModel.Object);

			listModel.AddTerm.Execute(null);

			editModel.VerifySet(m => m.Name = String.Empty);
			editModel.VerifySet(m => m.Definition = String.Empty);

			Assert.IsTrue(listModel.IsEditMode);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class refreshes
		/// state of commands after <see cref="ITermListViewModel.AddTerm"/> executed.
		/// </summary>
		[TestMethod]
		public void AddTermCommandRefreshesCommandState()
		{
			var editModel = new Mock<ITermEditViewModel>();
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(this._termsServiceMock.Object, editModel.Object);

			listModel.AddTerm.Execute(null);

			Assert.IsFalse(listModel.AddTerm.CanExecute(null));
			Assert.IsFalse(listModel.EditTerm.CanExecute(TermListViewModelFixture.SampleTerms[0]));
			Assert.IsFalse(listModel.DeleteTerm.CanExecute(null));
			Assert.IsTrue(listModel.Accept.CanExecute(null));
			Assert.IsTrue(listModel.Cancel.CanExecute(null));
		}

		#endregion

		#region EditTerm Command

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// refuses execution of <see cref="ITermListViewModel.EditTerm"/> command
		/// on term that doesn't exist.
		/// </summary>
		[TestMethod]
		public void EditTermCommandShouldNotExecuteOnUnknownTerm()
		{
			var editModel = new Mock<ITermEditViewModel>();
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(this._termsServiceMock.Object, editModel.Object);

			Assert.IsFalse(listModel.EditTerm.CanExecute(null));
			Assert.IsFalse(listModel.EditTerm.CanExecute(new Term("term4", "def 4")));
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class enters
		/// edit mode with existent values after <see cref="ITermListViewModel.EditTerm"/>
		/// command executed.
		/// </summary>
		[TestMethod]
		public void EditTermCommandEntersExistentEditMode()
		{
			var editModel = new Mock<ITermEditViewModel>();
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(this._termsServiceMock.Object, editModel.Object);

			var term = TermListViewModelFixture.SampleTerms[0];

			listModel.EditTerm.Execute(term);

			editModel.VerifySet(m => m.Name = term.Name);
			editModel.VerifySet(m => m.Definition = term.Definition);

			Assert.IsTrue(listModel.IsEditMode);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class refreshes
		/// state of commands after <see cref="ITermListViewModel.EditTerm"/> executed.
		/// </summary>
		[TestMethod]
		public void EditTermCommandRefreshesCommandState()
		{
			var editModel = new Mock<ITermEditViewModel>();
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(this._termsServiceMock.Object, editModel.Object);

			listModel.EditTerm.Execute(TermListViewModelFixture.SampleTerms[0]);

			Assert.IsFalse(listModel.AddTerm.CanExecute(null));
			Assert.IsFalse(listModel.EditTerm.CanExecute(TermListViewModelFixture.SampleTerms[0]));
			Assert.IsFalse(listModel.EditTerm.CanExecute(TermListViewModelFixture.SampleTerms[1]));
			Assert.IsTrue(listModel.DeleteTerm.CanExecute(null));
			Assert.IsTrue(listModel.Accept.CanExecute(null));
			Assert.IsTrue(listModel.Cancel.CanExecute(null));
		}

		#endregion

		#region Accept (Add) Command

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// subscribes to <see cref="ITermsService.AddTerm"/> when adding of term
		/// has been accepted.
		/// </summary>
		[TestMethod]
		public void AcceptAddCommandSubscibesAddTerm()
		{
			var term = new Term("term4", "def 4");

			var observable = new Mock<IObservable<Term>>();
			observable.Setup(o => o.Subscribe(It.IsAny<IObserver<Term>>())).Verifiable();

			var service = this._termsServiceMock;
			service.Setup(s => s.AddTerm(term, ThreadPoolScheduler.Instance)).Returns(() => observable.Object);

			var editModel = TermListViewModelFixture.MockEditModel(term);
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.AddTerm.Execute(null);
			listModel.Accept.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			observable.Verify();
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class show
		/// new term after <see cref="ITermsService.AddTerm"/> sequence completed.
		/// </summary>
		[TestMethod]
		public void AcceptAddCommandShowsNewTerm()
		{
			var term = new Term("term4", "def 4");

			var service = this._termsServiceMock;
			service.Setup(s => s.AddTerm(term, ThreadPoolScheduler.Instance)).Returns(() => Observable.Return(term));

			var editModel = TermListViewModelFixture.MockEditModel(term);
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.AddTerm.Execute(null);
			listModel.Accept.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			Assert.IsTrue(listModel.Terms.Cast<Term>().Contains(term));
			Assert.IsFalse(listModel.IsEditMode);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// sets notification and stays in edit mode when adding failed.
		/// </summary>
		[TestMethod]
		public void ModelNotifiesAndStaysInEditModeWhenAcceptAddFailed()
		{
			var term = new Term("term4", "def 4");
			var msg = "Failed";

			var service = this._termsServiceMock;
			service
				.Setup(s => s.AddTerm(term, ThreadPoolScheduler.Instance))
				.Returns(() => Observable.Throw<Term>(new Exception(msg)));

			var editModel = TermListViewModelFixture.MockEditModel(term);
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.AddTerm.Execute(null);
			listModel.Accept.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			Assert.IsFalse(listModel.Terms.Cast<Term>().Contains(term));
			Assert.AreEqual(msg, listModel.Notification);
			Assert.IsTrue(listModel.IsEditMode);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// sets notification and stays in edit mode when a data for a new term
		/// is invalid.
		/// </summary>
		[TestMethod]
		public void ModelNotifiesAndStaysInEditModeWhenAcceptAddInvalidTerm()
		{
			var editModel = new Mock<ITermEditViewModel>();
			editModel.SetupGet(m => m.Error).Returns("Error");

			var listModel = TermListViewModelFixture.CreateViewModelLoaded(this._termsServiceMock.Object, editModel.Object);

			listModel.AddTerm.Execute(null);
			listModel.Accept.Execute(null);

			StringAssert.Contains(listModel.Notification, editModel.Object.Error);
			Assert.IsTrue(listModel.IsEditMode);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// enables <see cref="ITermListViewModel.RecreateStorage"/> command when adding of a new
		/// term signaled that storage is invalid.
		/// </summary>
		[TestMethod]
		public void AcceptAddCommandEnablesRecreateStorageForInvalidStorage()
		{
			var term = new Term("term4", "def 4");

			var service = this._termsServiceMock;
			service
				.Setup(s => s.AddTerm(term, ThreadPoolScheduler.Instance))
				.Returns(() => Observable.Throw<Term>(new InvalidTermsStorageException("Invalid")));

			var editModel = TermListViewModelFixture.MockEditModel(term);
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.AddTerm.Execute(null);
			
			Assert.IsFalse(listModel.RecreateStorage.CanExecute(null));

			listModel.Accept.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			Assert.IsTrue(listModel.RecreateStorage.CanExecute(null));
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class refreshes
		/// state of commands after <see cref="ITermListViewModel.Accept"/> executed.
		/// </summary>
		[TestMethod]
		public void AcceptCommandShouldRefreshCommandState()
		{
			var oldTerm = TermListViewModelFixture.SampleTerms[0];
			var newTerm = new Term("term4", "def 4");

			var service = this._termsServiceMock;
			service
				.Setup(s => s.UpdateTerm(oldTerm, newTerm, ThreadPoolScheduler.Instance))
				.Returns(() => Observable.Return(newTerm));

			var editModel = TermListViewModelFixture.MockEditModel(newTerm);
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.EditTerm.Execute(oldTerm);
			listModel.Accept.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			Assert.IsTrue(listModel.AddTerm.CanExecute(null));
			Assert.IsTrue(listModel.EditTerm.CanExecute(newTerm));
			Assert.IsFalse(listModel.DeleteTerm.CanExecute(null));
			Assert.IsFalse(listModel.Accept.CanExecute(null));
			Assert.IsFalse(listModel.Cancel.CanExecute(null));
		}

		#endregion

		#region Accept (Edit) Command

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// subscribes to <see cref="ITermsService.UpdateTerm"/> when editing of term
		/// has been accepted.
		/// </summary>
		[TestMethod]
		public void AcceptEditTermCommandSubscibesUpdateTerm()
		{
			var oldTerm = TermListViewModelFixture.SampleTerms[0];
			var newTerm = new Term("term4", "def 4");

			var observable = new Mock<IObservable<Term>>();
			observable.Setup(o => o.Subscribe(It.IsAny<IObserver<Term>>())).Verifiable();

			var service = this._termsServiceMock;
			service.Setup(s => s.UpdateTerm(oldTerm, newTerm, ThreadPoolScheduler.Instance)).Returns(() => observable.Object);

			var editModel = TermListViewModelFixture.MockEditModel(newTerm);
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.EditTerm.Execute(oldTerm);
			listModel.Accept.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			observable.Verify();
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// shows updated term when <see cref="ITermsService.UpdateTerm"/> sequence
		/// completed.
		/// </summary>
		[TestMethod]
		public void AcceptEditTermCommandShowsUpdatedTerm()
		{
			var oldTerm = TermListViewModelFixture.SampleTerms[0];
			var newTerm = new Term("term4", "def 4");

			var service = this._termsServiceMock;
			service.Setup(s => s.UpdateTerm(oldTerm, newTerm, ThreadPoolScheduler.Instance)).Returns(() => Observable.Return(newTerm));

			var editModel = TermListViewModelFixture.MockEditModel(newTerm);
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.EditTerm.Execute(oldTerm);
			listModel.Accept.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			Assert.IsFalse(listModel.Terms.Cast<Term>().Contains(oldTerm));
			Assert.IsTrue(listModel.Terms.Cast<Term>().Contains(newTerm));
			Assert.IsFalse(listModel.IsEditMode);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// sets notification and stays in edit mode when update failed.
		/// </summary>
		[TestMethod]
		public void ModelNotifiesAndStaysInEditModeWhenAcceptEditFailed()
		{
			var oldTerm = TermListViewModelFixture.SampleTerms[0];
			var newTerm = new Term("term4", "def 4");

			var msg = "Failed";

			var service = this._termsServiceMock;
			service
				.Setup(s => s.UpdateTerm(oldTerm, newTerm, ThreadPoolScheduler.Instance))
				.Returns(() => Observable.Throw<Term>(new Exception(msg)));

			var editModel = TermListViewModelFixture.MockEditModel(newTerm);
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.EditTerm.Execute(oldTerm);
			listModel.Accept.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			Assert.IsTrue(listModel.Terms.Cast<Term>().Contains(oldTerm));
			Assert.IsFalse(listModel.Terms.Cast<Term>().Contains(newTerm));

			Assert.AreEqual(msg, listModel.Notification);
			Assert.IsTrue(listModel.IsEditMode);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// enables <see cref="ITermListViewModel.RecreateStorage"/> command when editing of term
		/// signaled that storage is invalid.
		/// </summary>
		[TestMethod]
		public void AcceptEditCommandEnablesRecreateStorageForInvalidStorage()
		{
			var oldTerm = TermListViewModelFixture.SampleTerms[0];
			var newTerm = new Term("term4", "def 4");

			var service = this._termsServiceMock;
			service
				.Setup(s => s.UpdateTerm(oldTerm, newTerm, ThreadPoolScheduler.Instance))
				.Returns(() => Observable.Throw<Term>(new InvalidTermsStorageException("Invalid")));

			var editModel = TermListViewModelFixture.MockEditModel(newTerm);
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.EditTerm.Execute(oldTerm);

			Assert.IsFalse(listModel.RecreateStorage.CanExecute(null));

			listModel.Accept.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			Assert.IsTrue(listModel.RecreateStorage.CanExecute(null));
		}

		#endregion

		#region Cancel Command

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// exits edit mode after <see cref="ITermListViewModel.Cancel"/> command
		/// executed.
		/// </summary>
		[TestMethod]
		public void CancelCommandExitsEditMode()
		{
			var editModel = new Mock<ITermEditViewModel>();

			// Test exit after add.

			var listModel = TermListViewModelFixture.CreateViewModelLoaded(this._termsServiceMock.Object, editModel.Object);

			listModel.AddTerm.Execute(null);
			listModel.Cancel.Execute(null);
			Assert.IsFalse(listModel.IsEditMode);

			// Test exit after edit.

			listModel = TermListViewModelFixture.CreateViewModelLoaded(this._termsServiceMock.Object, editModel.Object);

			listModel.EditTerm.Execute(TermListViewModelFixture.SampleTerms[0]);
			listModel.Cancel.Execute(null);

			Assert.IsFalse(listModel.IsEditMode);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class refreshes
		/// state of commands after <see cref="ITermListViewModel.Cancel"/> executed.
		/// </summary>
		[TestMethod]
		public void CancelCommandRefreshesCommandState()
		{
			var editModel = new Mock<ITermEditViewModel>();
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(this._termsServiceMock.Object, editModel.Object);

			listModel.AddTerm.Execute(null);
			listModel.Cancel.Execute(null);

			Assert.IsTrue(listModel.AddTerm.CanExecute(null));
			Assert.IsTrue(listModel.EditTerm.CanExecute(TermListViewModelFixture.SampleTerms[0]));
			Assert.IsFalse(listModel.DeleteTerm.CanExecute(null));
			Assert.IsFalse(listModel.Accept.CanExecute(null));
			Assert.IsFalse(listModel.Cancel.CanExecute(null));
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// empties notification modeafter <see cref="ITermListViewModel.Cancel"/> command
		/// executed.
		/// </summary>
		[TestMethod]
		public void CancelCommandEmptiesNotification()
		{
			var editModel = new Mock<ITermEditViewModel>();
			var listModel = new TermListViewModelDraft(this._termsServiceMock.Object, editModel.Object);

			listModel.AddTerm.Execute(null);
			listModel.FillNotification();
			listModel.Cancel.Execute(null);

			Assert.IsNull(listModel.Notification);
		}

		#endregion

		#region DeleteTerm Command

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// subscribes to <see cref="ITermsService.RemoveTerm"/> when deleting a term.
		/// </summary>
		[TestMethod]
		public void DeleteTermCommandSubscibesRemoveTerm()
		{
			var term = TermListViewModelFixture.SampleTerms[0];

			var observable = new Mock<IObservable<Term>>();
			observable.Setup(o => o.Subscribe(It.IsAny<IObserver<Term>>())).Verifiable();

			var service = this._termsServiceMock;
			service.Setup(s => s.RemoveTerm(term, ThreadPoolScheduler.Instance)).Returns(() => observable.Object);

			var editModel = Mock.Of<ITermEditViewModel>();
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.EditTerm.Execute(term);
			listModel.DeleteTerm.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			observable.Verify();
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// removes term from collection when <see cref="ITermsService.RemoveTerm"/>
		/// sequence completed.
		/// </summary>
		[TestMethod]
		public void DeleteTermCommandHidesDeletedTerm()
		{
			var term = TermListViewModelFixture.SampleTerms[0];

			var service = this._termsServiceMock;
			service.Setup(s => s.RemoveTerm(term, ThreadPoolScheduler.Instance)).Returns(() => Observable.Return(term));

			var editModel = Mock.Of<ITermEditViewModel>();
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.EditTerm.Execute(term);
			listModel.DeleteTerm.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			Assert.IsFalse(listModel.Terms.Cast<Term>().Contains(term));
			Assert.IsFalse(listModel.IsEditMode);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class refreshes
		/// state of commands after <see cref="ITermListViewModel.DeleteTerm"/> executed.
		/// </summary>
		[TestMethod]
		public void DeleteTermCommandRefreshesCommandState()
		{
			var term = TermListViewModelFixture.SampleTerms[0];

			var service = this._termsServiceMock;
			service.Setup(s => s.RemoveTerm(term, ThreadPoolScheduler.Instance)).Returns(() => Observable.Return(term));

			var editModel = Mock.Of<ITermEditViewModel>();
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.EditTerm.Execute(term);
			listModel.DeleteTerm.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			Assert.IsTrue(listModel.AddTerm.CanExecute(null));
			Assert.IsTrue(listModel.EditTerm.CanExecute(TermListViewModelFixture.SampleTerms[1]));
			Assert.IsFalse(listModel.DeleteTerm.CanExecute(null));
			Assert.IsFalse(listModel.Accept.CanExecute(null));
			Assert.IsFalse(listModel.Cancel.CanExecute(null));
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// sets notification and stays in edit mode when delete failed.
		/// </summary>
		[TestMethod]
		public void ModelNotifiesAndStaysInEditModeWhenDeleteFailed()
		{
			var term = TermListViewModelFixture.SampleTerms[0];
			var msg = "Failed";

			var service = this._termsServiceMock;
			service
				.Setup(s => s.RemoveTerm(term, ThreadPoolScheduler.Instance))
				.Returns(() => Observable.Throw<Term>(new Exception(msg)));

			var editModel = Mock.Of<ITermEditViewModel>();
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.EditTerm.Execute(term);
			listModel.DeleteTerm.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			Assert.IsTrue(listModel.Terms.Cast<Term>().Contains(term));
			Assert.AreEqual(msg, listModel.Notification);
			Assert.IsTrue(listModel.IsEditMode);
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// enables <see cref="ITermListViewModel.RecreateStorage"/> command when deletion of term
		/// signaled that storage is invalid.
		/// </summary>
		[TestMethod]
		public void DeleteCommandEnablesRecreateStorageForInvalidStorage()
		{
			var term = TermListViewModelFixture.SampleTerms[0];

			var service = this._termsServiceMock;
			service
				.Setup(s => s.RemoveTerm(term, ThreadPoolScheduler.Instance))
				.Returns(() => Observable.Throw<Term>(new InvalidTermsStorageException("Invalid")));

			var editModel = Mock.Of<ITermEditViewModel>();
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.EditTerm.Execute(term);

			Assert.IsFalse(listModel.RecreateStorage.CanExecute(null));

			listModel.DeleteTerm.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			Assert.IsTrue(listModel.RecreateStorage.CanExecute(null));
		}

		#endregion

		#region RecreateStorage Command

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// subscribes to <see cref="ITermsService.RecreateStorage"/> when
		/// corresponding command executed.
		/// </summary>
		[TestMethod]
		public void RecreateStorageCommandSubscibesRecreateStorage()
		{
			var term = new Term("term4", "def 4");

			// Create service mock that allows to load terms, prevents
			// adding a term, and verifies recreation attempt.

			var observable = new Mock<IObservable<Unit>>();
			observable.Setup(o => o.Subscribe(It.IsAny<IObserver<Unit>>())).Verifiable();

			var service = this._termsServiceMock;
			service
				.Setup(s => s.AddTerm(term, ThreadPoolScheduler.Instance))
				.Returns(() => Observable.Throw<Term>(new InvalidTermsStorageException("Invalid")));
			service
				.Setup(s => s.RecreateStorage(
					It.Is<IEnumerable<Term>>(terms => !TermListViewModelFixture.SampleTerms.Except(terms).Any()),
					ThreadPoolScheduler.Instance))
				.Returns(() => observable.Object);

			// Create view model to test invokation on RecreateStorage.

			var editModel = TermListViewModelFixture.MockEditModel(term);
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			listModel.AddTerm.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			listModel.RecreateStorage.Execute(null);

			observable.Verify();
		}

		/// <summary>
		/// Tests that an instance of the <see cref="TermListViewModel"/> class
		/// sets notification and still allows execution of the 
		/// <see cref="ITermListViewModel.RecreateStorage"/> command when recreation
		/// of storage failed.
		/// </summary>
		[TestMethod]
		public void ModelNotifiesWhenRecreateStorageFailed()
		{
			var term = new Term("term4", "def 4");
			
			var service = new Mock<ITermsService>();
			service
				.Setup(s => s.LoadTerms(ThreadPoolScheduler.Instance))
				.Returns(() => Observable.Throw<IEnumerable<Term>>(new InvalidTermsStorageException("Invalid")));
			service
				.Setup(s => s.RecreateStorage(It.Is<IEnumerable<Term>>(terms => !terms.Any()), ThreadPoolScheduler.Instance))
				.Returns(() => Observable.Throw<Unit>(new InvalidTermsStorageException("Invalid")));

			var editModel = TermListViewModelFixture.MockEditModel(term);
			var listModel = TermListViewModelFixture.CreateViewModelLoaded(service.Object, editModel);

			Assert.IsTrue(listModel.RecreateStorage.CanExecute(null));
			
			listModel.RecreateStorage.Execute(null);

			DispatcherHelper.ProcessCurrentQueue();

			Assert.IsNotNull(listModel.Notification);
			Assert.IsTrue(listModel.RecreateStorage.CanExecute(null));
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Initializes a new instance of the <see cref="TermListViewModel"/> class
		/// and ensures that terms were loaded from the service.
		/// </summary>
		/// <param name="termsService">A service that allows to access a storage of glossary terms.</param>
		/// <param name="editViewModel">A model of a view to edit terms.</param>
		private static TermListViewModel CreateViewModelLoaded(ITermsService termsService, ITermEditViewModel editViewModel)
		{
			var model = new TermListViewModel(termsService, editViewModel);
			DispatcherHelper.ProcessCurrentQueue();

			return model;
		}

		/// <summary>
		/// Initializes a mock of the <see cref="ITermEditViewModel"/> interface
		/// with property getters returning values from the specified term.
		/// </summary>
		/// <param name="term">Term to use in mocked properties.</param>
		/// <returns>Initialized mock of the <see cref="ITermEditViewModel"/>.</returns>
		private static ITermEditViewModel MockEditModel(Term term)
		{
			var model = new Mock<ITermEditViewModel>();
			model.SetupGet(m => m.Name).Returns(term.Name);
			model.SetupGet(m => m.Definition).Returns(term.Definition);

			return model.Object;
		}

		#endregion
	}
}
