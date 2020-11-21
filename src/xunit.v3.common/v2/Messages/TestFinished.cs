using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestFinished"/>.
	/// </summary>
	public class TestFinished : TestMessage, ITestFinished
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestFinished"/> class.
		/// </summary>
		public TestFinished(
			ITest test,
			decimal executionTime,
			string? output)
				: base(test)
		{
			ExecutionTime = executionTime;
			Output = output ?? string.Empty;
		}

		/// <inheritdoc/>
		public decimal ExecutionTime { get; }

		/// <inheritdoc/>
		public string Output { get; }
	}
}
