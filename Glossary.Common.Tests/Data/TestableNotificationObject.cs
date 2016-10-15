using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Glossary.Data
{
	/// <summary>
	/// Allows to tests to create instances of <see cref="NotificationObject"/> class.
	/// </summary>
	internal sealed class TestableNotificationObject : NotificationObject
	{
		/// <summary>
		/// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// </summary>
		/// <typeparam name="TClass">The type of object which property has a new value</typeparam>
		/// <typeparam name="TProperty">The type of the property that has a new value</typeparam>
		/// <param name="propertyExpression">A Lambda expression representing the property that has a new value.</param>
		public void InvokeRaisePropertyChanged<TClass, TProperty>(Expression<Func<TClass, TProperty>> propertyExpression)
		{
			base.RaisePropertyChanged<TClass, TProperty>(propertyExpression);
		}

		/// <summary>
		/// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event that notifies about changes of all properties.
		/// </summary>
		public void InvokeRaiseAllPropertiesChanged()
		{
			base.RaiseAllPropertiesChanged();
		}

		/// <summary>
		/// Property to test notification.
		/// </summary>
		public int TestProperty
		{
			get;
			set;
		}
	}
}
