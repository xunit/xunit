using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TheoryDiscovererTests : AcceptanceTest
{
    [Fact]
    public void NoDataAttributes()
    {
        var failures = Run<ITestFailed>(typeof(NoDataAttributesClass));

        var failure = Assert.Single(failures);
        Assert.Equal(typeof(InvalidOperationException).FullName, failure.ExceptionType);
        Assert.Equal("System.InvalidOperationException : No data found for TheoryDiscovererTests+NoDataAttributesClass.TheoryMethod", failure.Message);
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
        Assert.Equal(typeof(InvalidOperationException).FullName, failure.ExceptionType);
        Assert.Equal("System.InvalidOperationException : No data found for TheoryDiscovererTests+EmptyTheoryDataClass.TheoryMethod", failure.Message);
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
    public void MultipleDataRowsFromSingleDataAttribute()
    {
        var passes = Run<ITestPassed>(typeof(MultipleDataClass)).Select(tc => tc.TestDisplayName).ToList();

        Assert.Equal(2, passes.Count);
        Assert.Single(passes, name => name == "TheoryDiscovererTests+MultipleDataClass.TheoryMethod(x: 42)");
        Assert.Single(passes, name => name == "TheoryDiscovererTests+MultipleDataClass.TheoryMethod(x: 2112)");
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
        var testCollection = new XunitTestCollection();
        var discoverer = new TheoryDiscoverer();
        var type = typeof(ThrowingDataClass);
        var method = type.GetMethod("TheoryWithMisbehavingData");
        var theory = CustomAttributeData.GetCustomAttributes(method).Single(cad => cad.AttributeType == typeof(TheoryAttribute));

        var testCases = discoverer.Discover(testCollection, Reflector.Wrap(type.Assembly), Reflector.Wrap(type), Reflector.Wrap(method), Reflector.Wrap(theory));

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
        var testCollection = new XunitTestCollection();
        var discoverer = new TheoryDiscoverer();
        var assembly = Mocks.AssemblyInfo();
        var type = Mocks.TypeInfo();
        var theory = Mocks.TheoryAttribute();
        var data = Substitute.For<IAttributeInfo>();
        var method = Mocks.MethodInfo();
        method.GetCustomAttributes(typeof(DataAttribute).AssemblyQualifiedName).Returns(new[] { data });
        method.GetCustomAttributes(typeof(TheoryAttribute).AssemblyQualifiedName).Returns(new[] { theory });

        var testCases = discoverer.Discover(testCollection, assembly, type, method, theory);

        var testCase = Assert.Single(testCases);
        var theoryTestCase = Assert.IsType<XunitTheoryTestCase>(testCase);
        Assert.Equal("MockType.MockMethod", theoryTestCase.DisplayName);
    }

    [Fact]
    public void NonSerializableDataYieldsSingleTheoryTestCase()
    {
        var testCollection = new XunitTestCollection();
        var discoverer = new TheoryDiscoverer();
        var type = typeof(NonSerializableDataClass);
        var method = type.GetMethod("TheoryMethod");
        var theory = CustomAttributeData.GetCustomAttributes(method).Single(cad => cad.AttributeType == typeof(TheoryAttribute));

        var testCases = discoverer.Discover(testCollection, Reflector.Wrap(type.Assembly), Reflector.Wrap(type), Reflector.Wrap(method), Reflector.Wrap(theory));

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
    public void SkippedTheoryWithNoData()
    {
        var skips = Run<ITestSkipped>(typeof(SkippedWithNoData));

        var skip = Assert.Single(skips);
        Assert.Equal("TheoryDiscovererTests+SkippedWithNoData.TestMethod", skip.TestDisplayName);
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
        Assert.Equal("TheoryDiscovererTests+SkippedWithData.TestMethod", skip.TestDisplayName);
        Assert.Equal("I have data", skip.Reason);
    }

    class SkippedWithData
    {
        [Theory(Skip = "I have data")]
        [InlineData(42)]
        [InlineData(2112)]
        public void TestMethod(int value) { }
    }
}