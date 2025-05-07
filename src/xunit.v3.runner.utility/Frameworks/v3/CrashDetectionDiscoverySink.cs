using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="IMessageSink"/> that detects when test discovery starting
/// and finished messages have been sent, and will send cleanup/error messages to the inner
/// sink if it appears that the test process crashed (failed to send one or both messages).
/// </summary>
/// <param name="projectAssembly">The test project assembly</param>
/// <param name="innerSink">The inner message sink</param>
public class CrashDetectionDiscoverySink(
	XunitProjectAssembly projectAssembly,
	IMessageSink innerSink) :
		CrashDetectionSinkBase<IDiscoveryStarting, IDiscoveryComplete>(projectAssembly, innerSink)
{
	/// <inheritdoc/>
	protected override void SendFinish(string assemblyUniqueID) =>
		InnerSink.OnMessage(new DiscoveryComplete
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestCasesToRun = 0,
		});

	/// <inheritdoc/>
	protected override void SendStart(string assemblyUniqueID) =>
		InnerSink.OnMessage(new DiscoveryStarting
		{
			AssemblyName = ProjectAssembly.AssemblyDisplayName,
			AssemblyPath = ProjectAssembly.AssemblyFileName,
			AssemblyUniqueID = assemblyUniqueID,
			ConfigFilePath = ProjectAssembly.ConfigFileName,
		});
}
