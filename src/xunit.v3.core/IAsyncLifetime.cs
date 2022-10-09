using System;
using System.Threading.Tasks;

namespace Xunit;

/// <summary>
/// Used to provide asynchronous lifetime functionality. Currently supported:<br />
/// - Test classes<br />
/// - Classes used in <see cref="IClassFixture{TFixture}"/><br />
/// - Classes used in <see cref="ICollectionFixture{TFixture}"/>.<br />
/// - Classes used in <c>[assembly: <see cref="AssemblyFixtureAttribute"/>()]</c>.
/// </summary>
public interface IAsyncLifetime : IAsyncDisposable
{
	/// <summary>
	/// Called immediately after the class has been created, before it is used.
	/// </summary>
	ValueTask InitializeAsync();
}
