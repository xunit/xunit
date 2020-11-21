using Xunit.Abstractions;

namespace Xunit.Runner.v2
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
