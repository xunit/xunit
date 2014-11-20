using System;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestFrameworkProxyTests
{
    [Fact]
    public static void NoAttribute()
    {
        var assembly = Mocks.AssemblyInfo();

        var proxy = new TestFrameworkProxy(assembly, null);

        Assert.IsType<XunitTestFramework>(proxy.InnerTestFramework);
    }

    [Fact]
    public static void Attribute_NoDiscoverer()
    {
        var attribute = Mocks.TestFrameworkAttribute(typeof(AttributeWithoutDiscoverer));
        var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });

        var proxy = new TestFrameworkProxy(assembly, null);

        Assert.IsType<XunitTestFramework>(proxy.InnerTestFramework);
    }

    class AttributeWithoutDiscoverer : Attribute, ITestFrameworkAttribute { }

    [Fact]
    public static void Attribute_WithDiscoverer()
    {
        var attribute = Mocks.TestFrameworkAttribute(typeof(AttributeWithDiscoverer));
        var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });
        var sourceProvider = Substitute.For<ISourceInformationProvider>();

        var proxy = new TestFrameworkProxy(assembly, sourceProvider);

        var testFramework = Assert.IsType<MyTestFramework>(proxy.InnerTestFramework);
        Assert.Same(sourceProvider, testFramework.SourceInformationProvider);
    }

    [TestFrameworkDiscoverer("TestFrameworkProxyTests+MyDiscoverer", "test.xunit.execution")]
    public class AttributeWithDiscoverer : Attribute, ITestFrameworkAttribute { }

    public class MyDiscoverer : ITestFrameworkTypeDiscoverer
    {
        public Type GetTestFrameworkType(IAttributeInfo attribute)
        {
            return typeof(MyTestFramework);
        }
    }

    public class MyTestFramework : ITestFramework
    {
        public ISourceInformationProvider SourceInformationProvider { get; set; }

        public ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assembly)
        {
            throw new NotImplementedException();
        }

        public ITestFrameworkExecutor GetExecutor(System.Reflection.AssemblyName assemblyName)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
