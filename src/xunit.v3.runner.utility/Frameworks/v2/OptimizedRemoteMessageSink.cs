#pragma warning disable CA1812    // This class is instantiated in a remote AppDomain
#pragma warning disable xUnit3000 // This class does not have direct access to v2 xunit.runner.utility, so it can't derive from v2's LLMBRO

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Runner.v2;

/// <summary>
/// This class exists to live inside the v2 remote AppDomain and provide an optimized message
/// sink which calls through to a runner-side implementation of <see cref="IMessageSinkMessageWithTypes"/>.
/// This allows higher performance type dispatching, since retrieving and passing along the remote-side
/// interface list is much faster than attempting to do cross-AppDomain casts. This class is created
/// remotely in <see cref="Xunit2.CreateOptimizedRemoteMessageSink"/>.
/// </summary>
sealed class OptimizedRemoteMessageSink : MarshalByRefObject, IMessageSink, IDisposable
{
	readonly ReaderWriterLockSlim cacheLock = new();
	readonly Dictionary<Type, HashSet<string>> interfaceCache = [];
	readonly IMessageSinkWithTypes runnerSink;

	public OptimizedRemoteMessageSink(IMessageSinkWithTypes runnerSink)
	{
		Guard.ArgumentNotNull(runnerSink);

		this.runnerSink = runnerSink;
	}

	public void Dispose() =>
		cacheLock.Dispose();

	HashSet<string> GetMessageTypes(IMessageSinkMessage message)
	{
		var messageType = message.GetType();
		HashSet<string>? result;

		cacheLock.TryEnterReadLock(-1);

		try
		{
			interfaceCache.TryGetValue(messageType, out result);
		}
		finally
		{
			cacheLock.ExitReadLock();
		}

		if (result is null)
		{
			cacheLock.TryEnterWriteLock(-1);

			try
			{
				result = new HashSet<string>(messageType.GetInterfaces().Select(x => x.FullName!));
				interfaceCache[messageType] = result;
			}
			finally
			{
				cacheLock.ExitWriteLock();
			}
		}

		return result;
	}

#if NETFRAMEWORK
	/// <inheritdoc/>
	[System.Security.SecurityCritical]
	public sealed override object InitializeLifetimeService() => null!;
#endif

	public bool OnMessage(IMessageSinkMessage? message) =>
		message is null || runnerSink.OnMessageWithTypes(message, GetMessageTypes(message));
}
