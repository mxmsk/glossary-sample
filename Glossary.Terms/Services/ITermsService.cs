using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Reactive.Concurrency;

namespace Glossary.Terms.Services
{
	/// <summary>
	/// Defines a service that allows to access a storage of glossary terms. 
	/// </summary>
	public interface ITermsService
	{
		/// <summary>
		/// Gets an observable sequence that retrieves terms from the storage.
		/// </summary>
		/// <param name="scheduler">A scheduler to invoke service operations on.</param>
		/// <returns>An observable sequence of portions of terms.</returns>
		IObservable<IEnumerable<Term>> LoadTerms(IScheduler scheduler);

		/// <summary>
		/// Gets an observable that adds a new term to the storage.
		/// </summary>
		/// <param name="term">A term to add to the storage.</param>
		/// <param name="scheduler">A scheduler to invoke service operations on.</param>
		/// <returns>An observable sequence that adds the specified term and pushes
		/// it to subscribers.</returns>
		IObservable<Term> AddTerm(Term term, IScheduler scheduler);

		/// <summary>
		/// Gets an observable that updates an existing term in the storage.
		/// </summary>
		/// <param name="oldTerm">An old value of term to update in the storage.</param>
		/// <param name="newTerm">A new value of term.</param>
		/// <param name="scheduler">A scheduler to invoke service operations on.</param>
		/// <returns>An observable sequence that updates the specified term and pushes
		/// it to subscribers.</returns>
		IObservable<Term> UpdateTerm(Term oldTerm, Term newTerm, IScheduler scheduler);

		/// <summary>
		/// Gets an observable that removes an existing term from the storage.
		/// </summary>
		/// <param name="term">A term to remove from the storage.</param>
		/// <param name="scheduler">A scheduler to invoke service operations on.</param>
		/// <returns>An observable sequence that removes the specified term and pushes
		/// it to subscribers.</returns>
		IObservable<Term> RemoveTerm(Term term, IScheduler scheduler);

		/// <summary>
		/// Gets an observable that recreates storage with the specified terms.
		/// </summary>
		/// <param name="terms">A terms to add to a new storage.</param>
		/// <param name="scheduler">A scheduler to invoke service operations on.</param>
		/// <returns>An observable sequence that recreates storage.</returns>
		IObservable<Unit> RecreateStorage(IEnumerable<Term> terms, IScheduler scheduler);
	}
}
