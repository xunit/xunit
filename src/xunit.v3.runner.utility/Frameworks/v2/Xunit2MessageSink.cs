#pragma warning disable xUnit3000 // This class does not have direct access to v2 xunit.runner.utility, so it can't derive from v2's LLMBRO

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Runner.v2;

/// <summary>
/// A message sink which implements both <see cref="Abstractions.IMessageSink"/> and <see cref="IMessageSinkWithTypes"/>
/// which adapts (with <see cref="Xunit2MessageAdapter"/> and dispatches any incoming v2 messages to the
/// given v3 message sink.
/// </summary>
/// <param name="v3MessageSink">The v3 message sink to which to report the messages</param>
/// <param name="assemblyUniqueID">The unique ID of the assembly these message belong to</param>
/// <param name="discoverer">The discoverer used to serialize test cases</param>
public class Xunit2MessageSink(
	Sdk.IMessageSink v3MessageSink,
	string? assemblyUniqueID = null,
	ITestFrameworkDiscoverer? discoverer = null) :
		MarshalByRefObject, IMessageSink, IMessageSinkWithTypes
{
	readonly Xunit2MessageAdapter adapter = new(assemblyUniqueID, discoverer);
	readonly Sdk.IMessageSink v3MessageSink = Guard.ArgumentNotNull(v3MessageSink);

	/// <inheritdoc/>
	public void Dispose()
	{
		GC.SuppressFinalize(this);

		(v3MessageSink as IDisposable)?.SafeDispose();
	}

	static HashSet<string>? GetImplementedInterfaces(IMessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		if (message is IMessageSinkMessageWithTypes messageWithTypes)
			return messageWithTypes.InterfaceTypes;

#if NETFRAMEWORK
		// Can't get the list of interfaces across the remoting boundary
		if (System.Runtime.Remoting.RemotingServices.IsTransparentProxy(message))
			return null;
#endif

		return new(message.GetType().GetInterfaces().Select(i => i.FullName!), StringComparer.OrdinalIgnoreCase)
		{
			// TODO: Hack this to include the concrete type, while we transition from v2 to v3 so that we
			// can support our new message types which aren't interfaces.
			message.GetType().FullName!
		};
	}

#if NETFRAMEWORK
	/// <inheritdoc/>
	[System.Security.SecurityCritical]
	public sealed override object InitializeLifetimeService() => null!;
#endif

	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		return OnMessageWithTypes(message, GetImplementedInterfaces(message));
	}

	/// <inheritdoc/>
	public bool OnMessageWithTypes(
		IMessageSinkMessage message,
		HashSet<string>? messageTypes)
	{
		Guard.ArgumentNotNull(message);

		var v3Message = adapter.Adapt(message, messageTypes);
		return v3MessageSink.OnMessage(v3Message);
	}
}
