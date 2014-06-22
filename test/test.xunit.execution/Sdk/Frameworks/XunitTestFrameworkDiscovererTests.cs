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

            framework.Received(0).FindTestsForClass(Arg.Any<ITestClass>(), Arg.Any<bool>());
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

            framework.Received(1).FindTestsForClass(Arg.Is<ITestClass>(testClass => testClass.Class == objectTypeInfo), false);
            framework.Received(1).FindTestsForClass(Arg.Is<ITestClass>(testClass => testClass.Class == intTypeInfo), false);
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

            framework.Received(1).FindTestsForClass(Arg.Is<ITestClass>(testClass => testClass.Class == type), false);
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

            framework.Received(0).FindTestsForClass(Arg.Is<ITestClass>(testClass => testClass.Class == type), Arg.Any<bool>());
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
            var testClass = new XunitTestClass(Mocks.TestCollection(), Reflector.Wrap(typeof(ClassWithNoTests)));

            framework.FindTestsForClass(testClass);

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
            var testClass = new XunitTestClass(Mocks.TestCollection(), Reflector.Wrap(typeof(ClassWithOneFact)));

            framework.FindTestsForClass(testClass);

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
            var testClass = new XunitTestClass(Mocks.TestCollection(), Reflector.Wrap(typeof(ClassWithMixOfFactsAndNonFacts)));

            framework.FindTestsForClass(testClass);

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
            var testClass = Mocks.XunitTestClass(typeof(TheoryWithInlineData));

            framework.FindTestsForClass(testClass);

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
            var testClass = Mocks.XunitTestClass(typeof(TheoryWithPropertyData));

            framework.FindTestsForClass(testClass);

            Assert.Equal(2, framework.Visitor.TestCases.Count);
            Assert.Single(framework.Visitor.TestCases, testCase => testCase.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithPropertyData.TheoryMethod(value: 42)");
            Assert.Single(framework.Visitor.TestCases, testCase => testCase.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithPropertyData.TheoryMethod(value: 2112)");
        }
    }

    public class CreateTestClass
    {
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

            var testClass = framework.CreateTestClass(type);

            Assert.NotNull(testClass.TestCollection);
            Assert.Equal("Test collection for XunitTestFrameworkDiscovererTests+CreateTestClass+ClassWithNoCollection", testClass.TestCollection.DisplayName);
            Assert.Null(testClass.TestCollection.CollectionDefinition);
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

            var testClass = framework.CreateTestClass(type);

            Assert.NotNull(testClass.TestCollection);
            Assert.Equal("This a collection without declaration", testClass.TestCollection.DisplayName);
            Assert.Null(testClass.TestCollection.CollectionDefinition);
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

            var testClass = framework.CreateTestClass(type);

            Assert.NotNull(testClass.TestCollection);
            Assert.Equal("This a defined collection", testClass.TestCollection.DisplayName);
            Assert.NotNull(testClass.TestCollection.CollectionDefinition);
            Assert.Equal("XunitTestFrameworkDiscovererTests+CreateTestClass+DeclaredCollection", testClass.TestCollection.CollectionDefinition.Name);
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

        public new ITestClass CreateTestClass(ITypeInfo @class)
        {
            return base.CreateTestClass(@class);
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

        public virtual bool FindTestsForClass(ITestClass testClass, bool includeSourceInformation = false)
        {
            using (var messageBus = new MessageBus(Visitor))
                return base.FindTestsForType(testClass, includeSourceInformation, messageBus);
        }

        protected sealed override bool FindTestsForType(ITestClass testClass, bool includeSourceInformation, IMessageBus messageBus)
        {
            return FindTestsForClass(testClass, includeSourceInformation);
        }

        protected sealed override bool IsValidTestClass(ITypeInfo type)
        {
            return base.IsValidTestClass(type);
        }
    }
}