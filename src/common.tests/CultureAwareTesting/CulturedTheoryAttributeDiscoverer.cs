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
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestMethod testMethod,
		_IAttributeInfo theoryAttribute,
		ITheoryDataRow dataRow,
		object?[] testMethodArguments)
	{
		var cultures = GetCultures(theoryAttribute);
		var details = TestIntrospectionHelper.GetTestCaseDetailsForTheoryDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow, testMethodArguments);
		var traits = TestIntrospectionHelper.GetTraits(testMethod, dataRow);

		var result = cultures.Select(
			// TODO: How do we get source information in here?
			culture => new CulturedXunitTestCase(
				culture,
				details.ResolvedTestMethod,
				details.TestCaseDisplayName,
				details.UniqueID,
				details.Explicit,
				details.SkipReason,
				traits,
				testMethodArguments,
				timeout: details.Timeout
			)
		).CastOrToReadOnlyCollection();

		return new(result);
	}

	protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestMethod testMethod,
		_IAttributeInfo theoryAttribute)
	{
		var cultures = GetCultures(theoryAttribute);
		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, theoryAttribute);
		var traits = TestIntrospectionHelper.GetTraits(testMethod);

		var result =
			cultures
				.Select(
					// TODO: How do we get source information in here?
					culture => new CulturedXunitTheoryTestCase(
						culture,
						details.ResolvedTestMethod,
						details.TestCaseDisplayName,
						details.UniqueID,
						details.Explicit,
						traits,
						timeout: details.Timeout
					)
				)
				.CastOrToReadOnlyCollection();

		return new(result);
	}

	static string[] GetCultures(_IAttributeInfo culturedTheoryAttribute)
	{
		var ctorArgs = culturedTheoryAttribute.GetConstructorArguments().ToArray();
		var cultures = Reflector.ConvertArguments(ctorArgs, new[] { typeof(string[]) }).Cast<string[]>().Single();

		if (cultures is null || cultures.Length == 0)
			cultures = new[] { "en-US", "fr-FR" };

		return cultures;
	}
}
