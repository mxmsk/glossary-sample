using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Autofac;
using Autofac.Core;

using Glossary.Terms.Services;
using Glossary.Terms.Views;

namespace Glossary.Terms
{
	/// <summary>
	/// Provides Terms components to Autofac container.
	/// </summary>
	public sealed class TermsModule : Module
	{
		/// <summary>
		/// Adds registrations to the container.
		/// </summary>
		/// <param name="builder">The builder through which components can be registered.</param>
		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c => new XmlTermsService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Terms.xml")))
				.As<ITermsService>()
				.SingleInstance(); 

			builder.Register(c => new TermListViewModel(c.Resolve<ITermsService>(), c.Resolve<ITermEditViewModel>()))
				.As<ITermListViewModel>();
			builder.Register(c => new TermListView(c.Resolve<ITermListViewModel>()))
				.As<ITermListView>();

			builder.Register(c => new TermEditViewModel())
				.As<ITermEditViewModel>();
		}
	}
}
