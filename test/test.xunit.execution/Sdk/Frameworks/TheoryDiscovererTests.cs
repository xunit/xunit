using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TheoryDiscovererTests : AcceptanceTest
{
    readonly ITestFrameworkDiscoveryOptions discoveryOptions = TestFrameworkOptions.ForDiscovery();

    [Fact]
    public void NoDataAttributes()
    {
        var failures = Run<ITestFailed>(typeof(NoDataAttributesClass));

        var failure = Assert.Single(failures);
        Assert.Equal("System.InvalidOperationException", failure.ExceptionTypes.Single());
        Assert.Equal("No data found for TheoryDiscovererTests+NoDataAttributesClass.TheoryMethod", failure.Messages.Single());
    }

    class NoDataAttributesClass
    {
        [Theory]
        public void TheoryMethod(int x) { }
    }

    [Fact]
    public void EmptyTheoryData()
    {
        var failures = Run<ITestFailed>(typeof(EmptyTheoryDataClass));

        var failure = Assert.Single(failures);
        Assert.Equal("System.InvalidOperationException", failure.ExceptionTypes.Single());
        Assert.Equal("No data found for TheoryDiscovererTests+EmptyTheoryDataClass.TheoryMethod", failure.Messages.Single());
    }

    class EmptyTheoryDataAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            return new object[0][];
        }
    }

    class EmptyTheoryDataClass
    {
        [Theory, EmptyTheoryData]
        public void TheoryMethod(int x) { }
    }

    [Fact]
    public void DiscoveryOptions_PreEnumerateTheoriesSetToTrue_YieldsTestCasePerDataRow()
    {
        discoveryOptions.SetPreEnumerateTheories(true);
        var discoverer = TestableTheoryDiscoverer.Create();
        var testMethod = Mocks.TestMethod(typeof(MultipleDataClass), "TheoryMethod");
        var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

        var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute).ToList();

        Assert.Equal(2, testCases.Count);
        Assert.Single(testCases, testCase => testCase.DisplayName == "TheoryDiscovererTests+MultipleDataClass.TheoryMethod(x: 42)");
        Assert.Single(testCases, testCase => testCase.DisplayName == "TheoryDiscovererTests+MultipleDataClass.TheoryMethod(x: 2112)");
    }

    [Fact]
    public void DiscoveryOptions_PreEnumerateTheoriesSetToFalse_YieldsSingleTheoryTestCase()
    {
        discoveryOptions.SetPreEnumerateTheories(false);
        var discoverer = TestableTheoryDiscoverer.Create();
        var testMethod = Mocks.TestMethod(typeof(MultipleDataClass), "TheoryMethod");
        var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

        var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute);

        var testCase = Assert.Single(testCases);
        var theoryTestCase = Assert.IsType<XunitTheoryTestCase>(testCase);
        Assert.Equal("TheoryDiscovererTests+MultipleDataClass.TheoryMethod", theoryTestCase.DisplayName);
    }

    class MultipleDataAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            yield return new object[] { 42 };
            yield return new object[] { 2112 };
        }
    }

    class MultipleDataClass
    {
        [Theory, MultipleDataAttribute]
        public void TheoryMethod(int x) { }
    }

    [Fact]
    public void ThrowingData()
    {
        var discoverer = TestableTheoryDiscoverer.Create();
        var testMethod = Mocks.TestMethod(typeof(ThrowingDataClass), "TheoryWithMisbehavingData");
        var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

        var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute);

        var testCase = Assert.Single(testCases);
        var theoryTestCase = Assert.IsType<XunitTheoryTestCase>(testCase);
        Assert.Equal("TheoryDiscovererTests+ThrowingDataClass.TheoryWithMisbehavingData", theoryTestCase.DisplayName);
    }

    public class ThrowingDataAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo method)
        {
            throw new DivideByZeroException();
        }
    }

    class ThrowingDataClass
    {
        [Theory, ThrowingData]
        public void TheoryWithMisbehavingData(string a) { }
    }

    [Fact]
    public void DataDiscovererReturningNullYieldsSingleTheoryTestCase()
    {
        var discoverer = TestableTheoryDiscoverer.Create();
        var theoryAttribute = Mocks.TheoryAttribute();
        var dataAttribute = Mocks.DataAttribute();
        var testMethod = Mocks.TestMethod(methodAttributes: new[] { theoryAttribute, dataAttribute });

        var testCases = discoverer.Discover(discoveryOptions, testMethod, theoryAttribute);

        var testCase = Assert.Single(testCases);
        var theoryTestCase = Assert.IsType<XunitTheoryTestCase>(testCase);
        Assert.Equal("MockType.MockMethod", theoryTestCase.DisplayName);
    }

    [Fact]
    public void NonSerializableDataYieldsSingleTheoryTestCase()
    {
        var discoverer = TestableTheoryDiscoverer.Create();
        var testMethod = Mocks.TestMethod(typeof(NonSerializableDataClass), "TheoryMethod");
        var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

        var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute);

        var testCase = Assert.Single(testCases);
        var theoryTestCase = Assert.IsType<XunitTheoryTestCase>(testCase);
        Assert.Equal("TheoryDiscovererTests+NonSerializableDataClass.TheoryMethod", theoryTestCase.DisplayName);
    }

    public class NonSerializableDataAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo method)
        {
            yield return new object[] { 42 };
            yield return new object[] { new NonSerializableDataAttribute() };
        }
    }

    class NonSerializableDataClass
    {
        [Theory, NonSerializableData]
        public void TheoryMethod(object a) { }
    }

    [Fact]
    public void NonDiscoveryEnumeratedDataYieldsSingleTheoryTestCase()
    {
        var discoverer = TestableTheoryDiscoverer.Create();
        var testMethod = Mocks.TestMethod(typeof(NonDiscoveryEnumeratedData), "TheoryMethod");
        var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

        var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute);

        var testCase = Assert.Single(testCases);
        var theoryTestCase = Assert.IsType<XunitTheoryTestCase>(testCase);
        Assert.Equal("TheoryDiscovererTests+NonDiscoveryEnumeratedData.TheoryMethod", theoryTestCase.DisplayName);
    }

    class NonDiscoveryEnumeratedData
    {
        public static IEnumerable<object[]> foo { get { return Enumerable.Empty<object[]>(); } }
        public static IEnumerable<object[]> bar { get { return Enumerable.Empty<object[]>(); } }

        [Theory]
        [MemberData("foo", DisableDiscoveryEnumeration = true)]
        [MemberData("bar", DisableDiscoveryEnumeration = true)]
        public static void TheoryMethod(int x) { }
    }

    [Fact]
    public void MixedDiscoveryEnumerationDataYieldSingleTheoryTestCase()
    {
        var discoverer = TestableTheoryDiscoverer.Create();
        var testMethod = Mocks.TestMethod(typeof(MixedDiscoveryEnumeratedData), "TheoryMethod");
        var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

        var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute);

        var testCase = Assert.Single(testCases);
        var theoryTestCase = Assert.IsType<XunitTheoryTestCase>(testCase);
        Assert.Equal("TheoryDiscovererTests+MixedDiscoveryEnumeratedData.TheoryMethod", theoryTestCase.DisplayName);
    }

    class MixedDiscoveryEnumeratedData
    {
        public static IEnumerable<object[]> foo { get { return Enumerable.Empty<object[]>(); } }
        public static IEnumerable<object[]> bar { get { return Enumerable.Empty<object[]>(); } }

        [Theory]
        [MemberData("foo", DisableDiscoveryEnumeration = false)]
        [MemberData("bar", DisableDiscoveryEnumeration = true)]
        public static void TheoryMethod(int x) { }
    }

    [Fact]
    public void SkippedTheoryWithNoData()
    {
        var skips = Run<ITestSkipped>(typeof(SkippedWithNoData));

        var skip = Assert.Single(skips);
        Assert.Equal("TheoryDiscovererTests+SkippedWithNoData.TestMethod", skip.Test.DisplayName);
        Assert.Equal("I have no data", skip.Reason);
    }

    class SkippedWithNoData
    {
        [Theory(Skip = "I have no data")]
        public void TestMethod(int value) { }
    }

    [Fact]
    public void SkippedTheoryWithData()
    {
        var skips = Run<ITestSkipped>(typeof(SkippedWithData));

        var skip = Assert.Single(skips);
        Assert.Equal("TheoryDiscovererTests+SkippedWithData.TestMethod", skip.Test.DisplayName);
        Assert.Equal("I have data", skip.Reason);
    }

    class SkippedWithData
    {
        [Theory(Skip = "I have data")]
        [InlineData(42)]
        [InlineData(2112)]
        public void TestMethod(int value) { }
    }

    class TestableTheoryDiscoverer : TheoryDiscoverer
    {
        public TestableTheoryDiscoverer(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink) { }

        public static TestableTheoryDiscoverer Create()
        {
            return new TestableTheoryDiscoverer(SpyMessageSink.Create());
        }
    }
}