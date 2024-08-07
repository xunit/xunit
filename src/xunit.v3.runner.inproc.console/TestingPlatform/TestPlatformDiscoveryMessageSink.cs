using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Messages;
using Microsoft.Testing.Platform.TestHost;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

/// <summary>
/// Implementation of <see cref="IMessageSink"/> and <see cref="IDataProducer"/> in order to convert
/// xUnit.net v3 discovery messages into Microsoft.Testing.Platform <see cref="TestNode"/> messages.
/// </summary>
/// <param name="innerSink">The inner sink to delegate messages to</param>
/// <param name="assemblyFullName">The full name of the test assembly (via <see cref="Assembly.FullName"/>)</param>
/// <param name="sessionUid">The MTP session UID</param>
/// <param name="testNodeMessageBus">The message bus for reporting MTP messages to</param>
/// <param name="cancellationToken">The cancellation token provided by MTP</param>
[ExcludeFromCodeCoverage]
public class TestPlatformDiscoveryMessageSink(
	IMessageSink innerSink,
	string assemblyFullName,
	SessionUid sessionUid,
	IMessageBus testNodeMessageBus,
	CancellationToken cancellationToken) :
		ExtensionBase("discovery message sink", "b1ef01c2-95f4-4411-b6ef-19e290225124"), IMessageSink, IDataProducer
{
	/// <inheritdoc/>
	public Type[] DataTypesProduced =>
		[typeof(TestNodeUpdateMessage)];

	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		var result = innerSink.OnMessage(message);

		return
			message.DispatchWhen<ITestCaseDiscovered>(OnTestCaseDiscovered) &&
			result &&
			!cancellationToken.IsCancellationRequested;
	}

	void OnTestCaseDiscovered(MessageHandlerArgs<ITestCaseDiscovered> args)
	{
		var discovered = args.Message;

		var result = new TestNode { Uid = discovered.TestCaseUniqueID, DisplayName = discovered.TestCaseDisplayName };
		result.Properties.Add(DiscoveredTestNodeStateProperty.CachedInstance);
		result.AddMetadata(discovered, assemblyFullName);
		result.SendUpdate(this, sessionUid, testNodeMessageBus);
	}
}
