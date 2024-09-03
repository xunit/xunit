#if NETFRAMEWORK
using System;
using System.Security;
#endif

namespace Xunit.Sdk;

#if NETFRAMEWORK
/// <summary>
/// Base class for all long-lived objects that may cross over an AppDomain.
/// </summary>
public abstract class LongLivedMarshalByRefObject : MarshalByRefObject
{
	/// <inheritdoc/>
	[SecurityCritical]
	public sealed override object InitializeLifetimeService() => null!;
}
#else
/// <summary>
/// Base class for all long-lived objects that may cross over an AppDomain.
/// </summary>
public abstract class LongLivedMarshalByRefObject
{ }
#endif
