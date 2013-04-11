using System;
using System.Collections.Generic;
using System.Linq;
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
            Assert.Throws<ArgumentNullException>(() => new XunitTestFrameworkDiscoverer(assemblyInfo: null), "assemblyInfo");
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

            Assert.Collection(framework.Messages,
                message => Assert.IsAssignableFrom<IDiscoveryCompleteMessage>(message)
            );
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

            sourceProvider.Received(0).GetSourceInformation(Arg.Any<ITestCase>());
        }

        [Fact]
        public void CallsSourceProviderWhenTypesAreFoundInAssembly()
        {
            var sourceProvider = Substitute.For<ISourceInformationProvider>();
            sourceProvider.GetSourceInformation(null)
                          .ReturnsForAnyArgs(Tuple.Create<string, int?>("Source File", 42));
            var typeInfo = Reflector.Wrap(typeof(ClassWithSingleTest));
            var mockAssembly = Mocks.AssemblyInfo(types: new[] { typeInfo });
            var framework = TestableXunitTestFrameworkDiscoverer.Create(mockAssembly, sourceProvider);

            framework.Find(includeSourceInformation: true);

            Assert.Collection(framework.Messages,
                message =>
                {
                    var discoveryMessage = Assert.IsAssignableFrom<ITestCaseDiscoveryMessage>(message);
                    Assert.Equal("XunitTestFrameworkDiscovererTests+ClassWithSingleTest.TestMethod", discoveryMessage.TestCase.DisplayName);
                    Assert.Equal("Source File", discoveryMessage.TestCase.SourceFileName);
                    Assert.Equal(42, discoveryMessage.TestCase.SourceFileLine);
                },
                message => Assert.IsAssignableFrom<IDiscoveryCompleteMessage>(message)
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

            type.Received(1).GetMethods(includePrivateMethods: true);
        }

        [Fact]
        public void CallsFindImplWhenMethodsAreFoundOnType()
        {
            var framework = Substitute.For<TestableXunitTestFrameworkDiscoverer>();
            var type = Substitute.For<ITypeInfo>();
            framework.Assembly.GetType("abc").Returns(type);

            framework.Find("abc");

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
                          .ReturnsForAnyArgs(Tuple.Create<string, int?>("Source File", 42));
            var framework = TestableXunitTestFrameworkDiscoverer.Create(sourceProvider: sourceProvider);
            var typeInfo = Reflector.Wrap(typeof(ClassWithSingleTest));
            framework.Assembly.GetType("abc").Returns(typeInfo);

            framework.Find("abc", includeSourceInformation: true);

            Assert.Collection(framework.Messages,
                message =>
                {
                    var discoveryMessage = Assert.IsAssignableFrom<ITestCaseDiscoveryMessage>(message);
                    Assert.Equal("XunitTestFrameworkDiscovererTests+ClassWithSingleTest.TestMethod", discoveryMessage.TestCase.DisplayName);
                    Assert.Equal("Source File", discoveryMessage.TestCase.SourceFileName);
                    Assert.Equal(42, discoveryMessage.TestCase.SourceFileLine);
                },
                message => Assert.IsAssignableFrom<IDiscoveryCompleteMessage>(message)
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

            Assert.Empty(framework.Messages);
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

            var discoveryMessage = (ITestCaseDiscoveryMessage)Assert.Single(framework.Messages, msg => msg is ITestCaseDiscoveryMessage);
            Assert.IsType<XunitTestCase>(discoveryMessage.TestCase);
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

            var results = framework.Messages
                                   .OfType<ITestCaseDiscoveryMessage>()
                                   .Select(msg => msg.TestCase)
                                   .ToArray();
            Assert.Equal(2, results.Count());
            Assert.Single(results, t => t.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+ClassWithMixOfFactsAndNonFacts.TestMethod1");
            Assert.Single(results, t => t.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+ClassWithMixOfFactsAndNonFacts.TestMethod2");
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

            var results = framework.Messages
                                   .OfType<ITestCaseDiscoveryMessage>()
                                   .Select(msg => msg.TestCase)
                                   .ToArray();
            Assert.Equal(2, results.Count());
            Assert.Single(results, t => t.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithInlineData.TheoryMethod(value: \"Hello world\")");
            Assert.Single(results, t => t.DisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithInlineData.TheoryMethod(value: 42)");
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

            var results = framework.Messages
                                   .OfType<ITestCaseDiscoveryMessage>()
                                   .Select(msg => msg.TestCase)
                                   .Select(tc => tc.DisplayName)
                                   .ToArray();
            Assert.Equal(2, results.Count());
            Assert.Single(results, name => name == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithPropertyData.TheoryMethod(value: 42)");
            Assert.Single(results, name => name == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithPropertyData.TheoryMethod(value: 2112)");
        }
    }

    class ClassWithSingleTest
    {
        [Fact]
        public void TestMethod() { }
    }

    public class TestableXunitTestFrameworkDiscoverer : XunitTestFrameworkDiscoverer, IMessageSink
    {
        protected TestableXunitTestFrameworkDiscoverer()
            : this(Mocks.AssemblyInfo(), Substitute.For<ISourceInformationProvider>()) { }

        protected TestableXunitTestFrameworkDiscoverer(IAssemblyInfo assembly)
            : base(assembly)
        {
            Assembly = assembly;
        }

        TestableXunitTestFrameworkDiscoverer(IAssemblyInfo assembly, ISourceInformationProvider sourceProvider)
            : base(assembly, sourceProvider)
        {
            Assembly = assembly;
        }

        public IAssemblyInfo Assembly { get; private set; }

        public List<ITestMessage> Messages = new List<ITestMessage>();

        public static TestableXunitTestFrameworkDiscoverer Create(IAssemblyInfo assembly = null, ISourceInformationProvider sourceProvider = null)
        {
            return new TestableXunitTestFrameworkDiscoverer(assembly ?? Mocks.AssemblyInfo(),
                                                            sourceProvider ?? Substitute.For<ISourceInformationProvider>());
        }

        public void Find(bool includeSourceInformation = false)
        {
            base.Find(includeSourceInformation, this);
        }

        public void Find(string typeName, bool includeSourceInformation = false)
        {
            base.Find(typeName, includeSourceInformation, this);
        }

        public virtual bool FindImpl(ITypeInfo type, bool includeSourceInformation = false)
        {
            return base.FindImpl(type, includeSourceInformation, this);
        }

        protected sealed override bool FindImpl(ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink)
        {
            return FindImpl(type, includeSourceInformation);
        }

        public bool OnMessage(ITestMessage message)
        {
            Messages.Add(message);

            return true;
        }
    }
}