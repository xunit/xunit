using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Captures <see cref="Trace"/> and <see cref="Debug"/> output and reports it to the
/// test output helper.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class CaptureTraceAttribute : BeforeAfterTestAttribute
{
	static int initializedCounter;
	static readonly ManualResetEventSlim initializedEvent = new(initialState: false);

	/// <inheritdoc/>
	public override void Before(
		MethodInfo methodUnderTest,
		IXunitTest test)
	{
		if (Interlocked.Exchange(ref initializedCounter, 1) == 0)
		{
#pragma warning disable CA1806
#pragma warning disable CA2000

			// We don't need to retain or dispose of the value, because all the work is done in the
			// constructor, and usage here is intended to be a system-wide hook.
			new TraceCaptureTestOutputWriter(TestContextAccessor.Instance);

#pragma warning restore CA2000
#pragma warning restore CA1806

			// Use MRES to ensure nobody starts running until the initializer has finished. The wait
			// time for MRES.Wait() for something that's already signaled is as close to zero as we
			// can get, because it just checks a local field and returns when already signaled.
			initializedEvent.Set();
		}
		else
			initializedEvent.Wait();
	}
}
