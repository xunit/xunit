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
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("assemblyInfo", () => new XunitTestFrameworkDiscoverer(assemblyInfo: null!, configFileName: null));
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
		public static async ValueTask DefaultTestCollection()
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
		public static async ValueTask UndeclaredTestCollection()
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
		public static async ValueTask DefinedTestCollection()
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

#pragma warning disable xUnit1002 // Test methods cannot have multiple Fact or Theory attributes
#pragma warning disable xUnit1003 // Theory methods must have test data
#pragma warning disable xUnit1006 // Theory methods should have parameters
		class ClassWithTooManyFactAttributesOnTestMethod
		{
			[Fact]
			[Theory]
			public void TestMethod() { }
		}
#pragma warning restore xUnit1006 // Theory methods should have parameters
#pragma warning restore xUnit1003 // Theory methods must have test data
#pragma warning restore xUnit1002 // Test methods cannot have multiple Fact or Theory attributes

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
		public static async ValueTask Theory_WithPreEnumeration_ReturnsOneTestCasePerDataRecord()
		{
			var testClass = Mocks.TestClass<TheoryWithInlineData>();
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(preEnumerateTheories: true);

			await discoverer.FindTestsForType(testClass, discoveryOptions);

			Assert.Collection(
				discoverer.FindTestsForType_TestCases.Select(t => t.TestCaseDisplayName).OrderBy(x => x),
				displayName => Assert.Equal($"{typeof(TheoryWithInlineData).FullName}.{nameof(TheoryWithInlineData.TheoryMethod)}(_: \"Hello world\")", displayName),
				displayName => Assert.Equal($"{typeof(TheoryWithInlineData).FullName}.{nameof(TheoryWithInlineData.TheoryMethod)}(_: 42)", displayName)
			);
		}

		[Fact]
		public static async ValueTask Theory_WithoutPreEnumeration_ReturnsOneTestCase()
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
			public static void TheoryMethod(object _) { }
		}

		[Fact]
		public static async ValueTask AssemblyWithMultiLevelHierarchyWithFactOverridenInNonImmediateDerivedClass_ReturnsOneTestCase()
		{
			var testClass = Mocks.TestClass<Child>();
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			await discoverer.FindTestsForType(testClass);

			var testCase = Assert.Single(discoverer.FindTestsForType_TestCases);
			Assert.Equal($"{typeof(Child).FullName}.{nameof(GrandParent.FactOverridenInNonImmediateDerivedClass)}", testCase.TestCaseDisplayName);
		}

		[Fact]
		public static async ValueTask DiscoversBaseStaticMethodDecoratedWithFact()
		{
			var testClass = Mocks.TestClass<ClassWithInheritedStaticMethodUnderTest>();
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			await discoverer.FindTestsForType(testClass);

			var testCase = Assert.Single(discoverer.FindTestsForType_TestCases);
			Assert.IsType<XunitTestCase>(testCase);
			Assert.Equal($"{typeof(ClassWithInheritedStaticMethodUnderTest).FullName}.{nameof(ClassWithInheritedStaticMethodUnderTest.Passing)}", testCase.TestCaseDisplayName);
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

		public abstract class BaseClassWithStaticMethodUnderTest
		{
			[Fact]
			public static void Passing() { }
		}

		public class ClassWithInheritedStaticMethodUnderTest : BaseClassWithStaticMethodUnderTest
		{ }
	}

	public static class TestCollectionFactory
	{
		[Fact]
		public static void DefaultTestCollectionFactory()
		{
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			Assert.IsType<CollectionPerClassTestCollectionFactory>(discoverer.TestCollectionFactory);
		}

		[Theory]
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
			var spyMessageSink = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spyMessageSink;
			var behaviorAttribute = Mocks.CollectionBehaviorAttribute<object>();
			var assembly = Mocks.AssemblyInfo(attributes: new[] { behaviorAttribute });

			var discoverer = TestableXunitTestFrameworkDiscoverer.Create(assembly);

			Assert.IsType<CollectionPerClassTestCollectionFactory>(discoverer.TestCollectionFactory);
			var message = Assert.Single(spyMessageSink.Messages);
			var diagMessage = Assert.IsType<_DiagnosticMessage>(message);
			Assert.Equal("Test collection factory type 'System.Object' does not implement IXunitTestCollectionFactory", diagMessage.Message);
		}
	}

	public static class TestFrameworkDisplayName
	{
		[Fact]
		public static void Defaults()
		{
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			Assert.Matches(@"xUnit.net v3 \d+\.\d+\.\d+(-pre\.\d+(-dev)?(\+[0-9a-f]+)?)? \[collection-per-class, parallel\]", discoverer.TestFrameworkDisplayName);
		}

		[Fact]
		public static void CollectionPerAssembly()
		{
			var behaviorAttribute = Mocks.CollectionBehaviorAttribute(CollectionBehavior.CollectionPerAssembly);
			var assembly = Mocks.AssemblyInfo(attributes: new[] { behaviorAttribute });

			var discoverer = TestableXunitTestFrameworkDiscoverer.Create(assembly);

			Assert.Matches(@"xUnit.net v3 \d+\.\d+\.\d+(-pre\.\d+(-dev)?(\+[0-9a-f]+)?)? \[collection-per-assembly, parallel\]", discoverer.TestFrameworkDisplayName);
		}

		[Fact]
		public static void CustomCollectionFactory()
		{
			var behaviorAttribute = Mocks.CollectionBehaviorAttribute<CustomTestCollectionFactory>();
			var assembly = Mocks.AssemblyInfo(attributes: new[] { behaviorAttribute });

			var discoverer = TestableXunitTestFrameworkDiscoverer.Create(assembly);

			Assert.Matches(@"xUnit.net v3 \d+\.\d+\.\d+(-pre\.\d+(-dev)?(\+[0-9a-f]+)?)? \[my-custom-test-collection-factory, parallel\]", discoverer.TestFrameworkDisplayName);
		}

		[Fact]
		public static void NonParallel()
		{
			var behaviorAttribute = Mocks.CollectionBehaviorAttribute(disableTestParallelization: true);
			var assembly = Mocks.AssemblyInfo(attributes: new[] { behaviorAttribute });

			var discoverer = TestableXunitTestFrameworkDiscoverer.Create(assembly);

			Assert.Matches(@"xUnit.net v3 \d+\.\d+\.\d+(-pre\.\d+(-dev)?(\+[0-9a-f]+)?)? \[collection-per-class, non-parallel\]", discoverer.TestFrameworkDisplayName);
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
			_IAssemblyInfo assemblyInfo,
			IXunitTestCollectionFactory? collectionFactory)
				: base(assemblyInfo, configFileName: null, collectionFactory)
		{
			TestAssembly = Mocks.TestAssembly(assemblyInfo.AssemblyPath, uniqueID: "asm-id");
		}

		public new _IAssemblyInfo AssemblyInfo => base.AssemblyInfo;

		public override _ITestAssembly TestAssembly { get; }

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
			IXunitTestCollectionFactory? collectionFactory = null) =>
				new(assembly ?? Mocks.AssemblyInfo(), collectionFactory);
	}
}
