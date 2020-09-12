#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary />
	public class _TestClassMessage : _TestCollectionMessage
	{
		/// <summary />
		public string? TestClass { get; set; }
	}
}
