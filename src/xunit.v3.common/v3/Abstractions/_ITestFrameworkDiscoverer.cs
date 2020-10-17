using System;
using Xunit.Abstractions;

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
		/// Returns the display name of the test framework that this discoverer is running tests for.
		/// </summary>
		string TestFrameworkDisplayName { get; }

		/// <summary>
		/// Starts the process of finding all tests in an assembly.
		/// </summary>
		/// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
		/// <param name="discoveryMessageSink">The message sink to report results back to.</param>
		/// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
		void Find(
			bool includeSourceInformation,
			IMessageSink discoveryMessageSink,
			ITestFrameworkDiscoveryOptions discoveryOptions
		);

		/// <summary>
		/// Starts the process of finding all tests in a class.
		/// </summary>
		/// <param name="typeName">The fully qualified type name to find tests in.</param>
		/// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
		/// <param name="discoveryMessageSink">The message sink to report results back to.</param>
		/// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
		void Find(
			string typeName,
			bool includeSourceInformation,
			IMessageSink discoveryMessageSink,
			ITestFrameworkDiscoveryOptions discoveryOptions
		);

		// TODO: Can we get away without Serialize?

		/// <summary>
		/// Serializes a test case into string form.
		/// </summary>
		/// <param name="testCase">The test case to be serialized.</param>
		/// <returns>The serialized representation of the test case.</returns>
		string Serialize(ITestCase testCase);
	}
}
