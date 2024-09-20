#pragma warning disable xUnit3000 // This class does not have direct access to v2 xunit.runner.utility, so it can't derive from v2's LLMBRO

using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Runner.Common;

namespace Xunit.Runner.v2;

/// <summary>
/// An implementation of <see cref="IMessageSink"/> and <see cref="IMessageSinkWithTypes"/> which
/// collects native xUnit.net v2 test cases, for use with <see cref="Xunit2.FindAndRun"/>.
/// </summary>
/// <param name="assemblyName">The assembly name that this discovery sink is discovering</param>
/// <param name="filters">The filters to be applied to the discovered test cases</param>
public class Xunit2DiscoverySink(
	string assemblyName,
	XunitFilters filters) :
		MarshalByRefObject, IMessageSink, IMessageSinkWithTypes
{
	readonly Xunit2MessageAdapter adapter = new();
	bool disposed;

	/// <summary>
	/// Gets an event which is signaled once discovery is finished.
	/// </summary>
	public AutoResetEvent Finished { get; } = new(initialState: false);

	/// <summary>
	/// The list of discovered test cases.
	/// </summary>
	public List<ITestCase> TestCases { get; } = [];

	static void Dispatch<TMessage>(
		IMessageSinkMessage message,
		HashSet<string>? messageTypes,
		Action<TMessage> handler)
			where TMessage : class, IMessageSinkMessage
	{
		var castMessage = messageTypes is null || messageTypes.Contains(typeof(TMessage).FullName!) ? message as TMessage : null;
		if (castMessage is not null)
			handler(castMessage);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (disposed)
			return;

		disposed = true;

		GC.SuppressFinalize(this);

		Finished.Dispose();
	}

	void HandleDiscoveryComplete(IDiscoveryCompleteMessage message)
	{
		if (disposed)
			return;

		Finished.Set();
	}

	void HandleTestCaseDiscovery(ITestCaseDiscoveryMessage message)
	{
		if (disposed)
			return;

		if (filters.Empty || (adapter.Adapt(message) is TestCaseDiscovered adapted && filters.Filter(assemblyName, adapted)))
			TestCases.Add(message.TestCase);
	}

#if NETFRAMEWORK
	/// <inheritdoc/>
	[System.Security.SecurityCritical]
	public sealed override object InitializeLifetimeService() => null!;
#endif

	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message) =>
		OnMessageWithTypes(message, null);

	/// <inheritdoc/>
	public bool OnMessageWithTypes(
		IMessageSinkMessage message,
		HashSet<string>? messageTypes)
	{
		Dispatch<IDiscoveryCompleteMessage>(message, messageTypes, HandleDiscoveryComplete);
		Dispatch<ITestCaseDiscoveryMessage>(message, messageTypes, HandleTestCaseDiscovery);

		return true;
	}
}
