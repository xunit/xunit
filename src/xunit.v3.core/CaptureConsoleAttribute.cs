using System;
using System.Reflection;
using System.Threading;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Captures <see cref="Console"/> output (<see cref="Console.Out"/> and/or <see cref="Console.Error"/>)
/// and reports it to the test output helper.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class CaptureConsoleAttribute : BeforeAfterTestAttribute
{
	static int initializedCounter;
	static readonly ManualResetEventSlim initializedEvent = new(initialState: false);

	/// <summary>
	/// Gets or sets a flag to indicate whether to override <see cref="Console.Error"/>.
	/// </summary>
	public bool CaptureError { get; set; } = true;

	/// <summary>
	/// Gets or sets a flag to indicate whether to override <see cref="Console.Out"/>
	/// (which includes the <c>Write</c> and <c>WriteLine</c> methods on <see cref="Console"/>).
	/// </summary>
	public bool CaptureOut { get; set; } = true;

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
			new ConsoleCaptureTestOutputWriter(TestContextAccessor.Instance, CaptureError, CaptureOut);

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
