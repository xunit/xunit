using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="IXunitTestCaseDiscoverer"/> that supports finding test cases
/// on methods decorated with <see cref="FactAttribute"/>.
/// </summary>
public class FactDiscoverer : IXunitTestCaseDiscoverer
{
	/// <summary>
	/// Creates a single <see cref="XunitTestCase"/> for the given test method.
	/// </summary>
	/// <param name="discoveryOptions">The discovery options to be used.</param>
	/// <param name="testMethod">The test method.</param>
	/// <param name="factAttribute">The attribute that decorates the test method.</param>
	protected virtual IXunitTestCase CreateTestCase(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		IFactAttribute factAttribute)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(factAttribute);

		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);

		return new XunitTestCase(
			details.ResolvedTestMethod,
			details.TestCaseDisplayName,
			details.UniqueID,
			details.Explicit,
			details.SkipExceptions,
			details.SkipReason,
			details.SkipType,
			details.SkipUnless,
			details.SkipWhen,
			testMethod.Traits.ToReadWrite(StringComparer.OrdinalIgnoreCase),
			timeout: details.Timeout
		);
	}

	/// <summary>
	/// Discover test cases from a test method. By default, if the method is generic, or
	/// it contains arguments, returns a single <see cref="ExecutionErrorTestCase"/>;
	/// otherwise, it returns the result of calling <see cref="CreateTestCase"/>.
	/// </summary>
	/// <param name="discoveryOptions">The discovery options to be used.</param>
	/// <param name="testMethod">The test method the test cases belong to.</param>
	/// <param name="factAttribute">The fact attribute attached to the test method.</param>
	/// <returns>Returns zero or more test cases represented by the test method.</returns>
	public virtual ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		IFactAttribute factAttribute)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(factAttribute);

		var testCase =
			testMethod.Parameters.Count != 0
				? ErrorTestCase(discoveryOptions, testMethod, factAttribute, "[Fact] methods are not allowed to have parameters. Did you mean to use [Theory]?")
				: testMethod.IsGenericMethodDefinition
					? ErrorTestCase(discoveryOptions, testMethod, factAttribute, "[Fact] methods are not allowed to be generic.")
					: CreateTestCase(discoveryOptions, testMethod, factAttribute);

		return new([testCase]);
	}

	static ExecutionErrorTestCase ErrorTestCase(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		IFactAttribute factAttribute,
		string message)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(factAttribute);

		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);

		return new(
			details.ResolvedTestMethod,
			details.TestCaseDisplayName,
			details.UniqueID,
			message
		);
	}
}
