using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestOutput"/>.
	/// </summary>
	public class TestOutput : TestMessage, ITestOutput
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestOutput"/> class.
		/// </summary>
		public TestOutput(
			ITest test,
			string output)
				: base(test)
		{
			Output = output;
		}

		/// <inheritdoc/>
		public string Output { get; }
	}
}
