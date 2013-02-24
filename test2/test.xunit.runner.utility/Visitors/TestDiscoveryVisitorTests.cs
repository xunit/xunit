using Moq;
using Xunit;
using Xunit.Abstractions;

public class TestDiscoveryVisitorTests
{
    [Fact]
    public void CollectsTestCases()
    {
        var visitor = new TestDiscoveryVisitor();
        var testCase1 = new Mock<ITestCase>().Object;
        var testCase2 = new Mock<ITestCase>().Object;
        var testCase3 = new Mock<ITestCase>().Object;

        visitor.OnMessage(new DiscoveryMessage(testCase1));
        visitor.OnMessage(new DiscoveryMessage(testCase2));
        visitor.OnMessage(new DiscoveryMessage(testCase3));
        visitor.OnMessage(new Mock<ITestMessage>().Object); // Ignored

        Assert.Collection(visitor.TestCases,
            msg => Assert.Same(testCase1, msg),
            msg => Assert.Same(testCase2, msg),
            msg => Assert.Same(testCase3, msg)
        );
    }

    class DiscoveryMessage : ITestCaseDiscoveryMessage
    {
        public DiscoveryMessage(ITestCase testCase)
        {
            TestCase = testCase;
        }

        public ITestCase TestCase { get; private set; }
    }
}