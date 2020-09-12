using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.v2
#else
namespace Xunit.Runner.v2
#endif
{
	/// <summary>
	/// Default implementation of <see cref="ITestClassConstructionStarting"/>.
	/// </summary>
	public class TestClassConstructionStarting : TestMessage, ITestClassConstructionStarting
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestClassConstructionStarting"/> class.
		/// </summary>
		public TestClassConstructionStarting(ITest test)
			: base(test)
		{ }
	}
}
