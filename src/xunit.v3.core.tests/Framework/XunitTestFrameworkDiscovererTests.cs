using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestFrameworkDiscovererTests
{
	public class Construction
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("testAssembly", () => new XunitTestFrameworkDiscoverer(testAssembly: null!));
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

			var testClass = await discoverer.CreateTestClass(typeof(ClassWithNoCollection));

			Assert.NotNull(testClass.TestCollection);
#if BUILD_X86 && NETFRAMEWORK
			Assert.Equal("Test collection for XunitTestFrameworkDiscovererTests+CreateTestClass+ClassWithNoCollection (id: c6b93139464209e6f292e5c9beef80dd927a4b15a9bc5c25c799dba723f446c2)", testClass.TestCollection.TestCollectionDisplayName);
#elif BUILD_X86
			Assert.Equal("Test collection for XunitTestFrameworkDiscovererTests+CreateTestClass+ClassWithNoCollection (id: 909128fb37fc94f8968895780ad64cc07b350be11b06686e0353f576df69cf04)", testClass.TestCollection.TestCollectionDisplayName);
#elif NETFRAMEWORK
			Assert.Equal("Test collection for XunitTestFrameworkDiscovererTests+CreateTestClass+ClassWithNoCollection (id: ecfe652d9ecdedba356c64ae79857c8a48d7ace73b61d81a1ffda867882d5003)", testClass.TestCollection.TestCollectionDisplayName);
#else
			Assert.Equal("Test collection for XunitTestFrameworkDiscovererTests+CreateTestClass+ClassWithNoCollection (id: 908f91c64bf37155a8a298dc21327c01663c8f8936d324803da1f80b3029917f)", testClass.TestCollection.TestCollectionDisplayName);
#endif
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

			var testClass = await discoverer.CreateTestClass(typeof(ClassWithUndeclaredCollection));

			Assert.NotNull(testClass.TestCollection);
			Assert.Equal("This a collection without declaration", testClass.TestCollection.TestCollectionDisplayName);
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
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			var testClass = await discoverer.CreateTestClass(typeof(ClassWithDefinedCollection));

			Assert.NotNull(testClass.TestCollection);
			Assert.Equal("This a defined collection", testClass.TestCollection.TestCollectionDisplayName);
			Assert.NotNull(testClass.TestCollection.CollectionDefinition);
			Assert.Equal("XunitTestFrameworkDiscovererTests+CreateTestClass+DeclaredCollection", testClass.TestCollection.CollectionDefinition.SafeName());
		}
	}

	public class FindTestsForType
	{
		[Fact]
		public static async ValueTask CanFindNonPublicTestMethods()
		{
			var testClass = TestData.XunitTestClass<ClassWithPrivateTestMethod>();
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			await discoverer.FindTestsForType(testClass);

			var testCase = Assert.Single(discoverer.FindTestsForType_TestCases);
			Assert.Equal($"{typeof(ClassWithPrivateTestMethod).FullName}.{nameof(ClassWithPrivateTestMethod.TestMethod)}", testCase.TestCaseDisplayName);
		}

#pragma warning disable xUnit1003 // Theory methods must have test data
#pragma warning disable xUnit1006 // Theory methods should have parameters

		class ClassWithPrivateTestMethod()
		{
			[Theory]
			internal void TestMethod() { }
		}

#pragma warning restore xUnit1006 // Theory methods should have parameters
#pragma warning restore xUnit1003 // Theory methods must have test data

		[Fact]
		public static async ValueTask TestMethodWithTooManyFactAttributes_ReturnsExecutionErrorTestCase()
		{
			var testClass = TestData.XunitTestClass<ClassWithTooManyFactAttributesOnTestMethod>();
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
			var testClass = TestData.XunitTestClass<ClassWithNoTests>();
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			await discoverer.FindTestsForType(testClass);

			Assert.Empty(discoverer.FindTestsForType_TestCases);
		}

#pragma warning disable CA1822 // Mark members as static

		class ClassWithNoTests
		{
			public void TestMethod() { }
		}

#pragma warning restore CA1822 // Mark members as static

		[Fact]
		public static async ValueTask DiscoversFactDecoratedTestMethod()
		{
			var testClass = TestData.XunitTestClass<ClassWithOneTest>();
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
			var testClass = TestData.XunitTestClass<TheoryWithInlineData>();
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions(preEnumerateTheories: true);

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
			var testClass = TestData.XunitTestClass<TheoryWithInlineData>();
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions(preEnumerateTheories: false);

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
			var testClass = TestData.XunitTestClass<Child>();
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

		[Fact]
		public static async ValueTask DiscoversBaseStaticMethodDecoratedWithFact()
		{
			var testClass = TestData.XunitTestClass<ClassWithInheritedStaticMethodUnderTest>();
			var discoverer = TestableXunitTestFrameworkDiscoverer.Create();

			await discoverer.FindTestsForType(testClass);

			var testCase = Assert.Single(discoverer.FindTestsForType_TestCases);
			Assert.IsType<XunitTestCase>(testCase);
			Assert.Equal($"{typeof(ClassWithInheritedStaticMethodUnderTest).FullName}.{nameof(ClassWithInheritedStaticMethodUnderTest.Passing)}", testCase.TestCaseDisplayName);
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
			var testAssembly = Mocks.XunitTestAssembly(collectionBehavior: behaviorAttribute);

			var discoverer = TestableXunitTestFrameworkDiscoverer.Create(testAssembly: testAssembly);

			Assert.IsType(expectedFactoryType, discoverer.TestCollectionFactory);
		}

		[Fact]
		public static void ValidCustomFactory()
		{
			var behaviorAttribute = Mocks.CollectionBehaviorAttribute(typeof(CustomTestCollectionFactory));
			var testAssembly = Mocks.XunitTestAssembly(collectionBehavior: behaviorAttribute);

			var discoverer = TestableXunitTestFrameworkDiscoverer.Create(testAssembly);

			Assert.IsType<CustomTestCollectionFactory>(discoverer.TestCollectionFactory);
		}

		[Fact]
		public static void InvalidCustomFactoryFallsBackToDefault()
		{
			var spyMessageSink = SpyMessageSink.Capture();
			TestContextInternal.Current.DiagnosticMessageSink = spyMessageSink;
			var behaviorAttribute = Mocks.CollectionBehaviorAttribute(typeof(object));
			var testAssembly = Mocks.XunitTestAssembly(collectionBehavior: behaviorAttribute);

			var discoverer = TestableXunitTestFrameworkDiscoverer.Create(testAssembly);

			Assert.IsType<CollectionPerClassTestCollectionFactory>(discoverer.TestCollectionFactory);
			var message = Assert.Single(spyMessageSink.Messages);
			var diagMessage = Assert.IsType<IDiagnosticMessage>(message, exactMatch: false);
			Assert.Equal("Test collection factory type 'System.Object' does not implement IXunitTestCollectionFactory", diagMessage.Message);
		}
	}

	class ClassWithSingleTest
	{
		[Fact]
		public static void TestMethod() { }
	}

	class CustomTestCollectionFactory(IXunitTestAssembly _) : IXunitTestCollectionFactory
	{
		public string DisplayName => "my-custom-test-collection-factory";

		public IXunitTestCollection Get(Type testClass) => throw new NotImplementedException();
	}

	class TestableXunitTestFrameworkDiscoverer : XunitTestFrameworkDiscoverer
	{
		public List<ITestCase> FindTestsForType_TestCases = [];

		TestableXunitTestFrameworkDiscoverer(
			IXunitTestAssembly testAssembly,
			IXunitTestCollectionFactory? collectionFactory) :
				base(testAssembly, collectionFactory)
		{ }

		public new ValueTask<IXunitTestClass> CreateTestClass(Type @class) =>
			base.CreateTestClass(@class);

		public ValueTask<bool> FindTestsForType(
			IXunitTestClass testClass,
			ITestFrameworkDiscoveryOptions? discoveryOptions = null) =>
				base.FindTestsForType(
					testClass,
					discoveryOptions ?? TestData.TestFrameworkDiscoveryOptions(preEnumerateTheories: true),
					testCase =>
					{
						FindTestsForType_TestCases.Add(testCase);
						return new(true);
					}
				);

		protected sealed override bool IsValidTestClass(Type type) =>
			base.IsValidTestClass(type);

		public static TestableXunitTestFrameworkDiscoverer Create(
			IXunitTestAssembly? testAssembly = null,
			IXunitTestCollectionFactory? collectionFactory = null) =>
				new(
					testAssembly ?? TestData.XunitTestAssembly(typeof(XunitTestFrameworkDiscovererTests).Assembly),
					collectionFactory
				);
	}
}
