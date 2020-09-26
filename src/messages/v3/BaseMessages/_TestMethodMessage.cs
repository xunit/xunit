// TODO: Can we/should we figure out a unique ID for test methods? Is the name enough?

#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary>
	/// Base message for all messages related to test methods.
	/// </summary>
	public class _TestMethodMessage : _TestClassMessage
	{
		/// <summary>
		/// The name of the method that is associated with this message. If there is no test
		/// method, then this returns <c>null</c>.
		/// </summary>
		public string? TestMethod { get; set; }
	}
}
