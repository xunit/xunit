#pragma warning disable xUnit3000 // This class does not have direct access to v2 xunit.runner.utility, so it can't derive from v2's LLMBRO

using System.ComponentModel;
using Xunit.Abstractions;
using Xunit.Runner.Common;

namespace Xunit.Runner.v2;

/// <summary>
/// An implementation of <see cref="IMessageSink"/> and <see cref="IMessageSinkWithTypes"/> which
/// collects native xUnit.net v2 test cases, for use with <see cref="Xunit2.FindAndRun"/>.
/// </summary>
/// <param name="assemblyName">The assembly name that this discovery sink is discovering</param>
/// <param name="filters">The filters to be applied to the discovered test cases</param>
/// <param name="diagnosticMessageSink">The optional diagnostic message sink to report issues to</param>
public class Xunit2DiscoverySink(
	string assemblyName,
	XunitFilters filters,
	Sdk.IMessageSink? diagnosticMessageSink) :
		MarshalByRefObject, IMessageSink, IMessageSinkWithTypes
{
	/// <summary>
	/// Please use <see cref="Xunit2DiscoverySink(string, XunitFilters, Sdk.IMessageSink?)"/>.
	/// This overload will be removed in the next major version.
	/// </summary>
	[Obsolete("Please use the constructor which accepts diagnosticMessageSink. This overload will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public Xunit2DiscoverySink(
		string assemblyName,
		XunitFilters filters) :
			this(assemblyName, filters, null)
	{ }

	readonly Xunit2MessageAdapter adapter = new();
	bool disposed;
	readonly Dictionary<string, ITestCase> testCasesByID = [];

	/// <summary>
	/// Gets an event which is signaled once discovery is finished.
	/// </summary>
	public AutoResetEvent Finished { get; } = new(initialState: false);

	/// <summary>
	/// The list of discovered test cases.
	/// </summary>
	public IReadOnlyList<ITestCase> TestCases => testCasesByID.Values.CastOrToReadOnlyList();

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

		var testCase = message.TestCase;

		if (testCasesByID.ContainsKey(testCase.UniqueID))
		{
			diagnosticMessageSink?.OnMessage(
				new DiagnosticMessage(
					"Warning: Rejecting v2 test case with duplicate unique ID{0}ID:      {1}{0}Name:    {2}{0}Method:  {3}",
					Environment.NewLine,
					testCase.UniqueID,
					testCase.DisplayName,
					testCase.TestMethod is null || testCase.TestMethod.TestClass is null ? "null" : $"{testCase.TestMethod.TestClass.Class.Name}.{testCase.TestMethod.Method.Name}"
				)
			);

			return;
		}

		if (filters.Empty || (adapter.Adapt(message) is TestCaseDiscovered adapted && filters.Filter(assemblyName, adapted)))
			testCasesByID[testCase.UniqueID] = message.TestCase;
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
