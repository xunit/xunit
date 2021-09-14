using System.Collections.Generic;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public class TestDiscoveryVisitorTests
{
    [Fact]
    public void CollectsTestCases()
    {
        var visitor = new TestDiscoverySink();
        var testCase1 = Substitute.For<ITestCase>();
        var testCase2 = Substitute.For<ITestCase>();
        var testCase3 = Substitute.For<ITestCase>();

        visitor.OnMessageWithTypes(new DiscoveryMessage(testCase1), null);
        visitor.OnMessageWithTypes(new DiscoveryMessage(testCase2), null);
        visitor.OnMessageWithTypes(new DiscoveryMessage(testCase3), null);
        visitor.OnMessageWithTypes(Substitute.For<IMessageSinkMessage>(), null); // Ignored

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

        public ITestAssembly TestAssembly { get { return TestCase.TestMethod.TestClass.TestCollection.TestAssembly; } }
        public ITestCase TestCase { get; private set; }
        public IEnumerable<ITestCase> TestCases { get { return new[] { TestCase }; } }
        public ITestClass TestClass { get { return TestCase.TestMethod.TestClass; } }
        public ITestCollection TestCollection { get { return TestCase.TestMethod.TestClass.TestCollection; } }
        public ITestMethod TestMethod { get { return TestCase.TestMethod; } }
    }
}
