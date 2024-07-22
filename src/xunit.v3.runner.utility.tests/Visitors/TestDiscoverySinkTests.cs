using Xunit;
using Xunit.Runner.Common;

public class TestDiscoverySinkTests
{
	[Fact]
	public void CollectsTestCases()
	{
		var visitor = new TestDiscoverySink();
		var testCase1 = TestData.TestCaseDiscovered();
		var testCase2 = TestData.TestCaseDiscovered();
		var testCase3 = TestData.TestCaseDiscovered();

		visitor.OnMessage(testCase1);
		visitor.OnMessage(testCase2);
		visitor.OnMessage(testCase3);
		visitor.OnMessage(TestData.DiagnosticMessage()); // Ignored

		Assert.Collection(
			visitor.TestCases,
			msg => Assert.Same(testCase1, msg),
			msg => Assert.Same(testCase2, msg),
			msg => Assert.Same(testCase3, msg)
		);
	}
}
