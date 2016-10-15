using System;
using System.ComponentModel;
using System.Linq.Expressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Glossary.Data
{
	/// <summary>
	/// Contains unit tests of the <see cref="NotificationObject"/> class.
	/// </summary>
	[TestClass]
	public class NotificationObjectFixture
	{
		/// <summary>
		/// Tests whether <see cref="NotificationObject"/> notifies that
		/// property has changed.
		/// </summary>
		[TestMethod]
		public void TestNotificationAboutSingleProperty()
		{
			Expression<Func<TestableNotificationObject, int>> expr = _ => _.TestProperty;
			PropertyChangedEventArgs e = null;

			var no = new TestableNotificationObject();
			no.PropertyChanged += (sender, e2) => e = e2;
			no.InvokeRaisePropertyChanged(expr);

			Assert.AreNotEqual(null, e);
			Assert.AreEqual(PropertyExpressionHelper.GetName(expr), e.PropertyName);
		}

		/// <summary>
		/// Tests whether <see cref="NotificationObject"/> notifies that
		/// all properties has changed.
		/// </summary>
		[TestMethod]
		public void TestNotificationAboutAllProperties()
		{
			PropertyChangedEventArgs e = null;

			var no = new TestableNotificationObject();
			no.PropertyChanged += (sender, e2) => e = e2;
			no.InvokeRaiseAllPropertiesChanged();

			Assert.AreNotEqual(null, e);
			Assert.AreEqual(null, e.PropertyName);
		}
	}
}
