using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

using Glossary.Terms.Properties;

namespace Glossary.Terms.Services
{
	/// <summary>
	/// Provides a service that allows to access glossary terms stored in Xml. 
	/// </summary>
	internal sealed partial class XmlTermsService : ITermsService
	{
		/// <summary>
		/// Xml file where terms are stored.
		/// </summary>
		private readonly string _fileName;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="XmlTermsService"/> class.
		/// </summary>
		/// <param name="fileName">A name of an Xml file where terms are stored.</param>
		public XmlTermsService(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}

			this._fileName = fileName;
		}

		/// <summary>
		/// Gets an observable sequence that retrieves terms from the storage.
		/// </summary>
		/// <param name="scheduler">A scheduler to invoke service operations on.</param>
		/// <returns>An observable sequence of portions of terms.</returns>
		public IObservable<IEnumerable<Term>> LoadTerms(IScheduler scheduler)
		{
			return Observable
				.Return(Unit.Default, scheduler)
				.SelectMany(_ => this.LoadStorage().Descendants(TermXmlNames.TermElement))
				.Select(node => new Term(
					node.Attribute(TermXmlNames.TermNameAttribute).Value,
					node.Attribute(TermXmlNames.TermDefinitionAttribute).Value))
				.Do(term => XmlTermsService.ApplyTermRules(term, "term"))
				.Catch((Exception ex) => Observable.Throw<Term>(
					XmlTermsService.WrapIntoInvalidStorageException(
						ex,
						String.Format(Resources.InvalidXmlStorageToLoad, this._fileName))))
				.ToArray();
		}

		/// <summary>
		/// Gets an observable that adds a new term to the storage.
		/// </summary>
		/// <param name="term">A term to add to the storage.</param>
		/// <param name="scheduler">A scheduler to invoke service operations on.</param>
		/// <returns>An observable sequence that adds the specified term and pushes
		/// it to subscribers.</returns>
		public IObservable<Term> AddTerm(Term term, IScheduler scheduler)
		{
			XmlTermsService.ApplyTermRules(term, "term");

			return Observable
				.Return(term, scheduler)
				.Do(_ =>
					{
						var doc = this.LoadStorage();

						// Check whether a term with the same name already exists.
						// A client must use UpdateTerm in this case.
						if (doc
							.Descendants(TermXmlNames.TermElement)
							.Any(el => el.Attribute(TermXmlNames.TermNameAttribute).Value == term.Name))
						{
							throw new InvalidOperationException(String.Format(
								Resources.UnableToAddTermAlreadyExists,
								term.Name));
						}

						var xTerm = new XElement(
							TermXmlNames.TermElement,
							new XAttribute(TermXmlNames.TermNameAttribute, term.Name));

						if (!String.IsNullOrEmpty(term.Definition))
						{
							xTerm.Add(new XAttribute(TermXmlNames.TermDefinitionAttribute, term.Definition));
						}

						doc.Root.Add(xTerm);
						
						this.SaveStorage(doc);
					});
		}

		/// <summary>
		/// Gets an observable that updates an existing term in the storage.
		/// </summary>
		/// <param name="oldTerm">An old value of term to update in the storage.</param>
		/// <param name="newTerm">A new value of term.</param>
		/// <param name="scheduler">A scheduler to invoke service operations on.</param>
		/// <returns>An observable sequence that updates the specified term and pushes
		/// it to subscribers.</returns>
		public IObservable<Term> UpdateTerm(Term oldTerm, Term newTerm, IScheduler scheduler)
		{
			XmlTermsService.ApplyTermRules(oldTerm, "oldTerm");
			XmlTermsService.ApplyTermRules(newTerm, "newTerm");

			return Observable
				.Return(newTerm, scheduler)
				.Do(_ =>
				{
					var doc = this.LoadStorage();

					// Check whether an old term already exists.
					// If it is not, the client must use AddTerm.
					var xTerm = doc
						.Descendants(TermXmlNames.TermElement)
						.SingleOrDefault(el => el.Attribute(TermXmlNames.TermNameAttribute).Value == oldTerm.Name);
					if (xTerm == null)
					{
						throw new InvalidOperationException(String.Format(
							Resources.UnableToUpdateNonExistentTerm,
							oldTerm.Name));
					}

					// Check whether a new term already exists if names are different.
					if (oldTerm.Name != newTerm.Name)
					{
						var newTermExists = doc
							.Descendants(TermXmlNames.TermElement)
							.Any(el => el.Attribute(TermXmlNames.TermNameAttribute).Value == newTerm.Name);
						if (newTermExists)
						{
							throw new InvalidOperationException(String.Format(
								Resources.UnableToUpdateToAnotherExistentTerm,
								newTerm.Name));
						}
					}

					xTerm.Attribute(TermXmlNames.TermNameAttribute).Value = newTerm.Name;

					if (!String.IsNullOrEmpty(newTerm.Definition))
					{
						xTerm.SetAttributeValue(TermXmlNames.TermDefinitionAttribute, newTerm.Definition);
					}
					else
					{
						xTerm.Attribute(TermXmlNames.TermDefinitionAttribute).Remove();
					}

					this.SaveStorage(doc);
				});
		}

