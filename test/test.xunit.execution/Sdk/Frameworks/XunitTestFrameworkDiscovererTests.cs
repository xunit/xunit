using System;
using System.Collections.Generic;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using ITypeInfo = Xunit.Abstractions.ITypeInfo;

public class XunitTestFrameworkDiscovererTests
{
    public class Construction
    {
        [Fact]
        public static void GuardClause()
        {
            var assembly = Substitute.For<IAssemblyInfo>();
            var sourceProvider = Substitute.For<ISourceInformationProvider>();

            Assert.Throws<ArgumentNullException>("assemblyInfo", () => new XunitTestFrameworkDiscoverer(null, sourceProvider));
            Assert.Throws<ArgumentNullException>("sourceProvider", () => new XunitTestFrameworkDiscoverer(assembly, null));
        }

        [Fact]
        public static void DefaultTestCollectionFactoryIsCollectionPerClass()
        {
            var assembly = Mocks.AssemblyInfo();
            var sourceInfoProvider = Substitute.For<ISourceInformationProvider>();

            var discoverer = new XunitTestFrameworkDiscoverer(assembly, sourceInfoProvider);

            Assert.IsType<CollectionPerClassTestCollectionFactory>(discoverer.TestCollectionFactory);
            Assert.Equal(XunitTestFrameworkDiscoverer.DisplayName + " [collection-per-class, parallel]", discoverer.TestFrameworkDisplayName);
        }

        [Theory]
        [InlineData(CollectionBehavior.CollectionPerAssembly, typeof(CollectionPerAssemblyTestCollectionFactory), "collection-per-assembly")]
        [InlineData(CollectionBehavior.CollectionPerClass, typeof(CollectionPerClassTestCollectionFactory), "collection-per-class")]
        public static void UserCanChooseFromBuiltInCollectionFactories_NonParallel(CollectionBehavior behavior, Type expectedType, string expectedDisplayText)
        {
            var attr = Mocks.CollectionBehaviorAttribute(behavior, disableTestParallelization: true);
            var assembly = Mocks.AssemblyInfo(attributes: new[] { attr });
            var sourceInfoProvider = Substitute.For<ISourceInformationProvider>();

            var discoverer = new XunitTestFrameworkDiscoverer(assembly, sourceInfoProvider);

            Assert.IsType(expectedType, discoverer.TestCollectionFactory);
            Assert.Equal(String.Format("{0} [{1}, non-parallel]", XunitTestFrameworkDiscoverer.DisplayName, expectedDisplayText), discoverer.TestFrameworkDisplayName);
        }

        [Fact]
        public static void UserCanChooseCustomCollectionFactory()
        {
            var factoryType = typeof(MyTestCollectionFactory);
            var attr = Mocks.CollectionBehaviorAttribute(factoryType.FullName, factoryType.Assembly.FullName);
            var assembly = Mocks.AssemblyInfo(attributes: new[] { attr });
            var sourceInfoProvider = Substitute.For<ISourceInformationProvider>();

            var discoverer = new XunitTestFrameworkDiscoverer(assembly, sourceInfoProvider);

            Assert.IsType<MyTestCollectionFactory>(discoverer.TestCollectionFactory);
            Assert.Equal(XunitTestFrameworkDiscoverer.DisplayName + " [My Factory, parallel]", discoverer.TestFrameworkDisplayName);
        }

        class MyTestCollectionFactory : IXunitTestCollectionFactory
        {
            public string DisplayName { get { return "My Factory"; } }

            public MyTestCollectionFactory(IAssemblyInfo assembly) { }

            public ITestCollection Get(ITypeInfo testClass)
            {
                throw new NotImplementedException();
            }
        }

