using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class DefaultTestCaseOrdererTests
{
    static readonly ITestCase[] TestCases = new[] {
        Mocks.TestCase<ClassUnderTest>("Test1"),
        Mocks.TestCase<ClassUnderTest>("Test2"),
        Mocks.TestCase<ClassUnderTest>("Test3"),
        Mocks.TestCase<ClassUnderTest>("Test4"),
        Mocks.TestCase<ClassUnderTest>("Test3"),
        Mocks.TestCase<ClassUnderTest>("Test5"),
        Mocks.TestCase<ClassUnderTest>("Test6")
    };

    [Fact]
    public static void OrderIsStable()
    {
        var orderer = new DefaultTestCaseOrderer(SpyMessageSink.Create());

        var result1 = orderer.OrderTestCases(TestCases);
        var result2 = orderer.OrderTestCases(TestCases);
        var result3 = orderer.OrderTestCases(TestCases);

        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }

    [Fact]
    public static void OrderIsUnpredictable()
    {
        var orderer = new DefaultTestCaseOrderer(SpyMessageSink.Create());

        var result = orderer.OrderTestCases(TestCases);

        Assert.NotEqual(TestCases, result);
    }

    class ClassUnderTest
    {
        [Fact]
        public void Test1() { }

        [Fact]
        public void Test2() { }

        [Fact]
        public void Test3() { }

        [Fact]
        public void Test4() { }

        [Fact]
        public void Test5() { }

        [Fact]
        public void Test6() { }
    }
}
