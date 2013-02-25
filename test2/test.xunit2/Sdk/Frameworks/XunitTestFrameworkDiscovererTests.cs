using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
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
            Assert.ThrowsArgumentNull(() => new XunitTestFrameworkDiscoverer(assemblyInfo: null), "assemblyInfo");
        }
    }

    public class FindByAssembly
    {
        [Fact]
        public void GuardClause()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var sink = new Mock<IMessageSink>();

            Assert.ThrowsArgumentNull(
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

            framework.Assembly.Verify(a => a.GetTypes(/*includePrivateTypes*/ false), Times.Once());
        }

        [Fact]
        public void CallsFindImplWhenTypesAreFoundInAssembly()
        {
            var objectTypeInfo = Reflector.Wrap(typeof(object));
            var intTypeInfo = Reflector.Wrap(typeof(int));
            var mockAssembly = new MockAssemblyInfo(types: new[] { objectTypeInfo, intTypeInfo });
            var mockFramework = new Mock<TestableXunitTestFrameworkDiscoverer>(mockAssembly) { CallBase = true };

            mockFramework.Object.Find();

            mockFramework.Verify(f => f.FindImpl(objectTypeInfo, false), Times.Once());
            mockFramework.Verify(f => f.FindImpl(intTypeInfo, false), Times.Once());
        }

        [Fact]
        public void DoesNotCallSourceProviderWhenNotAskedFor()
        {
            var sourceProvider = new Mock<ISourceInformationProvider>(MockBehavior.Strict);
            var typeInfo = Reflector.Wrap(typeof(ClassWithSingleTest));
            var mockAssembly = new MockAssemblyInfo(types: new[] { typeInfo });
            var framework = TestableXunitTestFrameworkDiscoverer.Create(mockAssembly, sourceProvider);

            framework.Find();

            sourceProvider.Verify(sp => sp.GetSourceInformation(It.IsAny<ITestCase>()), Times.Never());
        }

        [Fact]
        public void CallsSourceProviderWhenTypesAreFoundInAssembly()
        {
            var sourceProvider = new Mock<ISourceInformationProvider>();
            sourceProvider.Setup(sp => sp.GetSourceInformation(It.IsAny<ITestCase>()))
                          .Returns(Tuple.Create<string, int?>("Source File", 42));
            var typeInfo = Reflector.Wrap(typeof(ClassWithSingleTest));
            var mockAssembly = new MockAssemblyInfo(types: new[] { typeInfo });
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
            var sink = new Mock<IMessageSink>();

            Assert.ThrowsArgumentNull(
                () => framework.Find(typeName: null, includeSourceInformation: false, messageSink: sink.Object),
                "typeName"
            );
            Assert.ThrowsArgument(
                () => framework.Find(typeName: "", includeSourceInformation: false, messageSink: sink.Object),
                "typeName"
            );
            Assert.ThrowsArgumentNull(
                () => framework.Find(typeName, includeSourceInformation: false, messageSink: null),
                "messageSink"
            );
        }

        [Fact]
        public void RequestsPublicAndPrivateMethodsFromType()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = new Mock<ITypeInfo>();
            framework.Assembly.Setup(a => a.GetType("abc")).Returns(type.Object);

            framework.Find("abc");

            type.Verify(t => t.GetMethods(/*includePrivateMethods*/ true), Times.Once());
        }

        [Fact]
        public void CallsFindImplWhenMethodsAreFoundOnType()
        {
            var mockFramework = new Mock<TestableXunitTestFrameworkDiscoverer> { CallBase = true };
            var type = new Mock<ITypeInfo>();
            mockFramework.Object.Assembly.Setup(a => a.GetType("abc")).Returns(type.Object);

            mockFramework.Object.Find("abc");

            mockFramework.Verify(f => f.FindImpl(type.Object, false), Times.Once());
        }

        [Fact]
        public void DoesNotCallSourceProviderWhenNotAskedFor()
        {
            var sourceProvider = new Mock<ISourceInformationProvider>(MockBehavior.Strict);
            var framework = TestableXunitTestFrameworkDiscoverer.Create(sourceProvider: sourceProvider);

            framework.Find("abc");

            sourceProvider.Verify(sp => sp.GetSourceInformation(It.IsAny<ITestCase>()), Times.Never());
        }

        [Fact]
        public void CallsSourceProviderWhenTypesAreFoundInAssembly()
        {
            var sourceProvider = new Mock<ISourceInformationProvider>();
            sourceProvider.Setup(sp => sp.GetSourceInformation(It.IsAny<ITestCase>()))
                          .Returns(Tuple.Create<string, int?>("Source File", 42));
            var framework = TestableXunitTestFrameworkDiscoverer.Create(sourceProvider: sourceProvider);
            var typeInfo = Reflector.Wrap(typeof(ClassWithSingleTest));
            framework.Assembly.Setup(a => a.GetType("abc")).Returns(typeInfo);

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
                                   .Cast<IMethodTestCase>()
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
                                   .Cast<IMethodTestCase>()
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
            //[PropertyData("TheData")]
            public void TheoryMethod(int value) { }
        }

        [Fact(Skip = "Working towards this...")]
        public void AssemblyWithTheoryWithPropertyData_ReturnsOneTestCasePerDataRecord()
        {
            var framework = TestableXunitTestFrameworkDiscoverer.Create();
            var type = Reflector.Wrap(typeof(TheoryWithPropertyData));

            framework.FindImpl(type);

            var results = framework.Messages
                                   .OfType<ITestCaseDiscoveryMessage>()
                                   .Select(msg => msg.TestCase)
                                   .Cast<IMethodTestCase>()
                                   .Select(tc => tc.DisplayName)
                                   .ToArray();
            Assert.Equal(2, results.Count());
            Assert.Single(results, name => name == "XunitTestFrameworkTests+FindImpl+TheoryWithPropertyData.TheoryMethod(value: 42)");
            Assert.Single(results, name => name == "XunitTestFrameworkTests+FindImpl+TheoryWithPropertyData.TheoryMethod(value: 2112)");
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
            : this(new MockAssemblyInfo(), new Mock<ISourceInformationProvider>()) { }

        protected TestableXunitTestFrameworkDiscoverer(MockAssemblyInfo assembly)
            : base(assembly.Object)
        {
            Assembly = assembly;
        }

        TestableXunitTestFrameworkDiscoverer(MockAssemblyInfo assembly, Mock<ISourceInformationProvider> sourceProvider)
            : base(assembly.Object, sourceProvider.Object)
        {
            Assembly = assembly;
            SourceProvider = sourceProvider;
        }

        public MockAssemblyInfo Assembly { get; private set; }

        public List<ITestMessage> Messages = new List<ITestMessage>();

        public Mock<ISourceInformationProvider> SourceProvider { get; private set; }

        public static TestableXunitTestFrameworkDiscoverer Create(MockAssemblyInfo assembly = null, Mock<ISourceInformationProvider> sourceProvider = null)
        {
            return new TestableXunitTestFrameworkDiscoverer(assembly ?? new MockAssemblyInfo(),
                                                            sourceProvider ?? new Mock<ISourceInformationProvider>());
        }

        public void Find(bool includeSourceInformation = false)
        {
            base.Find(includeSourceInformation, this);
        }

        public void Find(string typeName, bool includeSourceInformation = false)
        {
            base.Find(typeName, includeSourceInformation, this);
        }

        public virtual void FindImpl(ITypeInfo type, bool includeSourceInformation = false)
        {
            base.FindImpl(type, includeSourceInformation, this);
        }

        protected override void FindImpl(ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink)
        {
            FindImpl(type, includeSourceInformation);
        }

        public void OnMessage(ITestMessage message)
        {
            Messages.Add(message);
        }
    }
}
