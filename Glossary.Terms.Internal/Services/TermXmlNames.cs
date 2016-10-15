using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace Glossary.Terms.Services
{
	/// <summary>
	/// Provides XNames for Xml file which stores a terms.
	/// </summary>
	internal static class TermXmlNames
	{
		/// <summary>
		/// Gets the name of element containing Term(s).
		/// </summary>
		public static readonly XName TermsElement = "Terms";

		/// <summary>
		/// Gets the name of element that represents a term.
		/// </summary>
		public static readonly XName TermElement = "Term";

		/// <summary>
		/// Gets the name of attribute containing term name.
		/// </summary>
		public static readonly XName TermNameAttribute = "Name";

		/// <summary>
		/// Gets the name of attribute containing term definition.
		/// </summary>
		public static readonly XName TermDefinitionAttribute = "Definition";
	}
}