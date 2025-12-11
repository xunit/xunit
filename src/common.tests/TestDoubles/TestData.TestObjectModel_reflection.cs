#if !XUNIT_AOT

using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

// This file manufactures instances of the test object model (using reflection)
partial class TestData
{
	public static XunitDelayEnumeratedTheoryTestCase XunitDelayEnumeratedTheoryTestCase<TClassUnderTest>(
		string methodName,
		IXunitTestCollection? collection = null,
		TestMethodDisplay methodDisplay = TestMethodDisplay.ClassAndMethod,
		TestMethodDisplayOptions methodDisplayOptions = TestMethodDisplayOptions.None,
		bool @explicit = false,
		Type[]? skipExceptions = null,
		string? skipReason = null,
		bool skipTestWithoutData = false,
		Type? skipType = null,
		string? skipUnless = null,
		string? skipWhen = null,
		Dictionary<string, HashSet<string>>? traits = null,
		int timeout = 0,
		string uniqueID = DefaultTestCaseUniqueID)
	{
		var methodInfo = typeof(TClassUnderTest).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
		Guard.ArgumentNotNull($"Could not find method '{methodName}' on type '{typeof(TClassUnderTest).FullName}'", methodInfo, nameof(methodName));

		var testClass = XunitTestClass<TClassUnderTest>(collection);
		var testMethod = XunitTestMethod(testClass, methodInfo);
		var theoryAttribute = new TheoryAttribute { Explicit = @explicit, Timeout = timeout };
		var discoveryOptions = TestFrameworkDiscoveryOptions(methodDisplay: methodDisplay, methodDisplayOptions: methodDisplayOptions);
		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, theoryAttribute);

		return new(
			details.ResolvedTestMethod,
			details.TestCaseDisplayName,
			uniqueID ?? details.UniqueID,
			@explicit,
			skipTestWithoutData,
			skipExceptions,
			skipReason,
			skipType,
			skipUnless,
			skipWhen,
			traits ?? testMethod.Traits.ToReadWrite(StringComparer.OrdinalIgnoreCase),
			timeout: timeout
		);
	}

	public static XunitTest XunitTest<TClassUnderTest>(
		string methodName,
		bool? @explicit = null,
		string? skipReason = null,
		Type? skipType = null,
		string? skipUnless = null,
		string? skipWhen = null,
		string? testDisplayName = null,
		string? testLabel = null,
		object?[]? testMethodArguments = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		int? timeout = null,
		string uniqueID = DefaultTestUniqueID)
	{
		var testCase = XunitTestCase<TClassUnderTest>(methodName);

		return XunitTest(
			testCase,
			null,
			@explicit,
			skipReason ?? testCase.SkipReason,
			skipType ?? testCase.SkipType,
			skipUnless ?? testCase.SkipUnless,
			skipWhen ?? testCase.SkipWhen,
			testDisplayName,
			testLabel,
			testMethodArguments,
			traits,
			timeout,
			uniqueID
		);
	}

	public static XunitTestCase XunitTestCase<TClassUnderTest>(
		string methodName,
		bool? @explicit = null,
		TestMethodDisplay methodDisplay = TestMethodDisplay.ClassAndMethod,
		TestMethodDisplayOptions methodDisplayOptions = TestMethodDisplayOptions.None,
		Type[]? skipExceptions = null,
		string? skipReason = null,
		Type? skipType = null,
		string? skipUnless = null,
		string? skipWhen = null,
		IXunitTestCollection? testCollection = null,
		string? testLabel = null,
		object?[]? testMethodArguments = null,
		int? timeout = null,
		Dictionary<string, HashSet<string>>? traits = null,
		string uniqueID = DefaultTestCaseUniqueID)
	{
		var methodInfo = typeof(TClassUnderTest).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
		Guard.ArgumentNotNull($"Could not find method '{methodName}' on type '{typeof(TClassUnderTest).FullName}'", methodInfo, nameof(methodName));

		var testClass = XunitTestClass<TClassUnderTest>(testCollection);
		var testMethod = XunitTestMethod(testClass, methodInfo, testMethodArguments);

		return XunitTestCase(
			testMethod,
			@explicit,
			methodDisplay,
			methodDisplayOptions,
			skipExceptions,
			skipReason,
			skipType,
			skipUnless,
			skipWhen,
			testLabel,
			testMethodArguments,
			timeout,
			traits,
			uniqueID
		);
	}

	public static XunitTestMethod XunitTestMethod<TClassUnderTest>(
		string methodName,
		object?[]? testMethodArguments = null,
		string uniqueID = DefaultTestMethodUniqueID)
	{
		var methodInfo = typeof(TClassUnderTest).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
		Guard.ArgumentNotNull($"Could not find method '{methodName}' on type '{typeof(TClassUnderTest).FullName}'", methodInfo, nameof(methodName));

		var testClass = XunitTestClass<TClassUnderTest>();
		return XunitTestMethod(testClass, methodInfo, testMethodArguments, uniqueID);
	}
}

#endif  // !XUNIT_AOT
