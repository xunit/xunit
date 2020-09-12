#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary />
	public class _TestMethodMessage : _TestClassMessage
	{
		/// <summary />
		public string? TestMethod { get; set; }
	}
}
