using System;
using System.Linq.Expressions;

using Glossary.Properties;

namespace Glossary.Data
{
	/// <summary>
	/// Provides helper to extract property name from expression with property getter.
	/// </summary>
	public static class PropertyExpressionHelper
	{
		/// <summary>
		/// Gets the name of the property from expression that consists of property getter.
		/// </summary>
		/// <typeparam name="TClass">The type of object which property name is required.</typeparam>
		/// <typeparam name="TProperty">The type of the property which name is required.</typeparam>
		/// <param name="propertyExpression">A Lambda expression representing the property getter.</param>
		public static string GetName<TClass, TProperty>(Expression<Func<TClass, TProperty>> propertyExpression)
		{
			if (propertyExpression == null)
			{
				throw new ArgumentNullException("propertyExpression");
			}

			var memberExpression = propertyExpression.Body as MemberExpression;
			if (memberExpression == null)
			{
				throw new NotSupportedException(String.Format(
					Resources.NotificationObjectSupportsMemberExpressionOnly,
					typeof(MemberExpression).Name));
			}

			return memberExpression.Member.Name;
		}
	}
}
