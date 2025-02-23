using System;
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
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		IFactAttribute factAttribute)
	{
		var culturedFactAttribute = factAttribute as CulturedFactAttribute;
		Assert.NotNull(culturedFactAttribute);

		var cultures = culturedFactAttribute.Cultures;
		if (cultures is null || cultures.Length == 0)
			cultures = ["en-US", "fr-FR"];

		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);

		var result =
			cultures
				.Select(
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
						testMethod.Traits.ToReadWrite(StringComparer.OrdinalIgnoreCase),
						timeout: details.Timeout
					)
				)
				.CastOrToReadOnlyCollection();

		return new(result);
	}
}
