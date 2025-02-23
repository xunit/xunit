using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

public class CulturedTheoryAttributeDiscoverer : TheoryDiscoverer
{
	protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		ITheoryAttribute theoryAttribute,
		ITheoryDataRow dataRow,
		object?[] testMethodArguments)
	{
		var cultures = GetCultures(theoryAttribute);
		var details = TestIntrospectionHelper.GetTestCaseDetailsForTheoryDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow, testMethodArguments);
		var traits = TestIntrospectionHelper.GetTraits(testMethod, dataRow);

		var result = cultures.Select(
			culture => new CulturedXunitTestCase(
				culture,
				details.ResolvedTestMethod,
				details.TestCaseDisplayName,
				details.UniqueID,
				details.Explicit,
				details.SkipExceptions,
				details.SkipReason,
				details.SkipType,
				details.SkipUnless,
				details.SkipWhen,
				traits,
				testMethodArguments,
				timeout: details.Timeout
			)
		).CastOrToReadOnlyCollection();

		return new(result);
	}

	protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		ITheoryAttribute theoryAttribute)
	{
		var cultures = GetCultures(theoryAttribute);
		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, theoryAttribute);

		var result =
			cultures
				.Select(
					culture => new CulturedXunitTheoryTestCase(
						culture,
						details.ResolvedTestMethod,
						details.TestCaseDisplayName,
						details.UniqueID,
						details.Explicit,
						theoryAttribute.SkipTestWithoutData,
						details.SkipExceptions,
						details.SkipReason,
						details.SkipType,
						details.SkipUnless,
						details.SkipWhen,
						testMethod.Traits.ToReadWrite(StringComparer.OrdinalIgnoreCase),
						timeout: details.Timeout
					)
				)
				.CastOrToReadOnlyCollection();

		return new(result);
	}

	static string[] GetCultures(ITheoryAttribute theoryAttribute)
	{
		var culturedTheoryAttribute = theoryAttribute as CulturedTheoryAttribute;
		Assert.NotNull(culturedTheoryAttribute);

		var cultures = culturedTheoryAttribute.Cultures;
		if (cultures is null || cultures.Length == 0)
			cultures = ["en-US", "fr-FR"];

		return cultures;
	}
}
