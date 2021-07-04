using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestFrameworkDiscovererTests
{
	public class Construction
	{
		[Fact]
		public static void GuardClause()
		{
			var assembly = Substitute.For<_IAssemblyInfo>();
			var sourceProvider = Substitute.For<_ISourceInformationProvider>();
			var diagnosticMessageSink = SpyMessageSink.Create();

			Assert.Throws<ArgumentNullException>("assemblyInfo", () => new XunitTestFrameworkDiscoverer(assemblyInfo: null!, configFileName: null, sourceProvider, diagnosticMessageSink));
			Assert.Throws<ArgumentNullException>("sourceProvider", () => new XunitTestFrameworkDiscoverer(assembly, configFileName: null, sourceProvider: null!, diagnosticMessageSink));
			Assert.Throws<ArgumentNullException>("diagnosticMessageSink", () => new XunitTestFrameworkDiscoverer(assembly, configFileName: null, sourceProvider, diagnosticMessageSink: null!));
		}
	}

	public static class FindByAssembly
	{
		[Fact]
		public static void GuardClauses()
		{
			var framework = TestableXunitTestFrameworkDiscoverer.Create();

			Assert.Throws<ArgumentNullException>("discoveryMessageSink", () => framework.Find(discoveryMessageSink: null!, discoveryOptions: _TestFrameworkOptions.ForDiscovery()));
			Assert.Throws<ArgumentNullException>("discoveryOptions", () => framework.Find(discoveryMessageSink: Substitute.For<_IMessageSink>(), discoveryOptions: null!));
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
			framework.FindTestsForClass(null!).ReturnsForAnyArgs(true);

			framework.Find();
			framework.Sink.Finished.WaitOne();

			_ = framework.Received(0).FindTestsForClass(Arg.Any<_ITestClass>());
		}

		[Fact]
		public static void CallsFindImplWhenTypesAreFoundInAssembly()
		{
			var objectTypeInfo = Reflector.Wrap(typeof(object));
			var intTypeInfo = Reflector.Wrap(typeof(int));
			var assembly = Mocks.AssemblyInfo(types: new[] { objectTypeInfo, intTypeInfo });
			var framework = Substitute.For<TestableXunitTestFrameworkDiscoverer>(assembly);
			framework.FindTestsForClass(null!).ReturnsForAnyArgs(true);

			framework.Find();
			framework.Sink.Finished.WaitOne();

			_ = framework.Received(1).FindTestsForClass(Arg.Is<_ITestClass>(testClass => testClass.Class == objectTypeInfo));
			_ = framework.Received(1).FindTestsForClass(Arg.Is<_ITestClass>(testClass => testClass.Class == intTypeInfo));
		}

		[Fact]
		public static void DoesNotCallSourceProviderWhenNotAskedFor()
		{
			var sourceProvider = Substitute.For<_ISourceInformationProvider>();
			var typeInfo = Reflector.Wrap(typeof(ClassWithSingleTest));
			var mockAssembly = Mocks.AssemblyInfo(types: new[] { typeInfo });
			var framework = TestableXunitTestFrameworkDiscoverer.Create(mockAssembly, sourceProvider);

			framework.Find();
			framework.Sink.Finished.WaitOne();

			sourceProvider.Received(0).GetSourceInformation(Arg.Any<string?>(), Arg.Any<string?>());
		}

		[Fact]
		public static void SendsDiscoveryStartingMessage()
		{
			var typeInfo = Reflector.Wrap(typeof(ClassWithSingleTest));
			var mockAssembly = Mocks.AssemblyInfo(types: new[] { typeInfo });
			var framework = TestableXunitTestFrameworkDiscoverer.Create(mockAssembly);

			framework.Find();
			framework.Sink.Finished.WaitOne();

			Assert.True(framework.Sink.StartSeen);
		}
	}

	public class FindByTypeName
	{
		[Fact]
		public static void GuardClauses()
		{
			var framework = TestableXunitTestFrameworkDiscoverer.Create();
			var typeName = typeof(object).FullName!;
			var sink = Substitute.For<_IMessageSink>();
			var options = _TestFrameworkOptions.ForDiscovery();

			Assert.Throws<ArgumentNullException>("typeName", () => framework.Find(typeName: null!, discoveryMessageSink: sink, discoveryOptions: options));
			Assert.Throws<ArgumentException>("typeName", () => framework.Find(typeName: "", discoveryMessageSink: sink, discoveryOptions: options));
			Assert.Throws<ArgumentNullException>("discoveryMessageSink", () => framework.Find(typeName, discoveryMessageSink: null!, discoveryOptions: options));
			Assert.Throws<ArgumentNullException>("discoveryOptions", () => framework.Find(typeName, discoveryMessageSink: sink, discoveryOptions: null!));
		}

		[Fact]
		public static void RequestsPublicAndPrivateMethodsFromType()
		{
			var framework = TestableXunitTestFrameworkDiscoverer.Create();
			var type = Substitute.For<_ITypeInfo>();
			framework.Assembly.GetType("abc").Returns(type);

			framework.Find("abc");
			framework.Sink.Finished.WaitOne();

			type.Received(1).GetMethods(includePrivateMethods: true);
		}

		[Fact]
		public static void CallsFindImplWhenMethodsAreFoundOnType()
		{
			var framework = Substitute.For<TestableXunitTestFrameworkDiscoverer>();
			var type = Substitute.For<_ITypeInfo>();
			framework.Assembly.GetType("abc").Returns(type);

			framework.Find("abc");
			framework.Sink.Finished.WaitOne();

			_ = framework.Received(1).FindTestsForClass(Arg.Is<_ITestClass>(testClass => testClass.Class == type));
		}

		[Fact]
		public static void ExcludesAbstractTypesFromDiscovery()
		{
			var framework = Substitute.For<TestableXunitTestFrameworkDiscoverer>();
			var type = Substitute.For<_ITypeInfo>();
			type.IsAbstract.Returns(true);
			framework.Assembly.GetType("abc").Returns(type);

			framework.Find("abc");
			framework.Sink.Finished.WaitOne();

			_ = framework.Received(0).FindTestsForClass(Arg.Is<_ITestClass>(testClass => testClass.Class == type));
		}

		[Fact]
		public static void DoesNotCallSourceProviderWhenNotAskedFor()
		{
			var sourceProvider = Substitute.For<_ISourceInformationProvider>();
			var framework = TestableXunitTestFrameworkDiscoverer.Create(sourceProvider: sourceProvider);

			framework.Find("abc");

			sourceProvider.Received(0).GetSourceInformation(Arg.Any<string?>(), Arg.Any<string?>());
		}

		[Fact]
		public static void SendsDiscoveryStartingMessage()
		{
			var typeInfo = Reflector.Wrap(typeof(ClassWithSingleTest));
			var mockAssembly = Mocks.AssemblyInfo(types: new[] { typeInfo });
			var framework = TestableXunitTestFrameworkDiscoverer.Create(mockAssembly);

			framework.Find("abc");
			framework.Sink.Finished.WaitOne();

			Assert.True(framework.Sink.StartSeen);
		}
	}

	public class FindImpl
	{
		class ClassWithNoTests
		{
			public static void NonTestMethod() { }
		}

		[Fact]
		public static async void ClassWithNoTests_ReturnsNoTestCases()
		{
			var framework = TestableXunitTestFrameworkDiscoverer.Create();
			var testClass = new TestClass(Mocks.TestCollection(), Reflector.Wrap(typeof(ClassWithNoTests)));

			await framework.FindTestsForClass(testClass);

			Assert.False(framework.Sink.Finished.WaitOne(0));
		}

		class ClassWithOneFact
		{
			[Fact]
			public static void TestMethod() { }
		}

		[Fact]
		public static async void AssemblyWithFact_ReturnsOneTestCase()
		{
			var framework = TestableXunitTestFrameworkDiscoverer.Create();
			var testClass = new TestClass(Mocks.TestCollection(), Reflector.Wrap(typeof(ClassWithOneFact)));

			await framework.FindTestsForClass(testClass);

			var testCase = Assert.Single(framework.Sink.TestCases);
			Assert.NotNull(testCase);
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
		public static async void AssemblyWithMixOfFactsAndNonTests_ReturnsTestCasesOnlyForFacts()
		{
			var framework = TestableXunitTestFrameworkDiscoverer.Create();
			var testClass = new TestClass(Mocks.TestCollection(), Reflector.Wrap(typeof(ClassWithMixOfFactsAndNonFacts)));

			await framework.FindTestsForClass(testClass);

			Assert.Equal(2, framework.Sink.TestCases.Count);
			Assert.Single(framework.Sink.TestCases, t => t.TestCaseDisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+ClassWithMixOfFactsAndNonFacts.TestMethod1");
			Assert.Single(framework.Sink.TestCases, t => t.TestCaseDisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+ClassWithMixOfFactsAndNonFacts.TestMethod2");
		}

		class TheoryWithInlineData
		{
			[Theory]
			[InlineData("Hello world")]
			[InlineData(42)]
			public static void TheoryMethod(object value) { }
		}

		[Fact]
		public static async void AssemblyWithTheoryWithInlineData_ReturnsOneTestCasePerDataRecord()
		{
			var framework = TestableXunitTestFrameworkDiscoverer.Create();
			var testClass = Mocks.TestClass<TheoryWithInlineData>();

			await framework.FindTestsForClass(testClass);

			Assert.Equal(2, framework.Sink.TestCases.Count);
			Assert.Single(framework.Sink.TestCases, t => t.TestCaseDisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithInlineData.TheoryMethod(value: \"Hello world\")");
			Assert.Single(framework.Sink.TestCases, t => t.TestCaseDisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithInlineData.TheoryMethod(value: 42)");
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
		public static async void AssemblyWithTheoryWithPropertyData_ReturnsOneTestCasePerDataRecord()
		{
			var framework = TestableXunitTestFrameworkDiscoverer.Create();
			var testClass = Mocks.TestClass<TheoryWithPropertyData>();

			await framework.FindTestsForClass(testClass);

			Assert.Equal(2, framework.Sink.TestCases.Count);
			Assert.Single(framework.Sink.TestCases, testCase => testCase.TestCaseDisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithPropertyData.TheoryMethod(value: 42)");
			Assert.Single(framework.Sink.TestCases, testCase => testCase.TestCaseDisplayName == "XunitTestFrameworkDiscovererTests+FindImpl+TheoryWithPropertyData.TheoryMethod(value: 2112)");
		}

		[Fact]
		public static async void AssemblyWithMultiLevelHierarchyWithFactOverridenInNonImmediateDerivedClass_ReturnsOneTestCase()
		{
			var framework = TestableXunitTestFrameworkDiscoverer.Create();
			var testClass = Mocks.TestClass<Child>();

			await framework.FindTestsForClass(testClass);

			Assert.Equal(1, framework.Sink.TestCases.Count);
			Assert.Equal("XunitTestFrameworkDiscovererTests+FindImpl+Child.FactOverridenInNonImmediateDerivedClass", framework.Sink.TestCases[0].TestCaseDisplayName);
		}

		public abstract class GrandParent
		{
			[Fact]
			public virtual void FactOverridenInNonImmediateDerivedClass()
			{
				Assert.True(true);
			}
		}

		public abstract class Parent : GrandParent { }

		public class Child : Parent
		{
			public override void FactOverridenInNonImmediateDerivedClass()
			{
				base.FactOverridenInNonImmediateDerivedClass();

				Assert.False(false);
			}
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
		public static async void DefaultTestCollection()
		{
			var framework = TestableXunitTestFrameworkDiscoverer.Create();
			var type = Reflector.Wrap(typeof(ClassWithNoCollection));

			var testClass = await framework.CreateTestClass(type);

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
		public static async void UndeclaredTestCollection()
		{
			var framework = TestableXunitTestFrameworkDiscoverer.Create();
			var type = Reflector.Wrap(typeof(ClassWithUndeclaredCollection));

			var testClass = await framework.CreateTestClass(type);

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
		public static async void DefinedTestCollection()
		{
			var type = Reflector.Wrap(typeof(ClassWithDefinedCollection));
			var framework = TestableXunitTestFrameworkDiscoverer.Create(type.Assembly);

			var testClass = await framework.CreateTestClass(type);

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

	public class ReportDiscoveredTestCase
	{
		TestableXunitTestFrameworkDiscoverer framework;
		SpyMessageBus messageBus;

		public ReportDiscoveredTestCase()
		{
			messageBus = new SpyMessageBus();

			var sourceProvider = Substitute.For<_ISourceInformationProvider>();
			sourceProvider
				.GetSourceInformation(null, null)
				.ReturnsForAnyArgs(new _SourceInformation { FileName = "Source File", LineNumber = 42 });

			framework = TestableXunitTestFrameworkDiscoverer.Create(sourceProvider: sourceProvider);
		}

		[Fact]
		public void CallsSourceProviderWhenTestCaseSourceInformationIsMissing()
		{
			var testCase = Mocks.TestCase<ClassWithSingleTest>(nameof(ClassWithSingleTest.TestMethod));

			framework.ReportDiscoveredTestCase_Public(testCase, includeSourceInformation: true, messageBus);

			var msg = Assert.Single(messageBus.Messages);
			var discoveryMsg = Assert.IsAssignableFrom<_TestCaseDiscovered>(msg);
			Assert.Equal("Source File", testCase.SourceInformation?.FileName);
			Assert.Equal(42, testCase.SourceInformation?.LineNumber);
		}

		[Fact]
		public void DoesNotCallSourceProviderWhenTestCaseSourceInformationIsPresent()
		{
			var testCase = Mocks.TestCase<ClassWithSingleTest>(nameof(ClassWithSingleTest.TestMethod), fileName: "Alt Source File", lineNumber: 2112);

			framework.ReportDiscoveredTestCase_Public(testCase, includeSourceInformation: true, messageBus);

			var msg = Assert.Single(messageBus.Messages);
			var discoveryMsg = Assert.IsAssignableFrom<_TestCaseDiscovered>(msg);
			Assert.Equal("Alt Source File", testCase.SourceInformation?.FileName);
			Assert.Equal(2112, testCase.SourceInformation?.LineNumber);
		}

		[Fact]
		public void SerializationTestsForXunitTestCase()
		{
			var messageSink = SpyMessageSink.Create();
			var testMethod = Mocks.TestMethod<ClassWithSingleTest>(nameof(ClassWithSingleTest.TestMethod));
			var testCase = new XunitTestCase(messageSink, TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, testMethod);

			framework.ReportDiscoveredTestCase_Public(testCase, includeSourceInformation: true, messageBus);

			var msg = Assert.Single(messageBus.Messages);
			var discoveryMsg = Assert.IsAssignableFrom<_TestCaseDiscovered>(msg);
			Assert.Equal(":F:XunitTestFrameworkDiscovererTests+ClassWithSingleTest:TestMethod:1:0:0:(null)", discoveryMsg.Serialization);
		}
	}

	public class SerializationAcceptanceTests
	{
		[Fact]
		public static void TheoriesWithSerializableData_ReturnAsIndividualTestCases()
		{
			var assemblyInfo = Reflector.Wrap(Assembly.GetExecutingAssembly());
			var discoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, configFileName: null, _NullSourceInformationProvider.Instance, SpyMessageSink.Create());
			var sink = new TestDiscoverySink();

			discoverer.Find(typeof(ClassWithTheory).FullName!, sink, _TestFrameworkOptions.ForDiscovery(preEnumerateTheories: true));
			sink.Finished.WaitOne();

			Assert.Collection(
				sink.TestCases.OrderBy(tc => tc.TestCaseDisplayName),
				testCase => Assert.Equal("XunitTestFrameworkDiscovererTests+SerializationAcceptanceTests+ClassWithTheory.Test(x: \"hello\")", testCase.TestCaseDisplayName),
				testCase => Assert.Equal("XunitTestFrameworkDiscovererTests+SerializationAcceptanceTests+ClassWithTheory.Test(x: 1)", testCase.TestCaseDisplayName)
			);

			var first = sink.TestCases[0];
			var second = sink.TestCases[1];
			Assert.NotEqual(first.TestCaseUniqueID, second.TestCaseUniqueID);
		}

		class ClassWithTheory
		{
			[Theory]
			[InlineData(1)]
			[InlineData("hello")]
			public void Test(object x) { }
		}

		[Fact]
		public static void TheoryWithNonSerializableData_ReturnsAsASingleTestCase()
		{
			var assemblyInfo = Reflector.Wrap(Assembly.GetExecutingAssembly());
			var discoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, configFileName: null, _NullSourceInformationProvider.Instance, SpyMessageSink.Create());
			var sink = new TestDiscoverySink();

			discoverer.Find(typeof(ClassWithNonSerializableTheoryData).FullName!, sink, _TestFrameworkOptions.ForDiscovery());
			sink.Finished.WaitOne();

			Assert.Single(sink.TestCases);
		}

		class NonSerializableData { }

		class ClassWithNonSerializableTheoryData
		{
			public static IEnumerable<object[]> Data = new[] { new[] { new NonSerializableData() }, new[] { new object() } };

			[Theory]
			[MemberData("Data")]
			public void Test(object x) { }
		}
	}

	public class TestableXunitTestFrameworkDiscoverer : XunitTestFrameworkDiscoverer
	{
		protected TestableXunitTestFrameworkDiscoverer()
			: this(Mocks.AssemblyInfo()) { }

		protected TestableXunitTestFrameworkDiscoverer(_IAssemblyInfo assembly)
			: this(assembly, null, null, null) { }

		protected TestableXunitTestFrameworkDiscoverer(
			_IAssemblyInfo assembly,
			_ISourceInformationProvider? sourceProvider,
			_IMessageSink? diagnosticMessageSink,
			IXunitTestCollectionFactory? collectionFactory)
				: base(assembly, configFileName: null, sourceProvider ?? Substitute.For<_ISourceInformationProvider>(), diagnosticMessageSink ?? new _NullMessageSink(), collectionFactory)
		{
			Assembly = assembly;
			Sink = new TestableTestDiscoverySink();
		}

		public _IAssemblyInfo Assembly { get; private set; }

		public override sealed string TestAssemblyUniqueID => "asm-id";

		public List<_TestCaseDiscovered> TestCases
		{
			get
			{
				Sink.Finished.WaitOne();
				return Sink.TestCases;
			}
		}

		internal TestableTestDiscoverySink Sink { get; private set; }

		public static TestableXunitTestFrameworkDiscoverer Create(
			_IAssemblyInfo? assembly = null,
			_ISourceInformationProvider? sourceProvider = null,
			_IMessageSink? diagnosticMessageSink = null,
			IXunitTestCollectionFactory? collectionFactory = null)
		{
			return new TestableXunitTestFrameworkDiscoverer(assembly ?? Mocks.AssemblyInfo(), sourceProvider, diagnosticMessageSink, collectionFactory);
		}

		public new ValueTask<_ITestClass> CreateTestClass(_ITypeInfo @class) =>
			base.CreateTestClass(@class);

		public void Find()
		{
			Find(Sink, _TestFrameworkOptions.ForDiscovery(preEnumerateTheories: true));
			Sink.Finished.WaitOne();
		}

		public void Find(string typeName)
		{
			Find(typeName, Sink, _TestFrameworkOptions.ForDiscovery(preEnumerateTheories: true));
			Sink.Finished.WaitOne();
		}

		public virtual async ValueTask<bool> FindTestsForClass(_ITestClass testClass)
		{
			using var messageBus = new MessageBus(Sink);
			return await base.FindTestsForType(testClass, messageBus, _TestFrameworkOptions.ForDiscovery(preEnumerateTheories: true));
		}

		protected sealed override ValueTask<bool> FindTestsForType(
			_ITestClass testClass,
			IMessageBus messageBus,
			_ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			return FindTestsForClass(testClass);
		}

		protected sealed override bool IsValidTestClass(_ITypeInfo type)
		{
			return base.IsValidTestClass(type);
		}

		public ValueTask<bool> ReportDiscoveredTestCase_Public(
			_ITestCase testCase,
			bool includeSourceInformation,
			IMessageBus messageBus) =>
				ReportDiscoveredTestCase(testCase, includeSourceInformation, messageBus);

		protected override ValueTask<string> Serialize(_ITestCase testCase)
		{
			try
			{
				return base.Serialize(testCase);
			}
			catch (SerializationException)
			{
				// Mocks can't be serialized, so fall back if anything in the chain isn't serializable
				return new ValueTask<string>($"Serialization of test case '{testCase.DisplayName}'");
			}
		}
	}

	internal class TestableTestDiscoverySink : TestDiscoverySink
	{
		public bool StartSeen = false;

		public TestableTestDiscoverySink(Func<bool>? cancelThunk = null)
			: base(cancelThunk)
		{
			DiscoverySink.DiscoveryStartingEvent += args => StartSeen = true;
		}
	}
}