        [Theory]
        [InlineData("XunitTestFrameworkDiscovererTests+Construction+TestCollectionFactory_NoCompatibleConstructor")]
        [InlineData("XunitTestFrameworkDiscovererTests+Construction+TestCollectionFactory_DoesNotImplementInterface")]
        [InlineData("ThisIsNotARealType")]
        public static void IncompatibleOrInvalidTypesGetDefaultBehavior(string factoryTypeName)
        {
            var attr = Mocks.CollectionBehaviorAttribute(factoryTypeName, "test.xunit.execution");
            var assembly = Mocks.AssemblyInfo(attributes: new[] { attr });
            var sourceInfoProvider = Substitute.For<ISourceInformationProvider>();

            var discoverer = new XunitTestFrameworkDiscoverer(assembly, sourceInfoProvider);

            Assert.IsType<CollectionPerClassTestCollectionFactory>(discoverer.TestCollectionFactory);
            Assert.Equal(XunitTestFrameworkDiscoverer.DisplayName + " [collection-per-class, parallel]", discoverer.TestFrameworkDisplayName);
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

    public static class FindByAssembly
    {
        [Fact]
        public static void GuardClauses()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();

            Assert.Throws<ArgumentNullException>("messageSink", () => framework.Find(includeSourceInformation: false, messageSink: null, options: new TestFrameworkOptions()));
            Assert.Throws<ArgumentNullException>("options", () => framework.Find(includeSourceInformation: false, messageSink: Substitute.For<IMessageSink>(), options: null));
        }

        [Fact]
        public static void AssemblyWithNoTypes_ReturnsNoTestCases()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();

            framework.Find();

            Assert.Empty(framework.TestCases);
        }

        [Fact]
        public static void RequestsOnlyPublicTypesFromAssembly()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create(collectionFactory: Substitute.For<IXunitTestCollectionFactory>());

            framework.Find();

            framework.Assembly.Received(1).GetTypes(includePrivateTypes: false);
        }

        [Fact]
        public static void ExcludesAbstractTypesFromDiscovery()
        {
            var abstractClassTypeInfo = Reflector.Wrap(typeof(AbstractClass));
            var assembly = Mocks.AssemblyInfo(types: new[] { abstractClassTypeInfo });
            var framework = Substitute.For<TestableXunitTestFrameworkDiscoverer>(assembly);
            framework.FindTestsForClass(null).ReturnsForAnyArgs(true);

            framework.Find();
            framework.Visitor.Finished.WaitOne();

            framework.Received(0).FindTestsForClass(abstractClassTypeInfo, Arg.Any<bool>());
        }

        [Fact]
        public static void CallsFindImplWhenTypesAreFoundInAssembly()
        {
            var objectTypeInfo = Reflector.Wrap(typeof(object));
            var intTypeInfo = Reflector.Wrap(typeof(int));
            var assembly = Mocks.AssemblyInfo(types: new[] { objectTypeInfo, intTypeInfo });
            var framework = Substitute.For<TestableXunitTestFrameworkDiscoverer>(assembly);
            framework.FindTestsForClass(null).ReturnsForAnyArgs(true);

            framework.Find();
            framework.Visitor.Finished.WaitOne();

            framework.Received(1).FindTestsForClass(objectTypeInfo, false);
            framework.Received(1).FindTestsForClass(intTypeInfo, false);
        }

        [Fact]
        public static void DoesNotCallSourceProviderWhenNotAskedFor()
        {
            var sourceProvider = Substitute.For<ISourceInformationProvider>();
            var typeInfo = Reflector.Wrap(typeof(ClassWithSingleTest));
            var mockAssembly = Mocks.AssemblyInfo(types: new[] { typeInfo });
            var framework = TestableXunitTestFrameworkDiscoverer.Create(mockAssembly, sourceProvider);

            framework.Find();
            framework.Visitor.Finished.WaitOne();

            sourceProvider.Received(0).GetSourceInformation(Arg.Any<ITestCase>());
        }

