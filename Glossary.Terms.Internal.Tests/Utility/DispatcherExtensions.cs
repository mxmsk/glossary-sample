using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace Glossary.Terms.Utility
{
	/// <summary>
	/// Contains helper methods for working with WPF dispatcher.
	/// </summary>
	internal static class DispatcherHelper
	{
		/// <summary>
		/// Process current dispatcher's queue to the end.
		/// </summary>
		public static void ProcessCurrentQueue()
		{
			var frame = new DispatcherFrame();

			Dispatcher.CurrentDispatcher.BeginInvoke(
				DispatcherPriority.Background,
				new DispatcherOperationCallback(_ => frame.Continue = false), frame);

			Dispatcher.PushFrame(frame);
		}
    }
}
