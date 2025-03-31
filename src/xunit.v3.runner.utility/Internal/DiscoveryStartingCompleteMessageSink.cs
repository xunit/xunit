using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Internal;

// Delegates to fabricate IDiscoveryStarting and IDiscoveryComplete when using ITestProcessLauncher
// since the invocation via `-list` does not send starting or complete messages over the console

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public sealed class DiscoveryStartingCompleteMessageSink(
	string assemblyDisplayName,
	string assemblyFileName,
	string? configFileName,
	IMessageSink innerSink) :
		IMessageSink
{
	string? assemblyUniqueID;
	int testCaseCount;

	/// <summary/>
	public DiscoveryStartingCompleteMessageSink(
		XunitProjectAssembly projectAssembly,
		IMessageSink innerSink) :
			this(
				Guard.ArgumentNotNull(projectAssembly).AssemblyDisplayName,
				projectAssembly.AssemblyFileName,
				projectAssembly.ConfigFileName,
				innerSink
			)
	{ }

	/// <summary/>
	public void Finish()
	{
		// Fabricate the assembly unique ID and send starting if we never saw any test cases
		if (assemblyUniqueID is null)
		{
			assemblyUniqueID = UniqueIDGenerator.ForAssembly(assemblyFileName, configFileName);
			SendDiscoveryStarting(assemblyUniqueID);
		}

		SendDiscoveryComplete(assemblyUniqueID);
	}

	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		if (message is not ITestCaseDiscovered testDiscovered)
			return innerSink.OnMessage(message);

		var @continue = true;

		if (assemblyUniqueID is null)
		{
			assemblyUniqueID = testDiscovered.AssemblyUniqueID;
			@continue = SendDiscoveryStarting(assemblyUniqueID);
		}

		++testCaseCount;
		return innerSink.OnMessage(testDiscovered) && @continue;
	}

	bool SendDiscoveryComplete(string uniqueID) =>
		innerSink.OnMessage(new DiscoveryComplete
		{
			AssemblyUniqueID = uniqueID,
			TestCasesToRun = testCaseCount,
		});

	bool SendDiscoveryStarting(string uniqueID) =>
		innerSink.OnMessage(new DiscoveryStarting
		{
			AssemblyName = assemblyDisplayName,
			AssemblyPath = assemblyFileName,
			AssemblyUniqueID = uniqueID,
			ConfigFilePath = configFileName,
		});
}
