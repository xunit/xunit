using System;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test framework. There are two pieces to test frameworks: discovery and
/// execution. The two factory methods represent these two pieces. Test frameworks may optionally
/// implement either <see cref="IDisposable"/> or <see cref="IAsyncDisposable"/>. They may
/// implement a constructor which is either empty, or takes a single <see cref="IMessageSink"/>
/// for diagnostic messages, or takes two instances of <see cref="IMessageSink"/> for diagnostic
/// messages and internal diagnostic messages, respectively.
/// </summary>
// TODO: Do we still think this is the right way to do constructors?
public interface ITestFramework
{
	/// <summary>
	/// Returns the display name of the test framework that this discoverer is running tests for.
	/// </summary>
	string TestFrameworkDisplayName { get; }

	/// <summary>
	/// Will be invoked by the test pipeline when the user has provided a startup class. Test frameworks may
	/// choose to use this in any way they see fit.
	/// </summary>
	void SetTestPipelineStartup(ITestPipelineStartup pipelineStartup);

	/// <summary>
	/// Get a test discoverer.
	/// </summary>
	/// <param name="assembly">The assembly to discover tests from.</param>
	/// <returns>The test discoverer.</returns>
	ITestFrameworkDiscoverer GetDiscoverer(Assembly assembly);

	/// <summary>
	/// Get a test executor.
	/// </summary>
	/// <param name="assembly">The assembly to run tests from.</param>
	/// <returns>The test executor.</returns>
	ITestFrameworkExecutor GetExecutor(Assembly assembly);
}
