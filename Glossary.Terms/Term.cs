using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glossary.Terms
{
	/// <summary>
	/// Represents a term in a glossary.
	/// </summary>
	public class Term : IEquatable<Term>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Term"/> class.
		/// </summary>
		/// <param name="name">A name of a new term.</param>
		/// <param name="definition">A description of a new term.</param>
		public Term(string name, string definition)
		{
			this.Name = name;
			this.Definition = definition;
		}

		/// <summary>
		/// Gets the name of the term.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the description of the term.
		/// </summary>
		public string Definition
		{
			get;
			private set;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns><langword>true</langword> if the current object is equal to the other parameter;
		/// otherwise, <langword>false</langword>.</returns>
		public virtual bool Equals(Term other)
		{
			if (other != null)
			{
				return this.Name == other.Name && this.Definition == other.Definition;
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns><langword>true</langword> if the specified object is equal to the current
		/// object; otherwise, <langword>false</langword>.</returns>
		public override bool Equals(object obj)
		{
			if (obj != null && obj.GetType() == typeof(Term))
			{
				return this.Equals((Term)obj);
			}

			return false;
		}

		/// <summary>
		/// Gets a hash code of this object.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return this.Name.GetHashCode() ^ this.Definition.GetHashCode();
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return String.Format("{0} - {1}", this.Name, this.Definition);
		}
	}
}
