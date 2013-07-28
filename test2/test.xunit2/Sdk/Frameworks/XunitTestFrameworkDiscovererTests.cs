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
        public void GuardClause()
        {
            Assert.Throws<ArgumentNullException>(() => new XunitTestFrameworkDiscoverer(null, Substitute.For<ISourceInformationProvider>()), "assemblyInfo");
            Assert.Throws<ArgumentNullException>(() => new XunitTestFrameworkDiscoverer(Substitute.For<IAssemblyInfo>(), null), "sourceProvider");
        }

        [Fact]
        public void DefaultTestCollectionFactoryIsCollectionPerClass()
        {
            var assembly = Mocks.AssemblyInfo();
            var sourceInfoProvider = Substitute.For<ISourceInformationProvider>();

            var discoverer = new XunitTestFrameworkDiscoverer(assembly, sourceInfoProvider);

            Assert.IsType<CollectionPerClassTestCollectionFactory>(discoverer.TestCollectionFactory);
            Assert.Equal(XunitTestFrameworkDiscoverer.DisplayName + " [collection-per-class, non-parallel]", discoverer.TestFrameworkDisplayName);
        }

        [Fact]
        public void UserCanChooseFromBuiltInCollectionFactories()
        {
            var attr = Mocks.CollectionBehaviorAttribute(CollectionBehavior.CollectionPerAssembly);
            var assembly = Mocks.AssemblyInfo(attributes: new[] { attr });
            var sourceInfoProvider = Substitute.For<ISourceInformationProvider>();

            var discoverer = new XunitTestFrameworkDiscoverer(assembly, sourceInfoProvider);

            Assert.IsType<CollectionPerAssemblyTestCollectionFactory>(discoverer.TestCollectionFactory);
            Assert.Equal(XunitTestFrameworkDiscoverer.DisplayName + " [collection-per-assembly, non-parallel]", discoverer.TestFrameworkDisplayName);
        }

        [Fact]
        public void UserCanChooseCustomCollectionFactory()
        {
            var factoryType = typeof(MyTestCollectionFactory);
            var attr = Mocks.CollectionBehaviorAttribute(factoryType.FullName, factoryType.Assembly.FullName);
            var assembly = Mocks.AssemblyInfo(attributes: new[] { attr });
            var sourceInfoProvider = Substitute.For<ISourceInformationProvider>();

            var discoverer = new XunitTestFrameworkDiscoverer(assembly, sourceInfoProvider);

            Assert.IsType<MyTestCollectionFactory>(discoverer.TestCollectionFactory);
            Assert.Equal(XunitTestFrameworkDiscoverer.DisplayName + " [My Factory, non-parallel]", discoverer.TestFrameworkDisplayName);
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
        public void IncompatibleOrInvalidTypesGetDefaultBehavior(string factoryTypeName)
        {
            var attr = Mocks.CollectionBehaviorAttribute(factoryTypeName, "test.xunit2");
            var assembly = Mocks.AssemblyInfo(attributes: new[] { attr });
            var sourceInfoProvider = Substitute.For<ISourceInformationProvider>();

            var discoverer = new XunitTestFrameworkDiscoverer(assembly, sourceInfoProvider);

            Assert.IsType<CollectionPerClassTestCollectionFactory>(discoverer.TestCollectionFactory);
            Assert.Equal(XunitTestFrameworkDiscoverer.DisplayName + " [collection-per-class, non-parallel]", discoverer.TestFrameworkDisplayName);
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

    public class FindByAssembly
    {
        [Fact]
        public void GuardClause()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();

            Assert.Throws<ArgumentNullException>(
                () => framework.Find(includeSourceInformation: false, messageSink: null),
                "messageSink"
            );
        }

        [Fact]
        public void AssemblyWithNoTypes_ReturnsNoTestCases()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();

            framework.Find();

            Assert.Empty(framework.TestCases);
        }

        [Fact]
        public void RequestsOnlyPublicTypesFromAssembly()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();

            framework.Find();

            framework.Assembly.Received(1).GetTypes(/*includePrivateTypes*/ false);
        }

        [Fact]
        public void CallsFindImplWhenTypesAreFoundInAssembly()
        {
            var objectTypeInfo = Reflector.Wrap(typeof(object));
            var intTypeInfo = Reflector.Wrap(typeof(int));
            var assembly = Mocks.AssemblyInfo(types: new[] { objectTypeInfo, intTypeInfo });
            var framework = Substitute.For<TestableXunitTestFrameworkDiscoverer>(assembly);
            framework.FindImpl(null).ReturnsForAnyArgs(true);

            framework.Find();
            framework.Visitor.Finished.WaitOne();

            framework.Received(1).FindImpl(objectTypeInfo, false);
            framework.Received(1).FindImpl(intTypeInfo, false);
        }

        [Fact]
        public void DoesNotCallSourceProviderWhenNotAskedFor()
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
        public void CallsSourceProviderWhenTypesAreFoundInAssembly()
        {
            var sourceProvider = Substitute.For<ISourceInformationProvider>();
            sourceProvider.GetSourceInformation(null)
                          .ReturnsForAnyArgs(new SourceInformation { FileName = "Source File", LineNumber = 42 });
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
        public void GuardClauses()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var typeName = typeof(Object).FullName;
            var sink = Substitute.For<IMessageSink>();

            Assert.Throws<ArgumentNullException>(
                () => framework.Find(typeName: null, includeSourceInformation: false, messageSink: sink),
                "typeName"
            );
            Assert.Throws<ArgumentException>(
                () => framework.Find(typeName: "", includeSourceInformation: false, messageSink: sink),
                "typeName"
            );
            Assert.Throws<ArgumentNullException>(
                () => framework.Find(typeName, includeSourceInformation: false, messageSink: null),
                "messageSink"
            );
        }

        [Fact]
        public void RequestsPublicAndPrivateMethodsFromType()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Substitute.For<ITypeInfo>();
            framework.Assembly.GetType("abc").Returns(type);

            framework.Find("abc");
            framework.Visitor.Finished.WaitOne();

            type.Received(1).GetMethods(includePrivateMethods: true);
        }

        [Fact]
        public void CallsFindImplWhenMethodsAreFoundOnType()
        {
            var framework = Substitute.For<TestableXunitTestFrameworkDiscoverer>();
            var type = Substitute.For<ITypeInfo>();
            framework.Assembly.GetType("abc").Returns(type);

            framework.Find("abc");
            framework.Visitor.Finished.WaitOne();

            framework.Received(1).FindImpl(type, false);
        }

        [Fact]
        public void DoesNotCallSourceProviderWhenNotAskedFor()
        {
            var sourceProvider = Substitute.For<ISourceInformationProvider>();
            var framework = TestableXunitTestFrameworkDiscoverer.Create(sourceProvider: sourceProvider);

            framework.Find("abc");

            sourceProvider.Received(0).GetSourceInformation(Arg.Any<ITestCase>());
        }

        [Fact]
        public void CallsSourceProviderWhenTypesAreFoundInAssembly()
        {
            var sourceProvider = Substitute.For<ISourceInformationProvider>();
            sourceProvider.GetSourceInformation(null)
                          .ReturnsForAnyArgs(new SourceInformation { FileName = "Source File", LineNumber = 42 });
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
            public void NonTestMethod() { }
        }

        [Fact]
        public void ClassWithNoTests_ReturnsNoTestCases()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Reflector.Wrap(typeof(ClassWithNoTests));

            framework.FindImpl(type);

            Assert.False(framework.Visitor.Finished.WaitOne(0));
        }

        class ClassWithOneFact
        {
            [Fact]
            public void TestMethod() { }
        }

        [Fact]
        public void AssemblyWithFact_ReturnsOneTestCaseOfTypeXunitTestCase()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Reflector.Wrap(typeof(ClassWithOneFact));

            framework.FindImpl(type);

            Assert.Collection(framework.Visitor.TestCases,
                testCase => Assert.IsType<XunitTestCase>(testCase)
            );
        }

        class ClassWithMixOfFactsAndNonFacts
        {
            [Fact]
            public void TestMethod1() { }

            [Fact]
            public void TestMethod2() { }

            public void NonTestMethod() { }
        }

        [Fact]
        public void AssemblyWithMixOfFactsAndNonTests_ReturnsTestCasesOnlyForFacts()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Reflector.Wrap(typeof(ClassWithMixOfFactsAndNonFacts));

            framework.FindImpl(type);

            Assert.Equal(2, framework.Visitor.TestCases.Count);
            Assert.Single(framework.Visitor.TestCases, t => t.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+ClassWithMixOfFactsAndNonFacts.TestMethod1");
            Assert.Single(framework.Visitor.TestCases, t => t.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+ClassWithMixOfFactsAndNonFacts.TestMethod2");
        }

        class TheoryWithInlineData
        {
            [Theory]
            [InlineData("Hello world")]
            [InlineData(42)]
            public void TheoryMethod(object value) { }
        }

        [Fact]
        public void AssemblyWithTheoryWithInlineData_ReturnsOneTestCasePerDataRecord()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Reflector.Wrap(typeof(TheoryWithInlineData));

            framework.FindImpl(type);

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
            [PropertyData("TheData")]
            public void TheoryMethod(int value) { }
        }

        [Fact]
        public void AssemblyWithTheoryWithPropertyData_ReturnsOneTestCasePerDataRecord()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Reflector.Wrap(typeof(TheoryWithPropertyData));

            framework.FindImpl(type);

            Assert.Equal(2, framework.Visitor.TestCases.Count);
            Assert.Single(framework.Visitor.TestCases, testCase => testCase.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithPropertyData.TheoryMethod(value: 42)");
            Assert.Single(framework.Visitor.TestCases, testCase => testCase.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithPropertyData.TheoryMethod(value: 2112)");
        }
    }

    class ClassWithSingleTest
    {
        [Fact]
        public void TestMethod() { }
    }

    public class TestableXunitTestFrameworkDiscoverer : XunitTestFrameworkDiscoverer
    {
        protected TestableXunitTestFrameworkDiscoverer()
            : this(Mocks.AssemblyInfo(), Substitute.For<ISourceInformationProvider>()) { }

        protected TestableXunitTestFrameworkDiscoverer(IAssemblyInfo assembly)
            : this(assembly, Substitute.For<ISourceInformationProvider>()) { }

        TestableXunitTestFrameworkDiscoverer(IAssemblyInfo assembly, ISourceInformationProvider sourceProvider)
            : base(assembly, sourceProvider)
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

        public TestDiscoveryVisitor Visitor { get; private set; }

        public static TestableXunitTestFrameworkDiscoverer Create(IAssemblyInfo assembly = null, ISourceInformationProvider sourceProvider = null)
        {
            return new TestableXunitTestFrameworkDiscoverer(assembly ?? Mocks.AssemblyInfo(),
                                                            sourceProvider ?? Substitute.For<ISourceInformationProvider>());
        }

        public void Find(bool includeSourceInformation = false)
        {
            base.Find(includeSourceInformation, Visitor);
        }

        public void Find(string typeName, bool includeSourceInformation = false)
        {
            base.Find(typeName, includeSourceInformation, Visitor);
        }

        public virtual bool FindImpl(ITypeInfo type, bool includeSourceInformation = false)
        {
            return base.FindImpl(type, includeSourceInformation, Visitor);
        }

        protected sealed override bool FindImpl(ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink)
        {
            return FindImpl(type, includeSourceInformation);
        }
    }
}