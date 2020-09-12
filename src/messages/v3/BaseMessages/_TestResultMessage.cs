#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary />
	public class _TestResultMessage : _TestMessage
	{
		/// <summary />
		public decimal? ExecutionTime { get; set; }

		/// <summary />
		public string? Output { get; set; }
	}
}
