using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestFrameworkProxyTests
{
    readonly List<IMessageSinkMessage> diagnosticMessages = new List<IMessageSinkMessage>();
    readonly IMessageSink diagnosticSpy;

    public TestFrameworkProxyTests()
    {
        diagnosticSpy = SpyMessageSink.Create(messages: diagnosticMessages);
    }

    [Fact]
    public void NoAttribute()
    {
        var assembly = Mocks.AssemblyInfo();

        var proxy = new TestFrameworkProxy(assembly, null, diagnosticSpy);

        Assert.IsType<XunitTestFramework>(proxy.InnerTestFramework);
        Assert.Empty(diagnosticMessages);
    }

    [Fact]
    public void Attribute_NoDiscoverer()
    {
        var attribute = Mocks.TestFrameworkAttribute(typeof(AttributeWithoutDiscoverer));
        var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });

        var proxy = new TestFrameworkProxy(assembly, null, diagnosticSpy);

        Assert.IsType<XunitTestFramework>(proxy.InnerTestFramework);
        AssertSingleDiagnosticMessage("Assembly-level test framework attribute was not decorated with [TestFrameworkDiscoverer]");
    }

    class AttributeWithoutDiscoverer : Attribute, ITestFrameworkAttribute { }

    [Fact]
    public void Attribute_ThrowingDiscovererCtor()
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

        var attribute = Mocks.TestFrameworkAttribute(typeof(AttributeWithThrowingDiscovererCtor));
        var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });

        var proxy = new TestFrameworkProxy(assembly, null, diagnosticSpy);

        Assert.IsType<XunitTestFramework>(proxy.InnerTestFramework);
        AssertSingleDiagnosticMessage("Exception thrown during test framework discoverer construction: System.DivideByZeroException: Attempted to divide by zero.");
    }

    [TestFrameworkDiscoverer("TestFrameworkProxyTests+ThrowingDiscovererCtor", "test.xunit.execution")]
    class AttributeWithThrowingDiscovererCtor : Attribute, ITestFrameworkAttribute { }

    public class ThrowingDiscovererCtor : ITestFrameworkTypeDiscoverer
    {
        public ThrowingDiscovererCtor()
        {
            throw new DivideByZeroException();
        }

        public Type GetTestFrameworkType(IAttributeInfo attribute)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void Attribute_ThrowingDiscovererMethod()
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

        var attribute = Mocks.TestFrameworkAttribute(typeof(AttributeWithThrowingDiscovererMethod));
        var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });

        var proxy = new TestFrameworkProxy(assembly, null, diagnosticSpy);

        Assert.IsType<XunitTestFramework>(proxy.InnerTestFramework);
        AssertSingleDiagnosticMessage("Exception thrown during test framework discoverer construction: System.DivideByZeroException: Attempted to divide by zero.");
    }

    [TestFrameworkDiscoverer("TestFrameworkProxyTests+ThrowingDiscoverer", "test.xunit.execution")]
    class AttributeWithThrowingDiscovererMethod : Attribute, ITestFrameworkAttribute { }

    public class ThrowingDiscoverer : ITestFrameworkTypeDiscoverer
    {
        public Type GetTestFrameworkType(IAttributeInfo attribute)
        {
            throw new DivideByZeroException();
        }
    }

    [Fact]
    public void Attribute_ThrowingTestFrameworkCtor()
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

        var attribute = Mocks.TestFrameworkAttribute(typeof(AttributeWithThrowingTestFrameworkCtor));
        var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });

        var proxy = new TestFrameworkProxy(assembly, null, diagnosticSpy);

        Assert.IsType<XunitTestFramework>(proxy.InnerTestFramework);
        AssertSingleDiagnosticMessage("Exception thrown during test framework construction: System.DivideByZeroException: Attempted to divide by zero.");
    }

    [TestFrameworkDiscoverer("TestFrameworkProxyTests+DiscovererForThrowingTestFrameworkCtor", "test.xunit.execution")]
    class AttributeWithThrowingTestFrameworkCtor : Attribute, ITestFrameworkAttribute { }

    public class DiscovererForThrowingTestFrameworkCtor : ITestFrameworkTypeDiscoverer
    {
        public Type GetTestFrameworkType(IAttributeInfo attribute)
        {
            return typeof(ThrowingTestFrameworkCtor);
        }
    }

    public class ThrowingTestFrameworkCtor : ITestFramework
    {
        public ThrowingTestFrameworkCtor()
        {
            throw new DivideByZeroException();
        }

        public ISourceInformationProvider SourceInformationProvider { get; set; }

        public ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assembly)
        {
            throw new NotImplementedException();
        }

        public ITestFrameworkExecutor GetExecutor(AssemblyName assemblyName)
        {
            throw new NotImplementedException();
        }

        public void Dispose() { }
    }

    [Fact]
    public void Attribute_WithDiscoverer_NoMessageSink()
    {
        var attribute = Mocks.TestFrameworkAttribute(typeof(AttributeWithDiscoverer));
        var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });
        var sourceProvider = Substitute.For<ISourceInformationProvider>();

        var proxy = new TestFrameworkProxy(assembly, sourceProvider, diagnosticSpy);

        var testFramework = Assert.IsType<MyTestFramework>(proxy.InnerTestFramework);
        Assert.Same(sourceProvider, testFramework.SourceInformationProvider);
        Assert.Empty(diagnosticMessages);
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

        public ITestFrameworkExecutor GetExecutor(AssemblyName assemblyName)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void Attribute_WithDiscoverer_WithMessageSink()
    {
        var attribute = Mocks.TestFrameworkAttribute(typeof(AttributeWithDiscovererWithMessageSink));
        var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });
        var sourceProvider = Substitute.For<ISourceInformationProvider>();

        var proxy = new TestFrameworkProxy(assembly, sourceProvider, diagnosticSpy);

        var testFramework = Assert.IsType<MyTestFrameworkWithMessageSink>(proxy.InnerTestFramework);
        var wrapper = Assert.IsType<TestFrameworkProxy.MessageSinkWrapper>(testFramework.MessageSink);
        Assert.Same(diagnosticSpy, wrapper.InnerSink);
        Assert.Same(sourceProvider, testFramework.SourceInformationProvider);
        Assert.Empty(diagnosticMessages);
    }

    [TestFrameworkDiscoverer("TestFrameworkProxyTests+MyDiscovererWithMessageSink", "test.xunit.execution")]
    public class AttributeWithDiscovererWithMessageSink : Attribute, ITestFrameworkAttribute { }

    public class MyDiscovererWithMessageSink : ITestFrameworkTypeDiscoverer
    {
        public Type GetTestFrameworkType(IAttributeInfo attribute)
        {
            return typeof(MyTestFrameworkWithMessageSink);
        }
    }

    public class MyTestFrameworkWithMessageSink : ITestFramework
    {
        public readonly IMessageSink MessageSink;

        public MyTestFrameworkWithMessageSink(IMessageSink messageSink)
        {
            MessageSink = messageSink;
        }

        public ISourceInformationProvider SourceInformationProvider { get; set; }

        public ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assembly)
        {
            throw new NotImplementedException();
        }

        public ITestFrameworkExecutor GetExecutor(AssemblyName assemblyName)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    private void AssertSingleDiagnosticMessage(string expectedMessage)
    {
        var message = Assert.Single(diagnosticMessages);
        var diagnosticMessage = Assert.IsAssignableFrom<IDiagnosticMessage>(message);
        Assert.StartsWith(expectedMessage, diagnosticMessage.Message);
    }
}
