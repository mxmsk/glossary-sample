using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

using Glossary.Data;

namespace Glossary.Terms.Views
{
	/// <summary>
	/// Represents a view that manages glossary terms.
	/// </summary>
	internal sealed partial class TermListView : UserControl, ITermListView, IWeakEventListener
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TermListView"/> class.
		/// </summary>
		/// <param name="model">A model to use by a new instance.</param>
		public TermListView(ITermListViewModel model)
		{
			if (model == null)
			{
				throw new ArgumentException("model");
			}

			this.Model = model;
			this.InitializeComponent();

			this.SetBinding(
				TermListView.IsEditModeProperty,
				new Binding(PropertyExpressionHelper.GetName<ITermListViewModel, bool>(_ => _.IsEditMode)));
		}

		/// <summary>
		/// Gets the model of this view.
		/// </summary>
		public ITermListViewModel Model
		{
			get
			{
				return (ITermListViewModel)this.DataContext;
			}
			private set
			{
				this.DataContext = value;

				// Workaround for DataGrid scrolling to current item.
				CurrentChangedEventManager.AddListener(value.Terms, this);
			}
		}

		/// <summary>
		/// Called when <see cref="ITermListViewModel.Terms"/> changed current item.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> that contains no event data.</param>
		private void Terms_CurrentChanged(object sender, EventArgs e)
		{
			// Unfortunately, WPF DataGrid doesn't handle change of CollectionView
			// current item correctly. We need to force scroll bar to position.
			var cView = (ICollectionView)sender;
			if (cView.CurrentItem != null)
			{
				this.TermsDataGrid.ScrollIntoView(cView.CurrentItem);
			}
		}

		/// <summary>
		/// Identifies dependency property used to listen changes of <see cref="ITermListViewModel.IsEditMode"/>.
		/// </summary>
		public static readonly DependencyProperty IsEditModeProperty = DependencyProperty.Register(
			"IsEditMode", typeof(bool), typeof(TermListView),
			new FrameworkPropertyMetadata(TermListView.OnIsEditModeChanged));
		
		/// <summary>
		/// Invoke when model's <see cref="ITermListViewModel.IsEditMode"/> property changed.
		/// </summary>
		/// <param name="d">The <see cref="DependencyObject"/> on which the property has changed value.</param>
		/// <param name="e">The object containing the event data.</param>
		public static void OnIsEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var view = (TermListView)d;

			// Move focus to element used to edit term name when model entered edit mode.
			if (view.Model.IsEditMode)
			{
				view.Dispatcher.BeginInvoke(
					(Action)(() => Keyboard.Focus(view.TermNameElement)),
					DispatcherPriority.Input);
			}
		}

		#region IWeakEventListener Members

		/// <summary>
		/// Receives events from the centralized event manager.
		/// </summary>
		/// <param name="managerType">The type of the <see cref="WeakEventManager"/> calling
		/// this method.</param>
		/// <param name="sender">Object that originated the event.</param>
		/// <param name="e">Event data.</param>
		/// <returns><langword>true</langword> if the listener handled the event; otherwise,
		/// <langword>false</langword>.</returns>
		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(CurrentChangedEventManager))
			{
				this.Terms_CurrentChanged(sender, e);
				return true;
			}

			return false;
		}

		#endregion
	}
}
