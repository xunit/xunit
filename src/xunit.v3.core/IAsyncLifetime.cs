using System;
using System.Threading.Tasks;

namespace Xunit
{
	/// <summary>
	/// Used to provide asynchronous lifetime functionality. Currently supported:
	/// - Test classes
	/// - Classes used in <see cref="IClassFixture{TFixture}"/>
	/// - Classes used in <see cref="ICollectionFixture{TFixture}"/>.
	/// </summary>
	public interface IAsyncLifetime : IAsyncDisposable
	{
		/// <summary>
		/// Called immediately after the class has been created, before it is used.
		/// </summary>
		ValueTask InitializeAsync();
	}
}