		/// <summary>
		/// Gets an observable that removes an existing term from the storage.
		/// </summary>
		/// <param name="term">A term to remove from the storage.</param>
		/// <param name="scheduler">A scheduler to invoke service operations on.</param>
		/// <returns>An observable sequence that removes the specified term and pushes
		/// it to subscribers.</returns>
		public IObservable<Term> RemoveTerm(Term term, IScheduler scheduler)
		{
			XmlTermsService.ApplyTermRules(term, "term");

			return Observable
				.Return(term, scheduler)
				.Do(_ =>
				{
					var doc = this.LoadStorage();

					// Search for a node to remove.
					var xTerm = doc
						.Descendants(TermXmlNames.TermElement)
						.SingleOrDefault(el => el.Attribute(TermXmlNames.TermNameAttribute).Value == term.Name);
					if (xTerm == null)
					{
						throw new InvalidOperationException(String.Format(
							Resources.UnableToRemoveNonExistentTerm,
							term.Name));
					}

					xTerm.Remove();

					this.SaveStorage(doc);
				});
		}

		/// <summary>
		/// Gets an observable that recreates storage with the specified terms.
		/// </summary>
		/// <param name="terms">A terms to add to a new storage.</param>
		/// <param name="scheduler">A scheduler to invoke service operations on.</param>
		/// <returns>An observable sequence that recreates storage.</returns>
		public IObservable<Unit> RecreateStorage(IEnumerable<Term> terms, IScheduler scheduler)
		{
			if (terms == null)
			{
				throw new ArgumentNullException("terms");
			}

			return Observable
				.Return(Unit.Default, scheduler)
				.Do(_ =>
					{
						var doc = new XDocument(new XElement(TermXmlNames.TermsElement));

						foreach (var term in terms)
						{
							XmlTermsService.ApplyTermRules(term, "terms");

							var xTerm = new XElement(
								TermXmlNames.TermElement,
								new XAttribute(TermXmlNames.TermNameAttribute, term.Name));

							if (!String.IsNullOrEmpty(term.Definition))
							{
								xTerm.Add(new XAttribute(TermXmlNames.TermDefinitionAttribute, term.Definition));
							}

							doc.Root.Add(xTerm);
						}

						this.SaveStorage(doc);
					})
				.Catch((Exception ex) => Observable.Throw<Unit>(
					XmlTermsService.WrapIntoInvalidStorageException(
						ex,
						String.Format(Resources.InvalidXmlStorageToSave, this._fileName))));
		}

		/// <summary>
		/// Loads Xml file used as a storage.
		/// </summary>
		/// <returns><see cref="XDocument"/> containing loaded Xml.</returns>
		private XDocument LoadStorage()
		{
			try
			{
				return XDocument.Load(this._fileName);
			}
			catch (Exception ex)
			{
				throw new InvalidTermsStorageException(
					String.Format(Resources.InvalidXmlStorageToLoad, this._fileName),
					ex);
			}
		}

		/// <summary>
		/// Saves <see cref="XDocument"/> to Xml file used as a storage.
		/// </summary>
		/// <param name="doc"><see cref="XDocument"/> containing Xml to save.</param>
		private void SaveStorage(XDocument doc)
		{
			try
			{
				doc.Save(this._fileName);
			}
			catch (Exception ex)
			{
				throw new InvalidTermsStorageException(
					String.Format(Resources.InvalidXmlStorageToSave, this._fileName),
					ex);
			}
		}

		/// <summary>
		/// Wraps the specified exception into <see cref="InvalidTermsStorageException"/>
		/// or returns it if its type is <see cref="InvalidTermsStorageException"/>.
		/// </summary>
		/// <param name="ex">An exception to wrap.</param>
		/// <param name="message">A message to use for a new <see cref="InvalidTermsStorageException"/>
		/// instance.</param>
		/// <returns>An instance of the <see cref="InvalidTermsStorageException"/>.</returns>
		private static InvalidTermsStorageException WrapIntoInvalidStorageException(Exception ex, string message)
		{
			var ise = ex as InvalidTermsStorageException;
			if (ise != null)
			{
				return ise;
			}

			return new InvalidTermsStorageException(message, ex);
		}

		/// <summary>
		/// Applies common validation rules to the specified term.
		/// </summary>
		/// <param name="term">A term to apply rules to.</param>
		/// <param name="argument">A name of argument containing a term.</param>
		private static void ApplyTermRules(Term term, string argument)
		{
			if (term == null)
			{
				throw new ArgumentNullException(argument);
			}

			if (String.IsNullOrWhiteSpace(term.Name))
			{
				throw new ArgumentNullException(argument);
			}
		}
	}
}
