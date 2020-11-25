#if NETFRAMEWORK
using System;
using System.Security;
#endif

namespace Xunit.Runner.v2
{
#if NETFRAMEWORK
	/// <summary>
	/// Base class for all long-lived objects that may cross over an AppDomain.
	/// </summary>
	public abstract class LongLivedMarshalByRefObject : MarshalByRefObject
	{
		/// <inheritdoc/>
		[SecurityCritical]
		public override sealed object InitializeLifetimeService()
		{
			return null!;  // MarshalByRefObject isn't properly annotated for nullability yet
		}
	}
#else
	/// <summary>
	/// Base class for all long-lived objects that may cross over an AppDomain.
	/// </summary>
	public abstract class LongLivedMarshalByRefObject
	{ }
#endif
}
