using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestCaseTests
{
	public class FactAttributeValues
	{
		[Fact]
		public static void DisplayName()
		{
			var factAttribute = Mocks.FactAttribute(displayName: "Custom Display Name");
			var testMethod = Mocks.TestMethod(methodAttributes: new[] { factAttribute });

			var testCase = new TestableXunitTestCase(testMethod);

			Assert.Equal("Custom Display Name", testCase.DisplayName);
		}

		[Fact]
		public static void DisplayNameWithArguments()
		{
			var factAttribute = Mocks.FactAttribute(displayName: "Custom Display Name");
			var param1 = Mocks.ParameterInfo("p1");
			var param2 = Mocks.ParameterInfo("p2");
			var param3 = Mocks.ParameterInfo("p3");
			var testMethod = Mocks.TestMethod(methodAttributes: new[] { factAttribute }, parameters: new[] { param1, param2, param3 });
			var arguments = new object[] { 42, "Hello, world!", 'A' };

			var testCase = new TestableXunitTestCase(testMethod, arguments);

			Assert.Equal("Custom Display Name(p1: 42, p2: \"Hello, world!\", p3: 'A')", testCase.DisplayName);
		}

		[Fact]
		public static void SkipReason()
		{
			var factAttribute = Mocks.FactAttribute(skip: "Skip Reason");
			var testMethod = Mocks.TestMethod(methodAttributes: new[] { factAttribute });

			var testCase = new TestableXunitTestCase(testMethod);

			Assert.Equal("Skip Reason", testCase.SkipReason);
		}

		[Fact]
		public static void Timeout()
		{
			var factAttribute = Mocks.FactAttribute(timeout: 42);
			var testMethod = Mocks.TestMethod(methodAttributes: new[] { factAttribute });

			var testCase = new TestableXunitTestCase(testMethod);

			Assert.Equal(42, testCase.Timeout);
		}
	}

	public class Traits : AcceptanceTestV3
	{
		[Fact]
		public static void TraitsOnTestMethod()
		{
			var trait1 = Mocks.TraitAttribute("Trait1", "Value1");
			var trait2 = Mocks.TraitAttribute("Trait2", "Value2");
			var testMethod = Mocks.TestMethod(methodAttributes: new[] { trait1, trait2 });

			var testCase = new TestableXunitTestCase(testMethod);

			Assert.Equal("Value1", Assert.Single(testCase.Traits["Trait1"]));
			Assert.Equal("Value2", Assert.Single(testCase.Traits["Trait2"]));
		}

		[Fact]
		public static void TraitsOnTestClass()
		{
			var trait1 = Mocks.TraitAttribute("Trait1", "Value1");
			var trait2 = Mocks.TraitAttribute("Trait2", "Value2");
			var testMethod = Mocks.TestMethod(classAttributes: new[] { trait1, trait2 });

			var testCase = new TestableXunitTestCase(testMethod);

			Assert.Equal("Value1", Assert.Single(testCase.Traits["Trait1"]));
			Assert.Equal("Value2", Assert.Single(testCase.Traits["Trait2"]));
		}

		[Fact]
		public async void CustomTrait()
		{
			var messages = await RunAsync(typeof(ClassWithCustomTraitTest));
			var passingTests = messages.OfType<_TestPassed>();

			var passingTest = Assert.Single(passingTests);
			var passingTestCaseStarting = messages.OfType<_TestCaseStarting>().Single(tcs => tcs.TestCaseUniqueID == passingTest.TestCaseUniqueID);
			Assert.Collection(
				passingTestCaseStarting.Traits.OrderBy(x => x.Key),
				namedTrait =>
				{
					Assert.Equal("Assembly", namedTrait.Key);
					var value = Assert.Single(namedTrait.Value);
					Assert.Equal("Trait", value);
				},
				namedTrait =>
				{
					Assert.Equal("Author", namedTrait.Key);
					var value = Assert.Single(namedTrait.Value);
					Assert.Equal("Some Schmoe", value);
				},
				namedTrait =>
				{
					Assert.Equal("Bug", namedTrait.Key);
					var value = Assert.Single(namedTrait.Value);
					Assert.Equal("2112", value);
				}
			);
		}

		[Fact]
		public static void CustomTraitWithoutDiscoverer()
		{
			var trait = Mocks.TraitAttribute<BadTraitAttribute>();
			var testMethod = Mocks.TestMethod(classAttributes: new[] { trait });
			var messages = new List<_MessageSinkMessage>();
			var spy = SpyMessageSink.Create(messages: messages);

			var testCase = new TestableXunitTestCase(testMethod, diagnosticMessageSink: spy);

			Assert.Empty(testCase.Traits);
			var diagnosticMessages = messages.OfType<_DiagnosticMessage>();
			var diagnosticMessage = Assert.Single(diagnosticMessages);
			Assert.Equal($"Trait attribute on '{testCase.DisplayName}' did not have [TraitDiscoverer]", diagnosticMessage.Message);
		}

		class BadTraitAttribute : Attribute, ITraitAttribute { }

		class ClassWithCustomTraitTest
		{
			[Fact]
			[Bug(2112)]
			[Trait("Author", "Some Schmoe")]
			public static void BugFix() { }
		}

		public class BugDiscoverer : ITraitDiscoverer
		{
			public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits(_IAttributeInfo traitAttribute)
			{
				var ctorArgs = traitAttribute.GetConstructorArguments().ToList();
				return new[] { new KeyValuePair<string, string>("Bug", ctorArgs[0]!.ToString()!) };
			}
		}

		[TraitDiscoverer(typeof(BugDiscoverer))]
		class BugAttribute : Attribute, ITraitAttribute
		{
			public BugAttribute(int id) { }
		}

		public static TheoryData<Type, IEnumerable<string>> CustomAttributeTestCases() =>
			new()
		{
			{ typeof(ClassWithSingleTrait), new[] { "One" } },
			{ typeof(ClassWithMultipleTraits), new[] { "One", "Two" } },
			{ typeof(InheritedClassWithOnlyOwnTrait), new[] { "One" } },
			{ typeof(InheritedClassWithOnlyOwnMultipleTraits), new[] { "One", "Two" } },
			{ typeof(InheritedClassWithSingleBaseClassTrait), new[] { "BaseOne" } },
			{ typeof(InheritedClassWithMultipleBaseClassTraits), new[] { "BaseOne", "BaseTwo" } },
			{ typeof(InheritedClassWithOwnSingleTraitAndSingleBaseClassTrait), new[] { "One", "BaseOne" } },
			{ typeof(InheritedClassWithOwnSingleTraitAndMultipleBaseClassTrait), new[] { "One", "BaseOne", "BaseTwo" } },
			{ typeof(InheritedClassWithOwnMultipleTraitsAndSingleBaseClassTrait), new[] { "One", "Two", "BaseOne" } },
			{ typeof(InheritedClassWithOwnMultipleTraitsAndMultipleBaseClassTrait), new[] { "One", "Two", "BaseOne", "BaseTwo" } }
		};

		[Theory]
		[MemberData(nameof(CustomAttributeTestCases))]
		public void ReturnsCorrectCustomAttributes(Type classType, IEnumerable<string> expectedTraits)
		{
			var testAssembly = new TestAssembly(new ReflectionAssemblyInfo(classType.Assembly));
			var testCollection = new TestCollection(testAssembly, null, "Trait inheritance tests");
			var @class = new ReflectionTypeInfo(classType);
			var testClass = new TestClass(testCollection, @class);
			var methodInfo = new ReflectionMethodInfo(classType.GetMethod("TraitsTest")!);
			var testMethod = new TestMethod(testClass, methodInfo);
			var testCase = new TestableXunitTestCase(testMethod);

			var testTraits = testCase.Traits["Test"];

			Assert.NotNull(testTraits);
			foreach (var expectedTrait in expectedTraits)
				Assert.Contains(expectedTrait, testTraits);
		}

		class BaseClassWithoutTraits
		{ }

		[Trait("Test", "BaseOne")]
		class BaseClassWithSingleTrait
		{ }

		[Trait("Test", "BaseOne"), Trait("Test", "BaseTwo")]
		class BaseClassWithMultipleTraits
		{ }

		[Trait("Test", "One")]
		class ClassWithSingleTrait
		{
			[Fact]
			public void TraitsTest()
			{ }
		}

		[Trait("Test", "One"), Trait("Test", "Two")]
		class ClassWithMultipleTraits
		{
			[Fact]
			public void TraitsTest()
			{ }
		}

		[Trait("Test", "One")]
		class InheritedClassWithOnlyOwnTrait : BaseClassWithoutTraits
		{
			[Fact]
			public void TraitsTest()
			{ }
		}

		[Trait("Test", "One"), Trait("Test", "Two")]
		class InheritedClassWithOnlyOwnMultipleTraits : BaseClassWithoutTraits
		{
			[Fact]
			public void TraitsTest()
			{ }
		}

		class InheritedClassWithSingleBaseClassTrait : BaseClassWithSingleTrait
		{
			[Fact]
			public void TraitsTest()
			{ }
		}

		class InheritedClassWithMultipleBaseClassTraits : BaseClassWithMultipleTraits
		{
			[Fact]
			public void TraitsTest()
			{ }
		}

		[Trait("Test", "One")]
		class InheritedClassWithOwnSingleTraitAndSingleBaseClassTrait : BaseClassWithSingleTrait
		{
			[Fact]
			public void TraitsTest()
			{ }
		}

		[Trait("Test", "One")]
		class InheritedClassWithOwnSingleTraitAndMultipleBaseClassTrait : BaseClassWithMultipleTraits
		{
			[Fact]
			public void TraitsTest()
			{ }
		}

		[Trait("Test", "One"), Trait("Test", "Two")]
		class InheritedClassWithOwnMultipleTraitsAndSingleBaseClassTrait : BaseClassWithSingleTrait
		{
			[Fact]
			public void TraitsTest()
			{ }
		}

		[Trait("Test", "One"), Trait("Test", "Two")]
		class InheritedClassWithOwnMultipleTraitsAndMultipleBaseClassTrait : BaseClassWithMultipleTraits
		{
			[Fact]
			public void TraitsTest()
			{ }
		}
	}

	[Serializable]
	class TestableXunitTestCase : XunitTestCase
	{
		protected TestableXunitTestCase(
			SerializationInfo info,
			StreamingContext context) :
				base(info, context)
		{ }

		public TestableXunitTestCase(
			_ITestMethod testMethod,
			object?[]? testMethodArguments = null,
			_IMessageSink? diagnosticMessageSink = null) :
				base(
					diagnosticMessageSink ?? SpyMessageSink.Create(),
					TestMethodDisplay.ClassAndMethod,
					TestMethodDisplayOptions.None,
					testMethod,
					testMethodArguments,
					null,
					null,
					null,
					null
				)
		{ }
	}
}
