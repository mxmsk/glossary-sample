using System.Windows;

using Autofac;
using Glossary.Terms;
using Glossary.Terms.Views;

namespace Glossary
{
	/// <summary>
	/// Represents the main window of Glossary application.
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MainWindow"/> class.
		/// </summary>
		public MainWindow()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule<TermsModule>();

			this.DataContext = builder.Build().Resolve<ITermListView>();
			this.InitializeComponent();
		}
	}
}