using System;

#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary />
	public class _TestMessage : _TestCaseMessage
	{
		string? testDisplayName;

		/// <summary />
		public string TestDisplayName
		{
			get => testDisplayName ?? throw new InvalidOperationException($"Attempted to get {nameof(TestDisplayName)} on an uninitialized '{GetType().FullName}' object");
			set => testDisplayName = Guard.ArgumentNotNullOrEmpty(nameof(TestDisplayName), value);
		}
	}
}
