using System;

#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary>
	/// This message indicates that a test was skipped.
	/// </summary>
	public class _TestSkipped : _TestResultMessage
	{
		string? reason;

		/// <summary>
		/// The reason given for skipping the test.
		/// </summary>
		public string Reason
		{
			get => reason ?? throw new InvalidOperationException($"Attempted to get {nameof(Reason)} on an uninitialized '{GetType().FullName}' object");
			set => reason = Guard.ArgumentNotNull(nameof(Reason), value);
		}
	}
}
