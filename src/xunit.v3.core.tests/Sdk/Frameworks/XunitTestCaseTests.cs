using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.v2;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestCaseTests
{
	[Fact]
	public static void DefaultBehavior()
	{
		var testMethod = Mocks.TestMethod("MockType", "MockMethod");

		var testCase = new XunitTestCase(SpyMessageSink.Create(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, testMethod);

		Assert.Equal("MockType.MockMethod", testCase.DisplayName);
		Assert.Null(testCase.SkipReason);
		Assert.Empty(testCase.Traits);
	}

	[Fact]
	public static void SkipReason()
	{
		var testMethod = Mocks.TestMethod(skip: "Skip Reason");

		var testCase = new XunitTestCase(SpyMessageSink.Create(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, testMethod);

		Assert.Equal("Skip Reason", testCase.SkipReason);
	}

	[Fact]
	public static void Timeout()
	{
		var testMethod = Mocks.TestMethod(timeout: 42);

		var testCase = new XunitTestCase(SpyMessageSink.Create(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, testMethod);

		Assert.Equal(42, testCase.Timeout);
	}

	public class Traits : AcceptanceTestV3
	{
		[Fact]
		public static void TraitsOnTestMethod()
		{
			var trait1 = Mocks.TraitAttribute("Trait1", "Value1");
			var trait2 = Mocks.TraitAttribute("Trait2", "Value2");
			var testMethod = Mocks.TestMethod(methodAttributes: new[] { trait1, trait2 });

			var testCase = new XunitTestCase(SpyMessageSink.Create(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, testMethod);

			Assert.Equal("Value1", Assert.Single(testCase.Traits["Trait1"]));
			Assert.Equal("Value2", Assert.Single(testCase.Traits["Trait2"]));
		}

		[Fact]
		public static void TraitsOnTestClass()
		{
			var trait1 = Mocks.TraitAttribute("Trait1", "Value1");
			var trait2 = Mocks.TraitAttribute("Trait2", "Value2");
			var testMethod = Mocks.TestMethod(classAttributes: new[] { trait1, trait2 });

			var testCase = new XunitTestCase(SpyMessageSink.Create(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, testMethod);

			Assert.Equal("Value1", Assert.Single(testCase.Traits["Trait1"]));
			Assert.Equal("Value2", Assert.Single(testCase.Traits["Trait2"]));
		}

		[Fact]
		public async void CustomTrait()
		{
			var messages = await RunAsync(typeof(ClassWithCustomTraitTest));
			var passingTests = messages.OfType<_TestPassed>();

			Assert.Collection(
				passingTests,
				passingTest =>
				{
					var passingTestCaseStarting = messages.OfType<_TestCaseStarting>().Where(tcs => tcs.TestCaseUniqueID == passingTest.TestCaseUniqueID).Single();
					Assert.Collection(
						passingTestCaseStarting.Traits.OrderBy(x => x.Key),
						namedTrait =>
						{
							Assert.Equal("Assembly", namedTrait.Key);
							Assert.Collection(namedTrait.Value, value => Assert.Equal("Trait", value));
						},
						namedTrait =>
						{
							Assert.Equal("Author", namedTrait.Key);
							Assert.Collection(namedTrait.Value, value => Assert.Equal("Some Schmoe", value));
						},
						namedTrait =>
						{
							Assert.Equal("Bug", namedTrait.Key);
							Assert.Collection(namedTrait.Value, value => Assert.Equal("2112", value));
						}
					);
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

			var testCase = new XunitTestCase(spy, TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, testMethod);

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
			public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
			{
				var ctorArgs = traitAttribute.GetConstructorArguments().ToList();
				yield return new KeyValuePair<string, string>("Bug", ctorArgs[0].ToString()!);
			}
		}

		[TraitDiscoverer(typeof(BugDiscoverer))]
		class BugAttribute : Attribute, ITraitAttribute
		{
			public BugAttribute(int id) { }
		}

		public static TheoryData<Type, IEnumerable<string>> CustomAttributeTestCases() =>
			new TheoryData<Type, IEnumerable<string>>
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
			var testCase = new XunitTestCase(
				SpyMessageSink.Create(),
				TestMethodDisplay.ClassAndMethod,
				TestMethodDisplayOptions.None,
				testMethod
			);

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

	public class DisplayName
	{
		[Fact]
		public static void CustomDisplayName()
		{
			var testMethod = Mocks.TestMethod(displayName: "Custom Display Name");

			var testCase = new XunitTestCase(SpyMessageSink.Create(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, testMethod);

			Assert.Equal("Custom Display Name", testCase.DisplayName);
		}

		[Fact]
		public static void CustomDisplayNameWithArguments()
		{
			var param1 = Mocks.ParameterInfo("p1");
			var param2 = Mocks.ParameterInfo("p2");
			var param3 = Mocks.ParameterInfo("p3");
			var testMethod = Mocks.TestMethod(displayName: "Custom Display Name", parameters: new[] { param1, param2, param3 });
			var arguments = new object[] { 42, "Hello, world!", 'A' };

			var testCase = new XunitTestCase(SpyMessageSink.Create(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, testMethod, arguments);

			Assert.Equal("Custom Display Name(p1: 42, p2: \"Hello, world!\", p3: 'A')", testCase.DisplayName);
		}
	}
}
