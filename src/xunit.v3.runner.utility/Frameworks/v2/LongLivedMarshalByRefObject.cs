using System;

namespace Xunit.Sdk;

/// <summary>
/// Base class for all long-lived objects that may cross over an AppDomain.
/// </summary>
public abstract class LongLivedMarshalByRefObject : MarshalByRefObject
{
#if NETFRAMEWORK
	/// <inheritdoc/>
	[System.Security.SecurityCritical]
	public sealed override object InitializeLifetimeService() => null!;
#endif
}
