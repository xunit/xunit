using System;

namespace Xunit.v3
{
	/// <summary>
	/// Represents a test framework. There are two pieces to test frameworks: discovery and
	/// execution. The two factory methods represent these two pieces. Test frameworks can
	/// implement an empty constructor, or they can implement one that takes <see cref="_IMessageSink"/>
	/// if they want to be able to send diagnostic messages. It may optionally implement
	/// either <see cref="IDisposable"/> or <see cref="IAsyncDisposable"/>.
	/// </summary>
	// TODO: Do we still think this is the right way to do constructors?
	public interface _ITestFramework
	{
		/// <summary>
		/// Get a test discoverer.
		/// </summary>
		/// <param name="assembly">The assembly to discover tests from.</param>
		/// <returns>The test discoverer.</returns>
		_ITestFrameworkDiscoverer GetDiscoverer(_IAssemblyInfo assembly);

		/// <summary>
		/// Get a test executor.
		/// </summary>
		/// <param name="assembly">The assembly to run tests from.</param>
		/// <returns>The test executor.</returns>
		_ITestFrameworkExecutor GetExecutor(_IReflectionAssemblyInfo assembly);
	}
}
