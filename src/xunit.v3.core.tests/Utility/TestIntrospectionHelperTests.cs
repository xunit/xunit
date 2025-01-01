using System;
using System.Linq;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestIntrospectionHelperTests
{
	// Simplified signature that auto-looks up the FactAttribute
	static (
		string TestCaseDisplayName,
		bool Explicit,
		Type[]? SkipExceptions,
		string? SkipReason,
		Type? SkipType,
		string? SkipUnless,
		string? SkipWhen,
		int Timeout,
		string UniqueID,
		IXunitTestMethod ResolvedTestMethod
	) _GetTestCaseDetails(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		object?[]? testMethodArguments = null) =>
			TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, testMethod.FactAttributes.First(), testMethodArguments);

	public class GuardClauses
	{
		readonly ITestFrameworkDiscoveryOptions discoveryOptions = TestData.TestFrameworkDiscoveryOptions();

		[Fact]
		public void NullDiscoveryOptionsThrows()
		{
			var ex = Record.Exception(() => TestIntrospectionHelper.GetTestCaseDetails(null!, Mocks.XunitTestMethod(), Mocks.FactAttribute()));

			var argnEx = Assert.IsType<ArgumentNullException>(ex);
			Assert.Equal("discoveryOptions", argnEx.ParamName);
		}

		[Fact]
		public void NullTestMethodThrows()
		{
			var ex = Record.Exception(() => TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, null!, Mocks.FactAttribute()));

			var argnEx = Assert.IsType<ArgumentNullException>(ex);
			Assert.Equal("testMethod", argnEx.ParamName);
		}

		[Fact]
		public void NullFactAttributeThrows()
		{
			var ex = Record.Exception(() => TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, Mocks.XunitTestMethod(), null!));

			var argEx = Assert.IsType<ArgumentNullException>(ex);
			Assert.Equal("factAttribute", argEx.ParamName);
		}
	}

	public class GetTestCaseDetails
	{
		readonly ITestFrameworkDiscoveryOptions discoveryOptions = TestData.TestFrameworkDiscoveryOptions();

		[Fact]
		public void DisplayName()
		{
			var testMethod = TestData.XunitTestMethod<GetTestCaseDetails>(nameof(DisplayName));

			var details = _GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal("TestIntrospectionHelperTests+GetTestCaseDetails.DisplayName", details.TestCaseDisplayName);
		}

		[Theory]
		[InlineData(42, typeof(int))]
		[InlineData("Hello world", typeof(string))]
		[InlineData(null, typeof(object))]
		public void OpenGenericIsClosedByArguments(
			object? testArg,
			Type expectedGenericType)
		{
			var testMethod = TestData.XunitTestMethod<ClassWithGenericTestMethod>(nameof(ClassWithGenericTestMethod.OpenGeneric));

			var details = _GetTestCaseDetails(discoveryOptions, testMethod, [testArg]);

			Assert.Equal($"{typeof(ClassWithGenericTestMethod).FullName}.{nameof(ClassWithGenericTestMethod.OpenGeneric)}<{expectedGenericType.Name}>(_: {ArgumentFormatter.Format(testArg)})", details.TestCaseDisplayName);
			var closedMethod = details.ResolvedTestMethod;
			var closedMethodInfo = closedMethod.Method;
			Assert.True(closedMethodInfo.IsGenericMethod);
			Assert.False(closedMethodInfo.IsGenericMethodDefinition);
			var genericType = Assert.Single(closedMethodInfo.GetGenericArguments());
			Assert.Same(expectedGenericType, genericType);
		}

		[Fact]
		public void NonStandardMethodDisplay()
		{
			var testMethod = Mocks.XunitTestMethod(methodName: "method-name");
			discoveryOptions.SetMethodDisplay(TestMethodDisplay.Method);

			var details = _GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal("method-name", details.TestCaseDisplayName);
		}

		[Fact]
		public void NonStandardMethodDisplayOptions()
		{
			var testClass = Mocks.XunitTestClass(testClassName: "With_an_empty_stack");
			var testMethod = Mocks.XunitTestMethod(testClass: testClass, methodName: "count_eq_0X21");
			discoveryOptions.SetMethodDisplayOptions(TestMethodDisplayOptions.All);

			var details = _GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal("With an empty stack, count = 0!", details.TestCaseDisplayName);
		}

		[Fact]
		public void CustomDisplayName()
		{
			var factAttribute = Mocks.FactAttribute(displayName: "Custom Display Name");
			var testMethod = Mocks.XunitTestMethod(factAttributes: [factAttribute]);

			var details = _GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal("Custom Display Name", details.TestCaseDisplayName);
		}

		[Fact]
		public void CustomDisplayNameWithArguments()
		{
			var testMethod = TestData.XunitTestMethod<CustomDisplayNameWithArgumentsTestClass>(nameof(CustomDisplayNameWithArgumentsTestClass.TestMethod));

			var details = _GetTestCaseDetails(discoveryOptions, testMethod, [42, "Hello, world!", 'A']);

			Assert.Equal("Custom Display Name(_1: 42, _2: \"Hello, world!\", _3: 'A')", details.TestCaseDisplayName);
		}

		[Fact]
		public void NotEnoughTestArguments()
		{
			var testMethod = TestData.XunitTestMethod<CustomDisplayNameWithArgumentsTestClass>(nameof(CustomDisplayNameWithArgumentsTestClass.TestMethod));

			var details = _GetTestCaseDetails(discoveryOptions, testMethod, [42]);

			Assert.Equal("Custom Display Name(_1: 42, _2: ???, _3: ???)", details.TestCaseDisplayName);
		}

		[CulturedFact]
		public void TooManyTestArguments()
		{
			var testMethod = TestData.XunitTestMethod<CustomDisplayNameWithArgumentsTestClass>(nameof(CustomDisplayNameWithArgumentsTestClass.TestMethod));

			var details = _GetTestCaseDetails(discoveryOptions, testMethod, [42, "Hello, world!", 'A', 21.12m]);

			Assert.Equal($"Custom Display Name(_1: 42, _2: \"Hello, world!\", _3: 'A', ???: {21.12m})", details.TestCaseDisplayName);
		}

#pragma warning disable xUnit1003 // Theory methods must have test data

		class CustomDisplayNameWithArgumentsTestClass
		{
			[Theory(DisplayName = "Custom Display Name")]
			public void TestMethod(int _1, string _2, char _3) { }
		}

#pragma warning restore xUnit1003 // Theory methods must have test data

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Explicit(bool @explicit)
		{
			var factAttribute = Mocks.FactAttribute(@explicit: @explicit);
			var testMethod = Mocks.XunitTestMethod(factAttributes: [factAttribute]);

			var details = _GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal(@explicit, details.Explicit);
		}

		[Fact]
		public void SkipReason()
		{
			var factAttribute = Mocks.FactAttribute(skip: "Skip Reason");
			var testMethod = Mocks.XunitTestMethod(factAttributes: [factAttribute]);

			var details = _GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal("Skip Reason", details.SkipReason);
		}

		[Fact]
		public void SkipExceptions()
		{
			var factAttribute = Mocks.FactAttribute(skipExceptions: [typeof(NotImplementedException)]);
			var testMethod = Mocks.XunitTestMethod(factAttributes: [factAttribute]);

			var details = _GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal([typeof(NotImplementedException)], details.SkipExceptions);
		}

		[Fact]
		public void Timeout()
		{
			var factAttribute = Mocks.FactAttribute(timeout: 42);
			var testMethod = Mocks.XunitTestMethod(factAttributes: [factAttribute]);

			var details = _GetTestCaseDetails(discoveryOptions, testMethod);

			Assert.Equal(42, details.Timeout);
		}

#pragma warning disable xUnit1003 // Theory methods must have test data

		class ClassWithGenericTestMethod
		{
			[Theory]
			public void OpenGeneric<T>(T _) { }
		}

#pragma warning restore xUnit1003 // Theory methods must have test data
	}
}
