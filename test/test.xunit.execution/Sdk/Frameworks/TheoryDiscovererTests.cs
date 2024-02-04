using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using SerializationHelper = Xunit.Sdk.SerializationHelper;

public class TheoryDiscovererTests
{
    readonly ITestFrameworkDiscoveryOptions discoveryOptions = TestFrameworkOptions.ForDiscovery();

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
        [Theory, MultipleData]
        public void TheoryMethod(int x) { }
    }

    [Fact]
    public void DiscoverOptions_PreEnumerateTheoriesSetToTrueWithSkipOnData_YieldsSkippedTestCasePerDataRow()
    {
        discoveryOptions.SetPreEnumerateTheories(true);
        var discoverer = TestableTheoryDiscoverer.Create();
        var testMethod = Mocks.TestMethod(typeof(MultipleDataClassSkipped), "TheoryMethod");
        var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

        var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute).ToList();

        Assert.Equal(2, testCases.Count);
        Assert.Single(testCases, testCase => testCase.DisplayName == "TheoryDiscovererTests+MultipleDataClassSkipped.TheoryMethod(x: 42)" && testCase.SkipReason == "Skip this attribute");
        Assert.Single(testCases, testCase => testCase.DisplayName == "TheoryDiscovererTests+MultipleDataClassSkipped.TheoryMethod(x: 2112)" && testCase.SkipReason == "Skip this attribute");

    }

    class MultipleDataClassSkipped
    {
        [Theory, MultipleData(Skip = "Skip this attribute")]
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
        var message = Assert.Single(discoverer.DiagnosticMessages);
        var diagnostic = Assert.IsAssignableFrom<IDiagnosticMessage>(message);
        Assert.StartsWith($"Exception thrown during theory discovery on 'TheoryDiscovererTests+ThrowingDataClass.TheoryWithMisbehavingData'; falling back to single test case.{Environment.NewLine}System.DivideByZeroException: Attempted to divide by zero.", diagnostic.Message);
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
        var testMethod = Mocks.TestMethod("MockTheoryType", "MockTheoryMethod", methodAttributes: new[] { theoryAttribute, dataAttribute });

        var testCases = discoverer.Discover(discoveryOptions, testMethod, theoryAttribute);

        var testCase = Assert.Single(testCases);
        var theoryTestCase = Assert.IsType<XunitTheoryTestCase>(testCase);
        Assert.Equal("MockTheoryType.MockTheoryMethod", theoryTestCase.DisplayName);
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
        var message = Assert.Single(discoverer.DiagnosticMessages);
        var diagnostic = Assert.IsAssignableFrom<IDiagnosticMessage>(message);
        Assert.Equal("Non-serializable data ('System.Object[]') found for 'TheoryDiscovererTests+NonSerializableDataClass.TheoryMethod'; falling back to single test case.", diagnostic.Message);
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

#if NETFRAMEWORK
    [Fact]
    public void TheoryWithNonSerializableEnumYieldsSingleTheoryTestCase()
    {
        var discoverer = TestableTheoryDiscoverer.Create();
        var testMethod = Mocks.TestMethod(typeof(NonSerializableEnumDataClass), "TheTest");
        var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

        var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute);

        var testCase = Assert.Single(testCases);
        var theoryTestCase = Assert.IsType<XunitTheoryTestCase>(testCase);
        Assert.Equal("TheoryDiscovererTests+NonSerializableEnumDataClass.TheTest", theoryTestCase.DisplayName);
    }

    class NonSerializableEnumDataClass
    {
        [Theory]
        [InlineData(42)]
        [InlineData(ConformanceLevel.Auto)]
        public void TheTest(object x) { }
    }
#endif

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
    public void InlineDataWithNoValuesAndParamsArray()
    {
        void assertTestCaseDetails(IXunitTestCase testCase, params int[] items)
        {
            var paramsDisplay = ArgumentFormatter.Format(items);

            Assert.Equal($"TheoryDiscovererTests+ParamsArrayWithNoData.TestMethod(values: {paramsDisplay})", testCase.DisplayName);
            Assert.NotNull(testCase.TestMethodArguments);
            var arg = Assert.Single(testCase.TestMethodArguments);
            var array = Assert.IsType<int[]>(arg);
            Assert.Equal(items, array);
        }

        void assertValidTestCase(IXunitTestCase testCase, params int[] items)
        {
            assertTestCaseDetails(testCase, items);
            var serialized = SerializationHelper.Serialize(testCase);
            var deserialized = SerializationHelper.Deserialize<IXunitTestCase>(serialized);
            assertTestCaseDetails(deserialized, items);
        }

        var discoverer = TestableTheoryDiscoverer.Create();
        var testMethod = Mocks.TestMethod(typeof(ParamsArrayWithNoData), "TestMethod");
        var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

        var testCases = discoverer.Discover(discoveryOptions, testMethod, factAttribute);

        Assert.Collection(
            testCases.OrderBy(tc => tc.DisplayName),
            testCase => assertValidTestCase(testCase),
            testCase => assertValidTestCase(testCase, 1, 2),
            testCase => assertValidTestCase(testCase, 1)
        );
    }

    class ParamsArrayWithNoData
    {
        [Theory]
        [InlineData]
        [InlineData(1)]
        [InlineData(1, 2)]
        public void TestMethod(params int[] values) { }
    }

    class TestableTheoryDiscoverer : TheoryDiscoverer
    {
        public List<IMessageSinkMessage> DiagnosticMessages;

        public TestableTheoryDiscoverer(List<IMessageSinkMessage> diagnosticMessages)
            : base(SpyMessageSink.Create(messages: diagnosticMessages))
        {
            DiagnosticMessages = diagnosticMessages;
        }

        public static TestableTheoryDiscoverer Create()
        {
            var messages = new List<IMessageSinkMessage>();
            return new TestableTheoryDiscoverer(new List<IMessageSinkMessage>());
        }
    }
}
