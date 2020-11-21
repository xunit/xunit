using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestClassDisposeFinished"/>.
	/// </summary>
	public class TestClassDisposeFinished : TestMessage, ITestClassDisposeFinished
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestClassDisposeFinished"/> class.
		/// </summary>
		public TestClassDisposeFinished(ITest test)
			: base(test)
		{ }
	}
}
