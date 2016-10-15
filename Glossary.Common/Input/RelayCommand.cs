using System;
using System.Windows.Input;

namespace Glossary.Input
{
	/// <summary>
	/// Implements <see cref="ICommand"/> using actions for execution and availability check.
	/// </summary>
	public class RelayCommand : ICommand
	{
		/// <summary>
		/// An action to invoke when command is executed.
		/// </summary>
		private readonly Action<object> _execute;

		/// <summary>
		/// An predicate to invoke to check whether command is available.
		/// </summary>
		private readonly Predicate<object> _canExecute;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand"/> class.
		/// </summary>
		/// <param name="execute">An action to invoke when command is executed.</param>
		public RelayCommand(Action<object> execute)
			: this(execute, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand"/> class.
		/// </summary>
		/// <param name="execute">An action to invoke when command is executed.</param>
		/// <param name="canExecute">An predicate to invoke to check whether command is available.</param>
		public RelayCommand(Action<object> execute, Predicate<object> canExecute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException("execute");
			}

			this._execute = execute;
			this._canExecute = canExecute;
		}

		/// <summary>
		/// Determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">Data used by the command. If the command does
		/// not require data to be passed, this object can be set to <langword>null</langword>.</param>
		/// <returns><langword>true</langword> if this command can be executed; otherwise, <langword>false</langword>.</returns>
		public bool CanExecute(object parameter)
		{
			if (this._canExecute != null)
			{
				return this._canExecute(parameter);
			}

			return true;
		}

		/// <summary>
		/// Called when the command is invoked.
		/// </summary>
		/// <param name="parameter">Data used by the command. If the command does
		/// not require data to be passed, this object can be set to <langword>null</langword>.</param>
		public void Execute(object parameter)
		{
			this._execute(parameter);
		}

		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}
	}
}
