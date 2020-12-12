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
		var testCase1 = Substitute.For<_ITestCase>();
		var testCase2 = Substitute.For<_ITestCase>();
		var testCase3 = Substitute.For<_ITestCase>();

		visitor.OnMessage(new _TestCaseDiscovered { TestCase = testCase1 });
		visitor.OnMessage(new _TestCaseDiscovered { TestCase = testCase2 });
		visitor.OnMessage(new _TestCaseDiscovered { TestCase = testCase3 });
		visitor.OnMessage(new _MessageSinkMessage()); // Ignored

		Assert.Collection(
			visitor.TestCases,
			msg => Assert.Same(testCase1, msg),
			msg => Assert.Same(testCase2, msg),
			msg => Assert.Same(testCase3, msg)
		);
	}
}
