using System;
using System.Diagnostics;
using System.Windows.Input;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Glossary.Input
{
	/// <summary>
	/// Contains unit tests of the <see cref="RelayCommand"/> class.
	/// </summary>
	[TestClass]
	public class RelayCommandFixture
	{
		/// <summary>
		/// Tests that <see cref="RelayCommand"/> must have execution action.
		/// </summary>
		[TestMethod]
		public void NoRelayCommandWithoutExecution()
		{
			try
			{
				new RelayCommand(null);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
				Assert.AreEqual("execute", ((ArgumentNullException)ex).ParamName);
			}

			try
			{
				new RelayCommand(null, null);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
				Assert.AreEqual("execute", ((ArgumentNullException)ex).ParamName);
			}
		}

		/// <summary>
		/// Tests that <see cref="RelayCommand"/> invokes action specified for CanExecute once.
		/// </summary>
		[TestMethod]
		public void CanExecuteInvokedOnce()
		{
			var canExecuteInvokeCount = 0;
			Predicate<object> canExecute = _ =>
				{
					canExecuteInvokeCount++;
					return true;
				};

			var relayCommand = new RelayCommand(_ => { }, canExecute);
			relayCommand.CanExecute(null);

			Assert.AreEqual(1, canExecuteInvokeCount);
		}

		/// <summary>
		/// Tests that <see cref="RelayCommand"/> with no CanExecute invokes action specified
		/// for Execute once.
		/// </summary>
		[TestMethod]
		public void ExecuteInvokedOnce()
		{
			var executeInvokeCount = 0;
			Action<object> execute = _ => executeInvokeCount++;

			var relayCommand = new RelayCommand(execute);
			if (relayCommand.CanExecute(null))
			{
				relayCommand.Execute(null);
			}

			Assert.AreEqual(1, executeInvokeCount);
		}

		/// <summary>
		/// Tests that <see cref="RelayCommand"/> invokes all actions once.
		/// </summary>
		[TestMethod]
		public void ExecuteAndCanExecuteInvokedOnce()
		{
			var canExecuteInvokeCount = 0;
			Predicate<object> canExecute = _ =>
				{
					canExecuteInvokeCount++;
					return true;
				};

			var executeInvokeCount = 0;
			Action<object> execute = _ => executeInvokeCount++;

			var relayCommand = new RelayCommand(execute, canExecute);
			if (relayCommand.CanExecute(null))
			{
				relayCommand.Execute(null);
			}

			Assert.AreEqual(1, canExecuteInvokeCount);
			Assert.AreEqual(1, executeInvokeCount);
		}
	}
}
