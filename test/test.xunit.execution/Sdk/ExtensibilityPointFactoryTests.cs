using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class ExtensibilityPointFactoryTests
{
    readonly List<IMessageSinkMessage> messages = new List<IMessageSinkMessage>();
    protected IMessageSink spy;

    public ExtensibilityPointFactoryTests()
    {
        spy = SpyMessageSink.Create(messages: messages);
    }

    public IEnumerable<string> DiagnosticMessages
    {
        get
        {
            return messages.OfType<IDiagnosticMessage>().Select(m => m.Message);
        }
    }

    public class GetXunitTestCollectionFactory : ExtensibilityPointFactoryTests
    {
        [Fact]
        public void DefaultTestCollectionFactoryIsCollectionPerClass()
        {
            var assembly = Mocks.TestAssembly();

            var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(spy, (IAttributeInfo)null, assembly);

            Assert.IsType<CollectionPerClassTestCollectionFactory>(result);
        }

        [Theory]
        [InlineData(CollectionBehavior.CollectionPerAssembly, typeof(CollectionPerAssemblyTestCollectionFactory))]
        [InlineData(CollectionBehavior.CollectionPerClass, typeof(CollectionPerClassTestCollectionFactory))]
        public void UserCanChooseFromBuiltInCollectionFactories_NonParallel(CollectionBehavior behavior, Type expectedType)
        {
            var attr = Mocks.CollectionBehaviorAttribute(behavior);
            var assembly = Mocks.TestAssembly();

            var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(spy, attr, assembly);

            Assert.IsType(expectedType, result);
        }

        [Fact]
        public void UserCanChooseCustomCollectionFactory()
        {
            var factoryType = typeof(MyTestCollectionFactory);
            var attr = Mocks.CollectionBehaviorAttribute(factoryType.FullName, factoryType.Assembly.FullName);
            var assembly = Mocks.TestAssembly();

            var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(spy, attr, assembly);

            var myFactory = Assert.IsType<MyTestCollectionFactory>(result);
            Assert.Same(assembly, myFactory.Assembly);
        }

        class MyTestCollectionFactory : IXunitTestCollectionFactory
        {
            public MyTestCollectionFactory(ITestAssembly assembly)
            {
                Assembly = assembly;
            }

            public readonly ITestAssembly Assembly;

            public string DisplayName { get { return "My Factory"; } }

            public ITestCollection Get(ITypeInfo testClass)
            {
                throw new NotImplementedException();
            }
        }

        [Theory]
        [InlineData("ExtensibilityPointFactoryTests+GetXunitTestCollectionFactory+TestCollectionFactory_NoCompatibleConstructor",
                    "Could not find constructor for 'ExtensibilityPointFactoryTests+GetXunitTestCollectionFactory+TestCollectionFactory_NoCompatibleConstructor' with arguments type(s): Xunit.Sdk.TestAssembly")]
        [InlineData("ExtensibilityPointFactoryTests+GetXunitTestCollectionFactory+TestCollectionFactory_DoesNotImplementInterface",
                    "Test collection factory type 'test.xunit.execution, ExtensibilityPointFactoryTests+GetXunitTestCollectionFactory+TestCollectionFactory_DoesNotImplementInterface' does not implement IXunitTestCollectionFactory")]
        [InlineData("ThisIsNotARealType",
                    "Unable to create test collection factory type 'test.xunit.execution, ThisIsNotARealType'")]
        public void IncompatibleOrInvalidTypesGetDefaultBehavior(string factoryTypeName, string expectedMessage)
        {
            var attr = Mocks.CollectionBehaviorAttribute(factoryTypeName, "test.xunit.execution");
            var assembly = Mocks.TestAssembly();

            var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(spy, attr, assembly);

            Assert.IsType<CollectionPerClassTestCollectionFactory>(result);
            Assert.Collection(DiagnosticMessages,
                msg => Assert.Equal(expectedMessage, msg)
            );
        }

        class TestCollectionFactory_NoCompatibleConstructor : IXunitTestCollectionFactory
        {
            public string DisplayName
            {
                get { throw new NotImplementedException(); }
            }

            public ITestCollection Get(ITypeInfo testClass)
            {
                throw new NotImplementedException();
            }
        }

        class TestCollectionFactory_DoesNotImplementInterface
        {
            public TestCollectionFactory_DoesNotImplementInterface(IAssemblyInfo assemblyInfo) { }
        }
    }
}
