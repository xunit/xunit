using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestFrameworkDiscovererTests
{
	public class Construction
	{
		[Fact]
		public static void GuardClauses()
		{
			var assembly = Substitute.For<_IAssemblyInfo>();
			var diagnosticMessageSink = SpyMessageSink.Create();

			Assert.Throws<ArgumentNullException>("assemblyInfo", () => new XunitTestFrameworkDiscoverer(assemblyInfo: null!, configFileName: null, diagnosticMessageSink));
			Assert.Throws<ArgumentNullException>("diagnosticMessageSink", () => new XunitTestFrameworkDiscoverer(assembly, configFileName: null, diagnosticMessageSink: null!));
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
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();
			var type = Reflector.Wrap(typeof(ClassWithNoCollection));

			var testClass = await discoverer.CreateTestClass(type);

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
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();
			var type = Reflector.Wrap(typeof(ClassWithUndeclaredCollection));

			var testClass = await discoverer.CreateTestClass(type);

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
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create(type.Assembly);

			var testClass = await discoverer.CreateTestClass(type);

			Assert.NotNull(testClass.TestCollection);
			Assert.Equal("This a defined collection", testClass.TestCollection.DisplayName);
			Assert.NotNull(testClass.TestCollection.CollectionDefinition);
			Assert.Equal("XunitTestFrameworkDiscovererTests+CreateTestClass+DeclaredCollection", testClass.TestCollection.CollectionDefinition.Name);
		}
	}

	public class FindTestsForType
	{
		[Fact]
		public static async ValueTask RequestsPublicAndPrivateMethodsFromType()
		{
			var typeInfo = Mocks.TypeInfo();
			var testClass = Mocks.TestClass(typeInfo);
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			await discoverer.FindTestsForType(testClass);

			typeInfo.Received(1).GetMethods(includePrivateMethods: true);
		}

		[Fact]
		public static async ValueTask TestMethodWithTooManyFactAttributes_ReturnsExecutionErrorTestCase()
		{
			var testClass = Mocks.TestClass<ClassWithTooManyFactAttributesOnTestMethod>();
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			await discoverer.FindTestsForType(testClass);

			var testCase = Assert.Single(discoverer.FindTestsForType_TestCases);
			var errorTestCase = Assert.IsType<ExecutionErrorTestCase>(testCase);
			Assert.Equal($"Test method '{typeof(ClassWithTooManyFactAttributesOnTestMethod).FullName}.{nameof(ClassWithTooManyFactAttributesOnTestMethod.TestMethod)}' has multiple [Fact]-derived attributes", errorTestCase.ErrorMessage);
		}

		class ClassWithTooManyFactAttributesOnTestMethod
		{
			[Fact]
			[Theory]
			public void TestMethod() { }
		}

		[Fact]
		public static async ValueTask DoesNotDiscoverNonFactDecoratedTestMethod()
		{
			var testClass = Mocks.TestClass<ClassWithNoTests>();
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			await discoverer.FindTestsForType(testClass);

			Assert.Empty(discoverer.FindTestsForType_TestCases);
		}

		class ClassWithNoTests
		{
			public void TestMethod() { }
		}

		[Fact]
		public static async ValueTask DiscoversFactDecoratedTestMethod()
		{
			var testClass = Mocks.TestClass<ClassWithOneTest>();
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			await discoverer.FindTestsForType(testClass);

			var testCase = Assert.Single(discoverer.FindTestsForType_TestCases);
			Assert.IsType<XunitTestCase>(testCase);
			Assert.Equal($"{typeof(ClassWithOneTest).FullName}.{nameof(ClassWithOneTest.TestMethod)}", testCase.TestCaseDisplayName);
		}

		class ClassWithOneTest
		{
			[Fact]
			public void TestMethod() { }
		}

		[Fact]
		public static async void Theory_WithPreEnumeration_ReturnsOneTestCasePerDataRecord()
		{
			var testClass = Mocks.TestClass<TheoryWithInlineData>();
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(preEnumerateTheories: true);

			await discoverer.FindTestsForType(testClass, discoveryOptions);

			Assert.Collection(
				discoverer.FindTestsForType_TestCases.Select(t => t.TestCaseDisplayName).OrderBy(x => x),
				displayName => Assert.Equal($"{typeof(TheoryWithInlineData).FullName}.{nameof(TheoryWithInlineData.TheoryMethod)}(value: \"Hello world\")", displayName),
				displayName => Assert.Equal($"{typeof(TheoryWithInlineData).FullName}.{nameof(TheoryWithInlineData.TheoryMethod)}(value: 42)", displayName)
			);
		}

		[Fact]
		public static async void Theory_WithoutPreEnumeration_ReturnsOneTestCase()
		{
			var testClass = Mocks.TestClass<TheoryWithInlineData>();
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(preEnumerateTheories: false);

			await discoverer.FindTestsForType(testClass, discoveryOptions);

			var testCase = Assert.Single(discoverer.FindTestsForType_TestCases);
			Assert.Equal($"{typeof(TheoryWithInlineData).FullName}.{nameof(TheoryWithInlineData.TheoryMethod)}", testCase.TestCaseDisplayName);
		}

		class TheoryWithInlineData
		{
			[Theory]
			[InlineData("Hello world")]
			[InlineData(42)]
			public static void TheoryMethod(object value) { }
		}

		[Fact]
		public static async void AssemblyWithMultiLevelHierarchyWithFactOverridenInNonImmediateDerivedClass_ReturnsOneTestCase()
		{
			var testClass = Mocks.TestClass<Child>();
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			await discoverer.FindTestsForType(testClass);

			var testCase = Assert.Single(discoverer.FindTestsForType_TestCases);
			Assert.Equal($"{typeof(Child).FullName}.{nameof(GrandParent.FactOverridenInNonImmediateDerivedClass)}", testCase.TestCaseDisplayName);
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

	public static class TestCollectionFactory
	{
		[Fact]
		public static void DefaultTestCollectionFactory()
		{
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			Assert.IsType<CollectionPerClassTestCollectionFactory>(discoverer.TestCollectionFactory);
		}

		[Theory(DisableDiscoveryEnumeration = true)]
		[InlineData(CollectionBehavior.CollectionPerAssembly, typeof(CollectionPerAssemblyTestCollectionFactory))]
		[InlineData(CollectionBehavior.CollectionPerClass, typeof(CollectionPerClassTestCollectionFactory))]
		public static void AssemblyAttributeOverride(
			CollectionBehavior behavior,
			Type expectedFactoryType)
		{
			var behaviorAttribute = Mocks.CollectionBehaviorAttribute(behavior);
			var assembly = Mocks.AssemblyInfo(attributes: new[] { behaviorAttribute });

			var discoverer = TestableXunitTestFrameworkDiscoverer.Create(assembly);

			Assert.IsType(expectedFactoryType, discoverer.TestCollectionFactory);
		}

		[Fact]
		public static void ValidCustomFactory()
		{
			var behaviorAttribute = Mocks.CollectionBehaviorAttribute<CustomTestCollectionFactory>();
			var assembly = Mocks.AssemblyInfo(attributes: new[] { behaviorAttribute });

			var discoverer = TestableXunitTestFrameworkDiscoverer.Create(assembly);

			Assert.IsType<CustomTestCollectionFactory>(discoverer.TestCollectionFactory);
		}

		[Fact]
		public static void InvalidCustomFactoryFallsBackToDefault()
		{
			var behaviorAttribute = Mocks.CollectionBehaviorAttribute<object>();
			var assembly = Mocks.AssemblyInfo(attributes: new[] { behaviorAttribute });

			var discoverer = TestableXunitTestFrameworkDiscoverer.Create(assembly);

			Assert.IsType<CollectionPerClassTestCollectionFactory>(discoverer.TestCollectionFactory);
		}
	}

	public static class TestFrameworkDisplayName
	{
		[Fact]
		public static void Defaults()
		{
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			Assert.Matches(@"xUnit.net v3 \d+\.\d+\.\d+(-pre\.\d+(\+[0-9a-f]+)?)? \[collection-per-class, parallel\]", discoverer.TestFrameworkDisplayName);
		}

		[Fact]
		public static void CollectionPerAssembly()
		{
			var behaviorAttribute = Mocks.CollectionBehaviorAttribute(CollectionBehavior.CollectionPerAssembly);
			var assembly = Mocks.AssemblyInfo(attributes: new[] { behaviorAttribute });

			var discoverer = TestableXunitTestFrameworkDiscoverer.Create(assembly);

			Assert.Matches(@"xUnit.net v3 \d+\.\d+\.\d+(-pre\.\d+(\+[0-9a-f]+)?)? \[collection-per-assembly, parallel\]", discoverer.TestFrameworkDisplayName);
		}

		[Fact]
		public static void CustomCollectionFactory()
		{
			var behaviorAttribute = Mocks.CollectionBehaviorAttribute<CustomTestCollectionFactory>();
			var assembly = Mocks.AssemblyInfo(attributes: new[] { behaviorAttribute });

			var discoverer = TestableXunitTestFrameworkDiscoverer.Create(assembly);

			Assert.Matches(@"xUnit.net v3 \d+\.\d+\.\d+(-pre\.\d+(\+[0-9a-f]+)?)? \[my-custom-test-collection-factory, parallel\]", discoverer.TestFrameworkDisplayName);
		}

		[Fact]
		public static void NonParallel()
		{
			var behaviorAttribute = Mocks.CollectionBehaviorAttribute(disableTestParallelization: true);
			var assembly = Mocks.AssemblyInfo(attributes: new[] { behaviorAttribute });

			var discoverer = TestableXunitTestFrameworkDiscoverer.Create(assembly);

			Assert.Matches(@"xUnit.net v3 \d+\.\d+\.\d+(-pre\.\d+(\+[0-9a-f]+)?)? \[collection-per-class, non-parallel\]", discoverer.TestFrameworkDisplayName);
		}
	}

	class ClassWithSingleTest
	{
		[Fact]
		public static void TestMethod() { }
	}

	class CustomTestCollectionFactory : IXunitTestCollectionFactory
	{
		public CustomTestCollectionFactory(_ITestAssembly testAssembly)
		{ }

		public string DisplayName => "my-custom-test-collection-factory";

		public _ITestCollection Get(_ITypeInfo testClass) => throw new NotImplementedException();
	}

	class TestableXunitTestFrameworkDiscoverer : XunitTestFrameworkDiscoverer
	{
		public List<_ITestCaseMetadata> FindTestsForType_TestCases = new();

		TestableXunitTestFrameworkDiscoverer(
			_IAssemblyInfo assembly,
			_IMessageSink? diagnosticMessageSink,
			IXunitTestCollectionFactory? collectionFactory)
				: base(assembly, configFileName: null, diagnosticMessageSink ?? new _NullMessageSink(), collectionFactory)
		{ }

		public new _IAssemblyInfo AssemblyInfo => base.AssemblyInfo;

		public override sealed string TestAssemblyUniqueID => "asm-id";

		public new ValueTask<_ITestClass> CreateTestClass(_ITypeInfo @class) =>
			base.CreateTestClass(@class);

		public ValueTask<bool> FindTestsForType(
			_ITestClass testClass,
			_ITestFrameworkDiscoveryOptions? discoveryOptions = null) =>
				base.FindTestsForType(
					testClass,
					discoveryOptions ?? _TestFrameworkOptions.ForDiscovery(preEnumerateTheories: true),
					testCase =>
					{
						FindTestsForType_TestCases.Add(testCase);
						return new(true);
					}
				);

		protected sealed override bool IsValidTestClass(_ITypeInfo type) =>
			base.IsValidTestClass(type);

		public static TestableXunitTestFrameworkDiscoverer Create(
			_IAssemblyInfo? assembly = null,
			_IMessageSink? diagnosticMessageSink = null,
			IXunitTestCollectionFactory? collectionFactory = null) =>
				new(assembly ?? Mocks.AssemblyInfo(), diagnosticMessageSink, collectionFactory);
	}
}
