﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        var discoverer = new TheoryDiscoverer();
        var testMethod = Mocks.TestMethod(typeof(ThrowingDataClass), "TheoryWithMisbehavingData");
        var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

        var testCases = discoverer.Discover(testMethod, factAttribute);

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
        var discoverer = new TheoryDiscoverer();
        var theoryAttribute = Mocks.TheoryAttribute();
        var dataAttribute = Mocks.DataAttribute();
        var testMethod = Mocks.TestMethod(methodAttributes: new[] { theoryAttribute, dataAttribute });

        var testCases = discoverer.Discover(testMethod, theoryAttribute);

        var testCase = Assert.Single(testCases);
        var theoryTestCase = Assert.IsType<XunitTheoryTestCase>(testCase);
        Assert.Equal("MockType.MockMethod", theoryTestCase.DisplayName);
    }

    [Fact]
    public void NonSerializableDataYieldsSingleTheoryTestCase()
    {
        var discoverer = new TheoryDiscoverer();
        var testMethod = Mocks.TestMethod(typeof(NonSerializableDataClass), "TheoryMethod");
        var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();

        var testCases = discoverer.Discover(testMethod, factAttribute);

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