        [Fact]
        public static void CallsSourceProviderWhenTypesAreFoundInAssembly()
        {
            var sourceProvider = Substitute.For<ISourceInformationProvider>();
            sourceProvider.GetSourceInformation(null)
                          .ReturnsForAnyArgs(new Xunit.SourceInformation { FileName = "Source File", LineNumber = 42 });
            var typeInfo = Reflector.Wrap(typeof(ClassWithSingleTest));
            var mockAssembly = Mocks.AssemblyInfo(types: new[] { typeInfo });
            var framework = TestableXunitTestFrameworkDiscoverer.Create(mockAssembly, sourceProvider);

            framework.Find(includeSourceInformation: true);

            Assert.Collection(framework.TestCases,
                testCase =>
                {
                    Assert.Equal("XunitTestFrameworkDiscovererTests+ClassWithSingleTest.TestMethod", testCase.DisplayName);
                    Assert.Equal("Source File", testCase.SourceInformation.FileName);
                    Assert.Equal(42, testCase.SourceInformation.LineNumber);
                }
            );
        }
    }

    public class FindByTypeName
    {
        [Fact]
        public static void GuardClauses()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var typeName = typeof(Object).FullName;
            var sink = Substitute.For<IMessageSink>();
            var options = new TestFrameworkOptions();

            Assert.Throws<ArgumentNullException>("typeName", () => framework.Find(typeName: null, includeSourceInformation: false, messageSink: sink, options: options));
            Assert.Throws<ArgumentException>("typeName", () => framework.Find(typeName: "", includeSourceInformation: false, messageSink: sink, options: options));
            Assert.Throws<ArgumentNullException>("messageSink", () => framework.Find(typeName, includeSourceInformation: false, messageSink: null, options: options));
            Assert.Throws<ArgumentNullException>("options", () => framework.Find(typeName, includeSourceInformation: false, messageSink: sink, options: null));
        }

        [Fact]
        public static void RequestsPublicAndPrivateMethodsFromType()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Substitute.For<ITypeInfo>();
            framework.Assembly.GetType("abc").Returns(type);

            framework.Find("abc");
            framework.Visitor.Finished.WaitOne();

            type.Received(1).GetMethods(includePrivateMethods: true);
        }

        [Fact]
        public static void CallsFindImplWhenMethodsAreFoundOnType()
        {
            var framework = Substitute.For<TestableXunitTestFrameworkDiscoverer>();
            var type = Substitute.For<ITypeInfo>();
            framework.Assembly.GetType("abc").Returns(type);

            framework.Find("abc");
            framework.Visitor.Finished.WaitOne();

            framework.Received(1).FindTestsForClass(type, false);
        }

        [Fact]
        public static void ExcludesAbstractTypesFromDiscovery()
        {
            var framework = Substitute.For<TestableXunitTestFrameworkDiscoverer>();
            var type = Substitute.For<ITypeInfo>();
            type.IsAbstract.Returns(true);
            framework.Assembly.GetType("abc").Returns(type);

            framework.Find("abc");
            framework.Visitor.Finished.WaitOne();

            framework.Received(0).FindTestsForClass(type, Arg.Any<bool>());
        }

        [Fact]
        public static void DoesNotCallSourceProviderWhenNotAskedFor()
        {
            var sourceProvider = Substitute.For<ISourceInformationProvider>();
            var framework = TestableXunitTestFrameworkDiscoverer.Create(sourceProvider: sourceProvider);

            framework.Find("abc");

            sourceProvider.Received(0).GetSourceInformation(Arg.Any<ITestCase>());
        }

        [Fact]
        public static void CallsSourceProviderWhenTypesAreFoundInAssembly()
        {
            var sourceProvider = Substitute.For<ISourceInformationProvider>();
            sourceProvider.GetSourceInformation(null)
                          .ReturnsForAnyArgs(new Xunit.SourceInformation { FileName = "Source File", LineNumber = 42 });
            var framework = TestableXunitTestFrameworkDiscoverer.Create(sourceProvider: sourceProvider);
            var typeInfo = Reflector.Wrap(typeof(ClassWithSingleTest));
            framework.Assembly.GetType("abc").Returns(typeInfo);

            framework.Find("abc", includeSourceInformation: true);

            Assert.Collection(framework.TestCases,
                testCase =>
                {
                    Assert.Equal("XunitTestFrameworkDiscovererTests+ClassWithSingleTest.TestMethod", testCase.DisplayName);
                    Assert.Equal("Source File", testCase.SourceInformation.FileName);
                    Assert.Equal(42, testCase.SourceInformation.LineNumber);
                }
            );
        }
    }

    public class FindImpl
    {
        class ClassWithNoTests
        {
            public static void NonTestMethod() { }
        }

        [Fact]
        public static void ClassWithNoTests_ReturnsNoTestCases()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Reflector.Wrap(typeof(ClassWithNoTests));

            framework.FindTestsForClass(type);

            Assert.False(framework.Visitor.Finished.WaitOne(0));
        }

        class ClassWithOneFact
        {
            [Fact]
            public static void TestMethod() { }
        }

        [Fact]
        public static void AssemblyWithFact_ReturnsOneTestCaseOfTypeXunitTestCase()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Reflector.Wrap(typeof(ClassWithOneFact));

            framework.FindTestsForClass(type);

            Assert.Collection(framework.Visitor.TestCases,
                testCase => Assert.IsType<XunitTestCase>(testCase)
            );
        }

        class ClassWithMixOfFactsAndNonFacts
        {
            [Fact]
            public static void TestMethod1() { }

            [Fact]
            public static void TestMethod2() { }

            public static void NonTestMethod() { }
        }

        [Fact]
        public static void AssemblyWithMixOfFactsAndNonTests_ReturnsTestCasesOnlyForFacts()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Reflector.Wrap(typeof(ClassWithMixOfFactsAndNonFacts));

            framework.FindTestsForClass(type);

            Assert.Equal(2, framework.Visitor.TestCases.Count);
            Assert.Single(framework.Visitor.TestCases, t => t.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+ClassWithMixOfFactsAndNonFacts.TestMethod1");
            Assert.Single(framework.Visitor.TestCases, t => t.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+ClassWithMixOfFactsAndNonFacts.TestMethod2");
        }

        class TheoryWithInlineData
        {
            [Theory]
            [InlineData("Hello world")]
            [InlineData(42)]
            public static void TheoryMethod(object value) { }
        }

        [Fact]
        public static void AssemblyWithTheoryWithInlineData_ReturnsOneTestCasePerDataRecord()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Reflector.Wrap(typeof(TheoryWithInlineData));

            framework.FindTestsForClass(type);

            Assert.Equal(2, framework.Visitor.TestCases.Count);
            Assert.Single(framework.Visitor.TestCases, t => t.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithInlineData.TheoryMethod(value: \"Hello world\")");
            Assert.Single(framework.Visitor.TestCases, t => t.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithInlineData.TheoryMethod(value: 42)");
        }

        class TheoryWithPropertyData
        {
            public static IEnumerable<object[]> TheData
            {
                get
                {
                    yield return new object[] { 42 };
                    yield return new object[] { 2112 };
                }
            }

            [Theory]
            [MemberData("TheData")]
            public static void TheoryMethod(int value) { }
        }

        [Fact]
        public static void AssemblyWithTheoryWithPropertyData_ReturnsOneTestCasePerDataRecord()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Reflector.Wrap(typeof(TheoryWithPropertyData));

            framework.FindTestsForClass(type);

            Assert.Equal(2, framework.Visitor.TestCases.Count);
            Assert.Single(framework.Visitor.TestCases, testCase => testCase.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithPropertyData.TheoryMethod(value: 42)");
            Assert.Single(framework.Visitor.TestCases, testCase => testCase.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithPropertyData.TheoryMethod(value: 2112)");
        }

        class ClassWithNoCollection
        {
            [Fact]
            public static void TestMethod() { }
        }

        [Fact]
        public static void DefaultTestCollection()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Reflector.Wrap(typeof(ClassWithNoCollection));

            framework.FindTestsForClass(type);

            var testCase = Assert.Single(framework.Visitor.TestCases);
            Assert.NotNull(testCase.TestCollection);
            Assert.Equal("Test collection for XunitTestFrameworkDiscovererTests+FindImpl+ClassWithNoCollection", testCase.TestCollection.DisplayName);
            Assert.Null(testCase.TestCollection.CollectionDefinition);
        }

        [Collection("This a collection without declaration")]
        class ClassWithUndeclaredCollection
        {
            [Fact]
            public static void TestMethod() { }
        }

        [Fact]
        public static void UndeclaredTestCollection()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Reflector.Wrap(typeof(ClassWithUndeclaredCollection));

            framework.FindTestsForClass(type);

            var testCase = Assert.Single(framework.Visitor.TestCases);
            Assert.NotNull(testCase.TestCollection);
            Assert.Equal("This a collection without declaration", testCase.TestCollection.DisplayName);
            Assert.Null(testCase.TestCollection.CollectionDefinition);
        }

        [CollectionDefinition("This a defined collection")]
        public class DeclaredCollection { }

        [Collection("This a defined collection")]
        class ClassWithDefinedCollection
        {
            [Fact]
            public static void TestMethod() { }
        }

        [Fact]
        public static void DefinedTestCollection()
        {
            var type = Reflector.Wrap(typeof(ClassWithDefinedCollection));
            var framework = TestableXunitTestFrameworkDiscoverer.Create(type.Assembly);

            framework.FindTestsForClass(type);

            var testCase = Assert.Single(framework.Visitor.TestCases);
            Assert.NotNull(testCase.TestCollection);
            Assert.Equal("This a defined collection", testCase.TestCollection.DisplayName);
            Assert.NotNull(testCase.TestCollection.CollectionDefinition);
            Assert.Equal("XunitTestFrameworkDiscovererTests+FindImpl+DeclaredCollection", testCase.TestCollection.CollectionDefinition.Name);
        }
    }

    class ClassWithSingleTest
    {
        [Fact]
        public static void TestMethod() { }
    }

    abstract class AbstractClass
    {
        [Fact]
        public static void ATestNotToBeRun() { }
    }

    public class TestableXunitTestFrameworkDiscoverer : XunitTestFrameworkDiscoverer
    {
        protected TestableXunitTestFrameworkDiscoverer()
            : this(Mocks.AssemblyInfo()) { }

        protected TestableXunitTestFrameworkDiscoverer(IAssemblyInfo assembly)
            : this(assembly, null, null, null) { }

        protected TestableXunitTestFrameworkDiscoverer(IAssemblyInfo assembly,
                                                       ISourceInformationProvider sourceProvider,
                                                       IXunitTestCollectionFactory collectionFactory,
                                                       IMessageAggregator messageAggregator)
            : base(assembly,
                   sourceProvider ?? Substitute.For<ISourceInformationProvider>(),
                   collectionFactory,
                   messageAggregator)
        {
            Assembly = assembly;
            Visitor = new TestDiscoveryVisitor();
        }

        public IAssemblyInfo Assembly { get; private set; }

        public List<ITestCase> TestCases
        {
            get
            {
                Visitor.Finished.WaitOne();
                return Visitor.TestCases;
            }
        }

        internal TestDiscoveryVisitor Visitor { get; private set; }

        public static TestableXunitTestFrameworkDiscoverer Create(IAssemblyInfo assembly = null,
                                                                  ISourceInformationProvider sourceProvider = null,
                                                                  IXunitTestCollectionFactory collectionFactory = null,
                                                                  IMessageAggregator messageAggregator = null)
        {
            return new TestableXunitTestFrameworkDiscoverer(assembly ?? Mocks.AssemblyInfo(), sourceProvider, collectionFactory, messageAggregator);
        }

        public void Find(bool includeSourceInformation = false)
        {
            base.Find(includeSourceInformation, Visitor, new TestFrameworkOptions());
            Visitor.Finished.WaitOne();
        }

        public void Find(string typeName, bool includeSourceInformation = false)
        {
            base.Find(typeName, includeSourceInformation, Visitor, new TestFrameworkOptions());
            Visitor.Finished.WaitOne();
        }

        public virtual bool FindTestsForClass(ITypeInfo type, bool includeSourceInformation = false)
        {
            using (var messageBus = new MessageBus(Visitor))
                return base.FindTestsForType(type, includeSourceInformation, messageBus);
        }

        protected sealed override bool FindTestsForType(ITypeInfo type, bool includeSourceInformation, IMessageBus messageBus)
        {
            return FindTestsForClass(type, includeSourceInformation);
        }

        protected sealed override bool IsValidTestClass(ITypeInfo type)
        {
            return base.IsValidTestClass(type);
        }
    }
}