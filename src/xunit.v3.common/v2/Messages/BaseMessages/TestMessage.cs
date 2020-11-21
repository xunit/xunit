using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestMessage"/>.
	/// </summary>
	public class TestMessage : TestCaseMessage, ITestMessage
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestMessage"/> class.
		/// </summary>
		public TestMessage(ITest test)
			: base(test.TestCase)
		{
			Test = test;
		}

		/// <inheritdoc/>
		public ITest Test { get; }
	}
}
