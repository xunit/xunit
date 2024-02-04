#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TheoryDiscovererAcceptanceTests : AcceptanceTestV2
{
    [Fact]
    public void NoDataAttributes()
    {
        var failures = Run<ITestFailed>(typeof(NoDataAttributesClass));

        var failure = Assert.Single(failures);
        Assert.Equal("System.InvalidOperationException", failure.ExceptionTypes.Single());
        Assert.Equal("No data found for TheoryDiscovererAcceptanceTests+NoDataAttributesClass.TheoryMethod", failure.Messages.Single());
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
        Assert.Equal("Test data returned null for TheoryDiscovererAcceptanceTests+NullDataClass.NullMemberData. Make sure it is statically initialized before this test method is called.", failure.Messages.Single());
    }

    class NullDataClass
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
        Assert.Equal("No data found for TheoryDiscovererAcceptanceTests+EmptyTheoryDataClass.TheoryMethod", failure.Messages.Single());
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
    public void NoSuchDataDiscoverer_ThrowsInvalidOperationException()
    {
        var results = Run<ITestFailed>(typeof(NoSuchDataDiscovererClass));

        var failure = Assert.Single(results);
        Assert.Equal("System.InvalidOperationException", failure.ExceptionTypes.Single());
        Assert.Equal("Data discoverer specified for TheoryDiscovererAcceptanceTests+NoSuchDataDiscovererAttribute on TheoryDiscovererAcceptanceTests+NoSuchDataDiscovererClass.Test does not exist.", failure.Messages.Single());
    }

    class NoSuchDataDiscovererClass
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
        Assert.Equal("Data discoverer specified for TheoryDiscovererAcceptanceTests+NotADataDiscovererAttribute on TheoryDiscovererAcceptanceTests+NotADataDiscovererClass.Test does not implement IDataDiscoverer.", failure.Messages.Single());
    }

    class NotADataDiscovererClass
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
    public void SkippedTheoryWithNoData()
    {
        var skips = Run<ITestSkipped>(typeof(SkippedWithNoData));

        var skip = Assert.Single(skips);
        Assert.Equal("TheoryDiscovererAcceptanceTests+SkippedWithNoData.TestMethod", skip.Test.DisplayName);
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
        Assert.Equal("TheoryDiscovererAcceptanceTests+SkippedWithData.TestMethod", skip.Test.DisplayName);
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

#endif
