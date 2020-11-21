using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestClassDisposeStarting"/>.
	/// </summary>
	public class TestClassDisposeStarting : TestMessage, ITestClassDisposeStarting
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestClassDisposeStarting"/> class.
		/// </summary>
		public TestClassDisposeStarting(ITest test)
			: base(test)
		{ }
	}
}
