using System;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Reactive.Testing;

namespace Glossary.Terms.Services
{
	/// <summary>
	/// Contains unit tests of the <see cref="XmlTermsService"/> class.
	/// </summary>
	[TestClass]
	public class XmlTermsServiceFixture
	{
		/// <summary>
		/// Collection of valid sample terms.
		/// </summary>
		private static ReadOnlyCollection<Term> SampleTerms;

		/// <summary>
		/// Contains the directory for files deployed for the test run.
		/// </summary>
		private static string DeploymentDirectory;

		/// <summary>
		/// Contains code that must be used before any of the tests have run.
		/// </summary>
		/// <param name="context"><see cref="TestContext"/> that stores information about the current test.</param>
		[ClassInitialize]
		public static void ClassInit(TestContext context)
		{
			XmlTermsServiceFixture.DeploymentDirectory = context.DeploymentDirectory;

			XmlTermsServiceFixture.SampleTerms = new ReadOnlyCollection<Term>(
				new Term[]
				{
					new Term("term0", "def 0"),
					new Term("term1", "def 1"),
					new Term("term2", "def 2"),
				});
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// loads valid terms from valid Xml.
		/// </summary>
		[TestMethod]
		public void ObservableLoadsTermsFromValidXml()
		{
			var service = new XmlTermsService(XmlTermsServiceFixture.TermsToFile(
				XmlTermsServiceFixture.SampleTerms,
				"ObservableLoadsTermsFromValidXml.xml"));
			
			var serviceTerms = new List<Term>();

			service
				.LoadTerms(Scheduler.Immediate)
				.Subscribe(terms => serviceTerms.AddRange(terms));

			ReactiveAssert.AreElementsEqual(XmlTermsServiceFixture.SampleTerms, serviceTerms);
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// produces empty collection from valid Xml with no terms.
		/// </summary>
		[TestMethod]
		public void ObservableLoadsNoTermsFromEmptyValidXml()
		{
			var service = new XmlTermsService(XmlTermsServiceFixture.TermsToFile(
				Enumerable.Empty<Term>(),
				"ObservableLoadsNoTermsFromEmptyValidXml.xml"));

			var serviceTerms = new List<Term>();

			service
				.LoadTerms(Scheduler.Immediate)
				.Subscribe(terms => serviceTerms.AddRange(terms));

			ReactiveAssert.AreElementsEqual(Enumerable.Empty<Term>(), serviceTerms);
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when loading from invalid Xml.
		/// </summary>
		[TestMethod]
		public void ObservableThrowsFromInvalidXml()
		{
			var service = new XmlTermsService(XmlTermsServiceFixture.TermsToFile(
				new Term[] 
				{ 
					XmlTermsServiceFixture.SampleTerms[0],
					new Term(null, String.Empty),
				},
				"ObservableThrowsFromInvalidXml.xml"));

			ReactiveAssert.Throws<InvalidTermsStorageException>(
				() =>
				{
					service
						.LoadTerms(Scheduler.Immediate)
						.Subscribe();
				});
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when loading from invalid Xml.
		/// </summary>
		[TestMethod]
		public void ObservableShouldntPushTermsFromInvalidXml()
		{
			var mix = new Term[] 
			{
				XmlTermsServiceFixture.SampleTerms[0],
				new Term(null, null),
				XmlTermsServiceFixture.SampleTerms[1],
			};

			var service = new XmlTermsService(
				XmlTermsServiceFixture.TermsToFile(mix, "ObservableShouldntPushTermsFromInvalidXml.xml"));

			bool onNextInvoked = false;

			service
			    .LoadTerms(Scheduler.Immediate)
			    .Subscribe(terms => onNextInvoked = true, _ => { }, () => { });

			Assert.IsFalse(onNextInvoked);
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when loading from file that doesnt exist.
		/// </summary>
		[TestMethod]
		public void ObservableThrowsWhenFileDoesntExist()
		{
			var service = new XmlTermsService(String.Empty);

			ReactiveAssert.Throws<InvalidTermsStorageException>(
				() =>
				{
					service
						.LoadTerms(Scheduler.Immediate)
						.Subscribe();
				});
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// successfully stores new term in Xml and pushes it to subscribers.
		/// </summary>
		[TestMethod]
		public void ObservableAddsAndPushesNewTerm()
		{
			var fileName = XmlTermsServiceFixture.TermsToFile(
				XmlTermsServiceFixture.SampleTerms,
				"ObservableAddsAndPushesNewTerm.xml");

			var service = new XmlTermsService(fileName);
			var term = new Term("newTerm", "newDef");

			Term pushed = null;
			service.AddTerm(term, Scheduler.Immediate).Subscribe(p => pushed = p);

			Assert.AreEqual(term, pushed);

			var xTerm = XDocument.Load(fileName)
				.Descendants(TermXmlNames.TermElement)
				.FirstOrDefault(x => x.Attribute(TermXmlNames.TermNameAttribute).Value.Equals(term.Name));

			Assert.IsNotNull(xTerm);
			Assert.AreEqual(term.Definition, xTerm.Attribute(TermXmlNames.TermDefinitionAttribute).Value);
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when adding an existing term.
		/// </summary>
		[TestMethod]
		public void ObservableThrowsOnAddExistingTerm()
		{
			var service = new XmlTermsService(XmlTermsServiceFixture.TermsToFile(
				XmlTermsServiceFixture.SampleTerms,
				"ObservableThrowsOnAddExistingTerm.xml"));

			Exception ex = null;

			service
				.AddTerm(XmlTermsServiceFixture.SampleTerms[0], Scheduler.Immediate)
				.Subscribe(_ => { }, ex2 => ex = ex2, () => { });

			Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
			Assert.IsTrue(ex.Message.Contains("already exists"));
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when adding null.
		/// </summary>
		[TestMethod]
		public void ServiceThrowsOnAddNullTerm()
		{
			var service = new XmlTermsService(String.Empty);
			try
			{
				service.AddTerm(null, Scheduler.Immediate);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
				Assert.IsTrue(((ArgumentNullException)ex).ParamName == "term");
			}
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when adding a term without a name.
		/// </summary>
		[TestMethod]
		public void ServiceThrowsOnAddTermWithoutName()
		{
			var service = new XmlTermsService(String.Empty);

			// Test null.
			try
			{
				service.AddTerm(new Term(null, String.Empty), Scheduler.Immediate);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentException));
				Assert.IsTrue(((ArgumentException)ex).ParamName == "term");
			}

			// Test whitespace.
			try
			{
				service.AddTerm(new Term(" ", String.Empty), Scheduler.Immediate);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentException));
				Assert.IsTrue(((ArgumentException)ex).ParamName == "term");
			}
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// successfully updates term in Xml and pushes it to subscribers.
		/// </summary>
		[TestMethod]
		public void ObservableUpdatesAndPushesTerm()
		{
			var fileName = XmlTermsServiceFixture.TermsToFile(
				XmlTermsServiceFixture.SampleTerms,
				"ObservableUpdatesAndPushesTerm.xml");

			var service = new XmlTermsService(fileName);
			var oldTerm = XmlTermsServiceFixture.SampleTerms[0];
			var newTerm = new Term("new term0", "new def 0");

			Term pushed = null;

			service
				.UpdateTerm(oldTerm, newTerm, Scheduler.Immediate)
				.Subscribe(p => pushed = p);

			Assert.AreEqual(newTerm, pushed);

			// Check that old term is deleted.

			var xTerm = XDocument.Load(fileName)
				.Descendants(TermXmlNames.TermElement)
				.FirstOrDefault(x => x.Attribute(TermXmlNames.TermNameAttribute).Value == oldTerm.Name);

			Assert.IsNull(xTerm);

			// Check that new term is added.

			xTerm = XDocument.Load(fileName)
				.Descendants(TermXmlNames.TermElement)
				.FirstOrDefault(x => x.Attribute(TermXmlNames.TermNameAttribute).Value == newTerm.Name);

			Assert.IsNotNull(xTerm);
			Assert.AreEqual(newTerm.Definition, xTerm.Attribute(TermXmlNames.TermDefinitionAttribute).Value);
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when updating term that doesn't exist.
		/// </summary>
		[TestMethod]
		public void ObservableThrowsOnUpdateNonExistentTerm()
		{
			var service = new XmlTermsService(XmlTermsServiceFixture.TermsToFile(
				XmlTermsServiceFixture.SampleTerms,
				"ObservableThrowsOnUpdateNonExistentTerm.xml"));
			var newTerm = new Term("nonExistentTerm", String.Empty);

			Exception ex = null;

			service
				.UpdateTerm(newTerm, newTerm, Scheduler.Immediate)
				.Subscribe(_ => { }, ex2 => ex = ex2, () => { });

			Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
			Assert.IsTrue(ex.Message.Contains("doesn't exist"));
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when updating to term that already exists.
		/// </summary>
		[TestMethod]
		public void ObservableThrowsOnUpdateAnotherExistentTerm()
		{
			var service = new XmlTermsService(XmlTermsServiceFixture.TermsToFile(
				XmlTermsServiceFixture.SampleTerms,
				"ObservableThrowsOnUpdateAnotherExistentTerm.xml"));
			var oldTerm = XmlTermsServiceFixture.SampleTerms[0];
			var newTerm = XmlTermsServiceFixture.SampleTerms[1];

			Exception ex = null;

			service
				.UpdateTerm(oldTerm, newTerm, Scheduler.Immediate)
				.Subscribe(_ => { }, ex2 => ex = ex2, () => { });

			Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
			Assert.IsTrue(ex.Message.Contains("already exists"));
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when updating null.
		/// </summary>
		[TestMethod]
		public void ServiceThrowsOnUpdateNullTerm()
		{
			// Check validation of old term.
			var service = new XmlTermsService(String.Empty);
			try
			{
				service.UpdateTerm(null, XmlTermsServiceFixture.SampleTerms[0], Scheduler.Immediate);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
				Assert.IsTrue(((ArgumentNullException)ex).ParamName == "oldTerm");
			}

			// Check validation of new term.
			service = new XmlTermsService(String.Empty);
			try
			{
				service.UpdateTerm(XmlTermsServiceFixture.SampleTerms[0], null, Scheduler.Immediate);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
				Assert.IsTrue(((ArgumentNullException)ex).ParamName == "newTerm");
			}
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when updating a term without a name.
		/// </summary>
		[TestMethod]
		public void ServiceThrowsOnUpdateTermWithoutName()
		{
			var service = new XmlTermsService(String.Empty);

			var nameless = new Term(null, String.Empty);
			var whitespace = new Term(" ", String.Empty);

			// Test null.
			try
			{
				service.UpdateTerm(nameless, XmlTermsServiceFixture.SampleTerms[0], Scheduler.Immediate);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentException));
				Assert.IsTrue(((ArgumentException)ex).ParamName == "oldTerm");
			}

			// Test whitespace.
			try
			{
				service.UpdateTerm(whitespace, XmlTermsServiceFixture.SampleTerms[0], Scheduler.Immediate);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentException));
				Assert.IsTrue(((ArgumentException)ex).ParamName == "oldTerm");
			}
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when updating to a term without a name.
		/// </summary>
		[TestMethod]
		public void ServiceThrowsOnUpdateToTermWithoutName()
		{
			var service = new XmlTermsService(String.Empty);

			var nameless = new Term(null, String.Empty);
			var whitespace = new Term(" ", String.Empty);

			// Test null.
			try
			{
				service.UpdateTerm(XmlTermsServiceFixture.SampleTerms[0], nameless, Scheduler.Immediate);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentException));
				Assert.IsTrue(((ArgumentException)ex).ParamName == "newTerm");
			}

			// Test whitespace.
			try
			{
				service.UpdateTerm(XmlTermsServiceFixture.SampleTerms[0], whitespace, Scheduler.Immediate);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentException));
				Assert.IsTrue(((ArgumentException)ex).ParamName == "newTerm");
			}
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// successfully removes term from Xml and pushes it to subscribers.
		/// </summary>
		[TestMethod]
		public void ObservableRemovesAndPushesTerm()
		{
			var fileName = XmlTermsServiceFixture.TermsToFile(
				XmlTermsServiceFixture.SampleTerms,
				"ObservableRemovesTerm.xml");

			var service = new XmlTermsService(fileName);
			var term = XmlTermsServiceFixture.SampleTerms[0];

			Term pushed = null;

			service
				.RemoveTerm(term, Scheduler.Immediate)
				.Subscribe(p => pushed = p);

			Assert.AreEqual(term, pushed);

			var xTerm = XDocument.Load(fileName)
				.Descendants(TermXmlNames.TermElement)
				.FirstOrDefault(x => x.Attribute(TermXmlNames.TermNameAttribute).Value == term.Name);

			Assert.IsNull(xTerm);
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when removing term that doesn't exist.
		/// </summary>
		[TestMethod]
		public void ObservableThrowsOnRemoveNonExistentTerm()
		{
			var service = new XmlTermsService(XmlTermsServiceFixture.TermsToFile(
				XmlTermsServiceFixture.SampleTerms,
				"ObservableThrowsOnRemoveNonExistentTerm.xml"));
			var newTerm = new Term("nonExistentTerm", String.Empty);

			Exception ex = null;

			service
				.RemoveTerm(newTerm, Scheduler.Immediate)
				.Subscribe(_ => { }, ex2 => ex = ex2, () => { });

			Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
			Assert.IsTrue(ex.Message.Contains("doesn't exist"));
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when removing null.
		/// </summary>
		[TestMethod]
		public void ServiceThrowsOnRemoveNullTerm()
		{
			var service = new XmlTermsService(String.Empty);
			try
			{
				service.RemoveTerm(null, Scheduler.Immediate);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
				Assert.IsTrue(((ArgumentNullException)ex).ParamName == "term");
			}
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when removing a term without a name.
		/// </summary>
		[TestMethod]
		public void ServiceThrowsOnRemoveTermWithoutName()
		{
			var service = new XmlTermsService(String.Empty);

			// Test null.
			try
			{
				service.RemoveTerm(new Term(null, String.Empty), Scheduler.Immediate);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentException));
				Assert.IsTrue(((ArgumentException)ex).ParamName == "term");
			}

			// Test whitespace.
			try
			{
				service.RemoveTerm(new Term(" ", String.Empty), Scheduler.Immediate);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentException));
				Assert.IsTrue(((ArgumentException)ex).ParamName == "term");
			}
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// recreates valid storage with the specified terms.
		/// </summary>
		[TestMethod]
		public void ObservableRecreatesNewStorage()
		{
			var filePath = Path.Combine(XmlTermsServiceFixture.DeploymentDirectory, "ObservableRecreatesNewStorage.xml");
			var service = new XmlTermsService(filePath);
			
			service
				.RecreateStorage(XmlTermsServiceFixture.SampleTerms, Scheduler.Immediate)
				.Subscribe();

			var storedTerms = new List<Term>();

			service
				.LoadTerms(Scheduler.Immediate)
				.Subscribe(terms => storedTerms.AddRange(terms));

			ReactiveAssert.AreElementsEqual(XmlTermsServiceFixture.SampleTerms, storedTerms);
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when recreating null.
		/// </summary>
		[TestMethod]
		public void ServiceThrowsOnRecreateNothing()
		{
			var service = new XmlTermsService(String.Empty);
			try
			{
				service.RecreateStorage(null, Scheduler.Immediate);
				Assert.Fail();
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
				Assert.IsTrue(((ArgumentNullException)ex).ParamName == "terms");
			}
		}

		/// <summary>
		/// Tests whether an instance of the <see cref="XmlTermsService"/> class
		/// throws exception when recreating storage with invalid terms.
		/// </summary>
		[TestMethod]
		public void ObservableThrowsOnRecreateInvalidTerms()
		{
			var service = new XmlTermsService(String.Empty);

			// Test null.

			var invalidTerms = new Term[] 
				{ 
					XmlTermsServiceFixture.SampleTerms[0],
					new Term(null, String.Empty),
				};
			
			Exception ex = null;

			service
				.RecreateStorage(invalidTerms, Scheduler.Immediate)
				.Subscribe(_ => { }, ex2 => ex = ex2, () => { });

			Assert.IsInstanceOfType(ex, typeof(InvalidTermsStorageException));

			// Test whitespace.

			invalidTerms = new Term[] 
				{ 
					XmlTermsServiceFixture.SampleTerms[0],
					new Term(" ", String.Empty),
				};

			ex = null;

			service
				.RecreateStorage(invalidTerms, Scheduler.Immediate)
				.Subscribe(_ => { }, ex2 => ex = ex2, () => { });

			Assert.IsInstanceOfType(ex, typeof(InvalidTermsStorageException));
		}

		#region Helpers

		/// <summary>
		/// Creates Xml file with the specified terms.
		/// </summary>
		/// <param name="terms">A list of terms to use to create Xml file.</param>
		/// <param name="fileName">A name of file to create.</param>
		/// <returns>A full path to a file with the specified terms.</returns>
		private static string TermsToFile(IEnumerable<Term> terms, string fileName)
		{
			fileName = Path.Combine(XmlTermsServiceFixture.DeploymentDirectory, fileName);

			new XElement(
				TermXmlNames.TermsElement,
				terms.Select(term =>
					{
						var xTerm = new XElement(TermXmlNames.TermElement);
						if (term.Name != null)
						{
							xTerm.Add(new XAttribute(TermXmlNames.TermNameAttribute, term.Name));
						}
						if (!String.IsNullOrEmpty(term.Definition))
						{
							xTerm.Add(new XAttribute(TermXmlNames.TermDefinitionAttribute, term.Definition));
						}

						return xTerm;
					}))
			.Save(fileName);

			return fileName;
		}

		#endregion
	}
}
