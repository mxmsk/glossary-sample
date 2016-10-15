using System;
using System.ComponentModel;
using System.Linq.Expressions;

using Glossary.Properties;

namespace Glossary.Data
{
    /// <summary>
    /// Base class that support property notification.
    /// </summary>
    public abstract class NotificationObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>        
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
		/// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has a new value.</param>
		private void InternalRaisePropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
		/// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
		/// <typeparam name="TClass">The type of object which property has a new value.</typeparam>
		/// <typeparam name="TProperty">The type of the property that has a new value.</typeparam>
        /// <param name="propertyExpression">A Lambda expression representing the property that has a new value.</param>
        protected void RaisePropertyChanged<TClass, TProperty>(Expression<Func<TClass, TProperty>> propertyExpression)
        {
			this.InternalRaisePropertyChanged(PropertyExpressionHelper.GetName(propertyExpression));
        }

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event that notifies about changes of all properties.
		/// </summary>
		protected void RaiseAllPropertiesChanged()
		{
			this.InternalRaisePropertyChanged(null);
		}
    }
}
