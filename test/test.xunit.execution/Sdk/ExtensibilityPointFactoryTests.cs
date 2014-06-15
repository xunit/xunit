using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class ExtensibilityPointFactoryTests
{
    public class GetXunitTestCollectionFactory
    {
        [Fact]
        public static void DefaultTestCollectionFactoryIsCollectionPerClass()
        {
            var assembly = Mocks.AssemblyInfo();

            var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory((IAttributeInfo)null, assembly);

            Assert.IsType<CollectionPerClassTestCollectionFactory>(result);
        }

        [Theory]
        [InlineData(CollectionBehavior.CollectionPerAssembly, typeof(CollectionPerAssemblyTestCollectionFactory))]
        [InlineData(CollectionBehavior.CollectionPerClass, typeof(CollectionPerClassTestCollectionFactory))]
        public static void UserCanChooseFromBuiltInCollectionFactories_NonParallel(CollectionBehavior behavior, Type expectedType)
        {
            var attr = Mocks.CollectionBehaviorAttribute(behavior);
            var assembly = Mocks.AssemblyInfo();

            var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(attr, assembly);

            Assert.IsType(expectedType, result);
        }

        [Fact]
        public static void UserCanChooseCustomCollectionFactory()
        {
            var factoryType = typeof(MyTestCollectionFactory);
            var attr = Mocks.CollectionBehaviorAttribute(factoryType.FullName, factoryType.Assembly.FullName);
            var assembly = Mocks.AssemblyInfo();

            var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(attr, assembly);

            var myFactory = Assert.IsType<MyTestCollectionFactory>(result);
            Assert.Same(assembly, myFactory.Assembly);
        }

        class MyTestCollectionFactory : IXunitTestCollectionFactory
        {
            public MyTestCollectionFactory(IAssemblyInfo assembly)
            {
                Assembly = assembly;
            }

            public readonly IAssemblyInfo Assembly;

            public string DisplayName { get { return "My Factory"; } }

            public ITestCollection Get(ITypeInfo testClass)
            {
                throw new NotImplementedException();
            }
        }

        [Theory]
        [InlineData("ExtensibilityPointFactoryTests+GetXunitTestCollectionFactory+TestCollectionFactory_NoCompatibleConstructor")]
        [InlineData("ExtensibilityPointFactoryTests+GetXunitTestCollectionFactory+TestCollectionFactory_DoesNotImplementInterface")]
        [InlineData("ThisIsNotARealType")]
        public static void IncompatibleOrInvalidTypesGetDefaultBehavior(string factoryTypeName)
        {
            var attr = Mocks.CollectionBehaviorAttribute(factoryTypeName, "test.xunit.execution");
            var assembly = Mocks.AssemblyInfo();

            var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(attr, assembly);

            Assert.IsType<CollectionPerClassTestCollectionFactory>(result);
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
