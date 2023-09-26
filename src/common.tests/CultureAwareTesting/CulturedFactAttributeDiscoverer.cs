using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

public class CulturedFactAttributeDiscoverer : IXunitTestCaseDiscoverer
{
	public ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestMethod testMethod,
		_IAttributeInfo factAttribute)
	{
		var ctorArgs = factAttribute.GetConstructorArguments().ToArray();
		var cultures = Reflector.ConvertArguments(ctorArgs, new[] { typeof(string[]) }).Cast<string[]>().Single();

		if (cultures is null || cultures.Length == 0)
			cultures = new[] { "en-US", "fr-FR" };

		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);
		var traits = TestIntrospectionHelper.GetTraits(testMethod);

		var result =
			cultures
				.Select(
					// TODO: How do we get source information in here?
					culture => new CulturedXunitTestCase(
						culture,
						details.ResolvedTestMethod,
						details.TestCaseDisplayName,
						details.UniqueID,
						details.Explicit,
						details.SkipReason,
						traits,
						timeout: details.Timeout
					)
				)
				.CastOrToReadOnlyCollection();

		return new(result);
	}
}
