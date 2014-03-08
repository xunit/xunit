using System;
using System.Linq;
using Xunit;
using Xunit.Sdk;

public class DefaultTestCaseOrdererTests
{
    static readonly XunitTestCase[] TestCases = new[] {
        Mocks.XunitTestCase<ClassUnderTest>("Test1"),
        Mocks.XunitTestCase<ClassUnderTest>("Test2"),
        Mocks.XunitTestCase<ClassUnderTest>("Test3"),
        Mocks.XunitTestCase<ClassUnderTest>("Test4"),
        Mocks.XunitTestCase<ClassUnderTest>("Test5"),
        Mocks.XunitTestCase<ClassUnderTest>("Test6")
    };

    [Fact]
    public void OrderIsStable()
    {
        var orderer = new DefaultTestCaseOrderer();

        var result1 = orderer.OrderTestCases(TestCases);
        var result2 = orderer.OrderTestCases(TestCases);
        var result3 = orderer.OrderTestCases(TestCases);

        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }

    [Fact]
    public void OrderIsUnpredictable()
    {
        var orderer = new DefaultTestCaseOrderer();

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
