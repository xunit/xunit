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
	/// <remarks>
	/// If this method throws an exception, then <see cref="IAsyncDisposable.DisposeAsync"/>
	/// will not be called. This mirrors the behavior of constructors and usage of
	/// <see cref="IDisposable"/>/<see cref="IAsyncDisposable"/>, since this method
	/// is intended as the equivalent of an async constructor.
	/// </remarks>
	ValueTask InitializeAsync();
}
