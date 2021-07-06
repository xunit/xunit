using System;

namespace Xunit.v3
{
	/// <summary>
	/// Represents an implementation of the discovery part of a test framework.
	/// Implementations may optionally implement <see cref="IDisposable"/> and/or <see cref="IAsyncDisposable"/>
	/// for cleanup operations.
	/// </summary>
	public interface _ITestFrameworkDiscoverer
	{
		/// <summary>
		/// Gets the target framework that the test assembly is linked against.
		/// </summary>
		string TargetFramework { get; }

		/// <summary>
		/// Gets the unique ID for the test assembly provided to the discoverer.
		/// </summary>
		string TestAssemblyUniqueID { get; }

		/// <summary>
		/// Returns the display name of the test framework that this discoverer is running tests for.
		/// </summary>
		string TestFrameworkDisplayName { get; }

		/// <summary>
		/// Starts the process of finding all tests in an assembly. It is expected that this
		/// method spawns a worker thread to do work, and returns immediately, while reporting
		/// status back through the <paramref name="discoveryMessageSink"/>.
		/// </summary>
		/// <param name="discoveryMessageSink">The message sink to report results back to.</param>
		/// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
		void Find(
			_IMessageSink discoveryMessageSink,
			_ITestFrameworkDiscoveryOptions discoveryOptions
		);

		/// <summary>
		/// Starts the process of finding all tests in a class. It is expected that this
		/// method spawns a worker thread to do work, and returns immediately, while reporting
		/// status back through the <paramref name="discoveryMessageSink"/>.
		/// </summary>
		/// <param name="typeName">The fully qualified type name to find tests in.</param>
		/// <param name="discoveryMessageSink">The message sink to report results back to.</param>
		/// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
		void Find(
			string typeName,
			_IMessageSink discoveryMessageSink,
			_ITestFrameworkDiscoveryOptions discoveryOptions
		);
	}
}
