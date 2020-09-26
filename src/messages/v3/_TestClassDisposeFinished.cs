using System;

#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary>
	/// This message indicates that the <see cref="IDisposable.Dispose"/> and/or
	/// <see cref="IAsyncDisposable.DisposeAsync"/> method was just called on the test class
	/// for the test case that just finished executing.
	/// </summary>
	public class _TestClassDisposeFinished : _TestMessage
	{ }
}
