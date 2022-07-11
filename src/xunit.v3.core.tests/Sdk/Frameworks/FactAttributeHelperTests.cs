using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class FactAttributeHelperTests
{
	public class GuardClauses
	{
		_ITestFrameworkDiscoveryOptions discoveryOptions = _TestFrameworkOptions.ForDiscovery();

		[Fact]
		public void NullDiscoveryOptionsThrows()
		{
			var ex = Record.Exception(() => FactAttributeHelper.GetTestCaseDetails(null!, Mocks.TestMethod()));

			var argnEx = Assert.IsType<ArgumentNullException>(ex);
			Assert.Equal("discoveryOptions", argnEx.ParamName);
		}

		[Fact]
		public void NullTestMethodThrows()
		{
			var ex = Record.Exception(() => FactAttributeHelper.GetTestCaseDetails(discoveryOptions, null!));

			var argnEx = Assert.IsType<ArgumentNullException>(ex);
			Assert.Equal("testMethod", argnEx.ParamName);
		}

		[Fact]
		public void TestMethodWithoutFactAttributeThrows()
		{
			var testMethod = Mocks.TestMethod<object>(nameof(object.ToString));

			var ex = Record.Exception(() => FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("testMethod", argEx.ParamName);
			Assert.StartsWith($"Could not locate the FactAttribute on test method '{typeof(object).FullName}.{nameof(object.ToString)}'", argEx.Message);
		}
	}

	public class FactAttributeValues
	{
		_ITestFrameworkDiscoveryOptions discoveryOptions = _TestFrameworkOptions.ForDiscovery();

		[Fact]
		public void DisplayName()
		{
			var testMethod = Mocks.TestMethod("type-name", "method-name");

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal("type-name.method-name", details.TestCaseDisplayName);
		}

		[Theory]
		[InlineData(42, typeof(int))]
		[InlineData("Hello world", typeof(string))]
		[InlineData(null, typeof(object))]
		public void OpenGenericIsClosedByArguments(
			object? testArg,
			Type expectedGenericType)
		{
			var testMethod = TestData.TestMethod<ClassWithGenericTestMethod>(nameof(ClassWithGenericTestMethod.OpenGeneric));

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod, testMethodArguments: new[] { testArg });

			Assert.Equal($"{typeof(ClassWithGenericTestMethod).FullName}.{nameof(ClassWithGenericTestMethod.OpenGeneric)}<{expectedGenericType.Name}>(value: {ArgumentFormatter.Format(testArg)})", details.TestCaseDisplayName);
			var closedMethod = details.ResolvedTestMethod;
			var closedMethodInfo = Assert.IsAssignableFrom<_IReflectionMethodInfo>(closedMethod.Method).MethodInfo;
			Assert.True(closedMethodInfo.IsGenericMethod);
			Assert.False(closedMethodInfo.IsGenericMethodDefinition);
			var genericType = Assert.Single(closedMethodInfo.GetGenericArguments());
			Assert.Same(expectedGenericType, genericType);
		}

		[Fact]
		public void NonStandardMethodDisplay()
		{
			var testMethod = Mocks.TestMethod("type-name", "method-name");
			discoveryOptions.SetMethodDisplay(TestMethodDisplay.Method);

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal("method-name", details.TestCaseDisplayName);
		}

		[Fact]
		public void NonStandardMethodDisplayOptions()
		{
			var testMethod = Mocks.TestMethod("With_an_empty_stack", "count_eq_0X21");
			discoveryOptions.SetMethodDisplayOptions(TestMethodDisplayOptions.All);

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal("With an empty stack, count = 0!", details.TestCaseDisplayName);
		}

		[Fact]
		public void CustomDisplayName()
		{
			var factAttribute = Mocks.FactAttribute(displayName: "Custom Display Name");
			var testMethod = Mocks.TestMethod(methodAttributes: new[] { factAttribute });

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal("Custom Display Name", details.TestCaseDisplayName);
		}

		[Fact]
		public void CustomDisplayNameWithArguments()
		{
			var factAttribute = Mocks.FactAttribute(displayName: "Custom Display Name");
			var param1 = Mocks.ParameterInfo("p1");
			var param2 = Mocks.ParameterInfo("p2");
			var param3 = Mocks.ParameterInfo("p3");
			var testMethod = Mocks.TestMethod(methodAttributes: new[] { factAttribute }, parameters: new[] { param1, param2, param3 });
			var arguments = new object[] { 42, "Hello, world!", 'A' };

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod, testMethodArguments: arguments);

			Assert.Equal("Custom Display Name(p1: 42, p2: \"Hello, world!\", p3: 'A')", details.TestCaseDisplayName);
		}

		[Fact]
		public void NotEnoughTestArguments()
		{
			var param = Mocks.ParameterInfo("p1");
			var testMethod = Mocks.TestMethod(parameters: new[] { param });

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod, testMethodArguments: Array.Empty<object?>());

			Assert.Equal($"{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}(p1: ???)", details.TestCaseDisplayName);
		}

		[CulturedFact]
		public void TooManyTestArguments()
		{
			var param = Mocks.ParameterInfo("p1");
			var testMethod = Mocks.TestMethod(parameters: new[] { param });
			var arguments = new object[] { 42, 21.12M };

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod, testMethodArguments: arguments);

			Assert.Equal($"{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}(p1: 42, ???: {21.12})", details.TestCaseDisplayName);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Explicit(bool @explicit)
		{
			var factAttribute = Mocks.FactAttribute(@explicit: @explicit);
			var testMethod = Mocks.TestMethod(methodAttributes: new[] { factAttribute });

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal(@explicit, details.Explicit);
		}

		[Fact]
		public void SkipReason()
		{
			var factAttribute = Mocks.FactAttribute(skip: "Skip Reason");
			var testMethod = Mocks.TestMethod(methodAttributes: new[] { factAttribute });

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal("Skip Reason", details.SkipReason);
		}

		[Fact]
		public void Timeout()
		{
			var factAttribute = Mocks.FactAttribute(timeout: 42);
			var testMethod = Mocks.TestMethod(methodAttributes: new[] { factAttribute });

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal(42, details.Timeout);
		}

		class ClassWithGenericTestMethod
		{
			[Theory]
			public void OpenGeneric<T>(T value) { }
		}
	}

	public class Traits
	{
		_ITestFrameworkDiscoveryOptions discoveryOptions = _TestFrameworkOptions.ForDiscovery();

		[Fact]
		public void TraitsOnTestMethod()
		{
			var trait1 = Mocks.TraitAttribute("Trait1", "Value1");
			var trait2 = Mocks.TraitAttribute("Trait2", "Value2");
			var testMethod = Mocks.TestMethod(methodAttributes: new[] { trait1, trait2 });

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal("Value1", Assert.Single(details.Traits["Trait1"]));
			Assert.Equal("Value2", Assert.Single(details.Traits["Trait2"]));
		}

		[Fact]
		public void TraitsOnTestClass()
		{
			var trait1 = Mocks.TraitAttribute("Trait1", "Value1");
			var trait2 = Mocks.TraitAttribute("Trait2", "Value2");
			var testMethod = Mocks.TestMethod(classAttributes: new[] { trait1, trait2 });

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal("Value1", Assert.Single(details.Traits["Trait1"]));
			Assert.Equal("Value2", Assert.Single(details.Traits["Trait2"]));
		}

		[Fact]
		public void CustomTrait()
		{
			var testMethod = TestData.TestMethod<ClassWithCustomTraitTest>(nameof(ClassWithCustomTraitTest.BugFix));

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Collection(
				details.Traits.OrderBy(x => x.Key),
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
		public void CustomTraitWithoutDiscoverer()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;
			var trait = Mocks.TraitAttribute<BadTraitAttribute>();
			var testMethod = Mocks.TestMethod(classAttributes: new[] { trait });

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Empty(details.Traits);
			var diagnosticMessages = spy.Messages.OfType<_DiagnosticMessage>();
			var diagnosticMessage = Assert.Single(diagnosticMessages);
			Assert.Equal($"Trait attribute '{typeof(BadTraitAttribute).FullName}' on test method '{details.TestCaseDisplayName}' does not have [TraitDiscoverer]", diagnosticMessage.Message);
		}

		[Fact]
		public void CustomTraitWithBadDiscovererCtor()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;
			var trait = Mocks.TraitAttribute<TraitWithDiscovererWithBadCtorAttribute>();
			var testMethod = Mocks.TestMethod(classAttributes: new[] { trait });

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Empty(details.Traits);
			var diagnosticMessages = spy.Messages.OfType<_DiagnosticMessage>();
			var diagnosticMessage = Assert.Single(diagnosticMessages);
			Assert.Equal($"Could not find empty constructor for '{typeof(TraitDiscovererWithBadCtor).FullName}'", diagnosticMessage.Message);
		}

		[Fact]
		public void CustomTraitWithInvalidDiscovererTypeSpecification()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;
			var trait = Mocks.TraitAttribute<TraitWithInvalidDiscovererTypeSpecification>();
			var testMethod = Mocks.TestMethod(classAttributes: new[] { trait });

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Empty(details.Traits);
			var diagnosticMessages = spy.Messages.OfType<_DiagnosticMessage>();
			var diagnosticMessage = Assert.Single(diagnosticMessages);
			Assert.Equal($"Discoverer on trait attribute '{typeof(TraitWithInvalidDiscovererTypeSpecification).FullName}' appears to be malformed (invalid type reference?)", diagnosticMessage.Message);
		}

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

			var details = FactAttributeHelper.GetTestCaseDetails(discoveryOptions, testMethod);

			var testTraits = details.Traits["Test"];
			foreach (var expectedTrait in expectedTraits)
				Assert.Contains(expectedTrait, testTraits);
		}

		class BadTraitAttribute : Attribute, ITraitAttribute { }

		[TraitDiscoverer(typeof(TraitDiscovererWithBadCtor))]
		class TraitWithDiscovererWithBadCtorAttribute : Attribute, ITraitAttribute { }

		[TraitDiscoverer("foo", "bar")]
		class TraitWithInvalidDiscovererTypeSpecification : Attribute, ITraitAttribute { }

		class TraitDiscovererWithBadCtor : ITraitDiscoverer
		{
			public TraitDiscovererWithBadCtor(int _)
			{ }

			IReadOnlyCollection<KeyValuePair<string, string>> ITraitDiscoverer.GetTraits(_IAttributeInfo traitAttribute)
			{
				throw new NotImplementedException();
			}
		}

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
}
