using NSubstitute;
using Xunit;
using Xunit.Runner.Common;
using Xunit.v3;

public class TestDiscoverySinkTests
{
	[Fact]
	public void CollectsTestCases()
	{
		var visitor = new TestDiscoverySink();
		var testCase1 = new _TestCaseDiscovered();
		var testCase2 = new _TestCaseDiscovered();
		var testCase3 = new _TestCaseDiscovered();

		visitor.OnMessage(testCase1);
		visitor.OnMessage(testCase2);
		visitor.OnMessage(testCase3);
		visitor.OnMessage(new _MessageSinkMessage()); // Ignored

		Assert.Collection(
			visitor.TestCases,
			msg => Assert.Same(testCase1, msg),
			msg => Assert.Same(testCase2, msg),
			msg => Assert.Same(testCase3, msg)
		);
	}
}
