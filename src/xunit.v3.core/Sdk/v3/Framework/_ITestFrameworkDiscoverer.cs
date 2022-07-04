using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents an implementation of the discovery part of a test framework. Implementations may
/// optionally implement <see cref="IDisposable"/> and/or <see cref="IAsyncDisposable"/>
/// for cleanup operations.
/// </summary>
public interface _ITestFrameworkDiscoverer
{
	/// <summary>
	/// Gets the target framework that the test assembly is linked against.
	/// </summary>
	string TargetFramework { get; }

	/// <summary>
	/// Gets the test assembly provided to the discoverer.
	/// </summary>
	_ITestAssembly TestAssembly { get; }

	/// <summary>
	/// Returns the display name of the test framework that this discoverer is running tests for.
	/// </summary>
	string TestFrameworkDisplayName { get; }

	/// <summary>
	/// Finds tests inside an assembly. Calls the callback for each discovered test.
	/// </summary>
	/// <param name="callback">Called for each discovered test. Return <c>true</c> to continue test
	/// discovery; return <c>false</c> to cancel test discovery.</param>
	/// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
	/// <param name="types">When passed a non-<c>null</c> collection, only returns tests found
	/// from one of the provided types; when passed a <c>null</c> collection, discovers all
	/// tests in the assembly.</param>
	/// <param name="cancellationToken">The cancellation token which can be used to cancel the test
	/// discovery process.</param>
	ValueTask Find(
		Func<_ITestCase, ValueTask<bool>> callback,
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		Type[]? types = null,
		CancellationToken? cancellationToken = null
	);
}
