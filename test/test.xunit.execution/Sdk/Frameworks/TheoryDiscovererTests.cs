#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TheoryDiscovererTests : AcceptanceTestV2
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
    public void NullMemberData_ThrowsInvalidOperationException()
    {
        var results = Run<ITestFailed>(typeof(NullDataClass));

        var failure = Assert.Single(results);
        Assert.Equal("System.InvalidOperationException", failure.ExceptionTypes.Single());
        Assert.Equal("Test data returned null for TheoryDiscovererTests+NullDataClass.NullMemberData. Make sure it is statically initialized before this test method is called.", failure.Messages.Single());
    }

    public class NullDataClass
    {
        public static IEnumerable<object[]> InitializedInConstructor;

        public NullDataClass()
        {
            InitializedInConstructor = new List<object[]>
            {
                new object[] { "1", "2"}
            };
        }

        [Theory]
        [MemberData(nameof(InitializedInConstructor))]
        public void NullMemberData(string str1, string str2) { }
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
        [Theory, MultipleDataAttribute(Skip = "Skip this attribute")]
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

    public class NonSerializableEnumDataClass
    {
        [Theory]
        [InlineData(42)]
        [InlineData(ConformanceLevel.Auto)]
        public void TheTest(object x) { }
    }

    [Fact]
    public void NoSuchDataDiscoverer_ThrowsInvalidOperationException()
    {
        var results = Run<ITestFailed>(typeof(NoSuchDataDiscovererClass));

        var failure = Assert.Single(results);
        Assert.Equal("System.InvalidOperationException", failure.ExceptionTypes.Single());
        Assert.Equal("Data discoverer specified for TheoryDiscovererTests+NoSuchDataDiscovererAttribute on TheoryDiscovererTests+NoSuchDataDiscovererClass.Test does not exist.", failure.Messages.Single());
    }

    public class NoSuchDataDiscovererClass
    {
        [Theory]
        [NoSuchDataDiscoverer]
        public void Test() { }
    }

    [DataDiscoverer("Foo.Blah.ThingDiscoverer", "invalid_assembly_name")]
    public class NoSuchDataDiscovererAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void NotADataDiscoverer_ThrowsInvalidOperationException()
    {
        var results = Run<ITestFailed>(typeof(NotADataDiscovererClass));

        var failure = Assert.Single(results);
        Assert.Equal("System.InvalidOperationException", failure.ExceptionTypes.Single());
        Assert.Equal("Data discoverer specified for TheoryDiscovererTests+NotADataDiscovererAttribute on TheoryDiscovererTests+NotADataDiscovererClass.Test does not implement IDataDiscoverer.", failure.Messages.Single());
    }

    public class NotADataDiscovererClass
    {
        [Theory]
        [NotADataDiscoverer]
        public void Test() { }
    }

    [DataDiscoverer("TheoryDiscovererTests", "test.xunit.execution")]
    public class NotADataDiscovererAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            throw new NotImplementedException();
        }
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

#endif
