namespace Xunit;

/// <summary>
/// Used to provide asynchronous lifetime functionality. Currently supported:
/// <list type="bullet">
/// <item>Test classes</item>
/// <item>Classes used in <see cref="IClassFixture{TFixture}"/></item>
/// <item>Classes used in <see cref="ICollectionFixture{TFixture}"/></item>
/// <item>Classes used in <c>[assembly: <see cref="AssemblyFixtureAttribute"/>]</c></item>
/// </list>
/// </summary>
public interface IAsyncLifetime : IAsyncDisposable
{
	/// <summary>
	/// Called immediately after the class has been created, before it is used.
	/// </summary>
	ValueTask InitializeAsync();
}
