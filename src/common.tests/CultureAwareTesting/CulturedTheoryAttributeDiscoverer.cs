using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit
{
	public class CulturedTheoryAttributeDiscoverer : TheoryDiscoverer
	{
		protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestMethod testMethod,
			_IAttributeInfo theoryAttribute,
			string? displayName,
			Dictionary<string, List<string>>? traits,
			object?[] dataRow)
		{
			var cultures = GetCultures(theoryAttribute);
			var result = cultures.Select(
				culture => new CulturedXunitTestCase(
					discoveryOptions.MethodDisplayOrDefault(),
					discoveryOptions.MethodDisplayOptionsOrDefault(),
					testMethod,
					culture,
					dataRow,
					traits,
					displayName)
			).CastOrToReadOnlyCollection();

			return new(result);
		}

		protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestMethod testMethod,
			_IAttributeInfo theoryAttribute)
		{
			var cultures = GetCultures(theoryAttribute);
			var result = cultures.Select(
				culture => new CulturedXunitTheoryTestCase(
					discoveryOptions.MethodDisplayOrDefault(),
					discoveryOptions.MethodDisplayOptionsOrDefault(),
					testMethod,
					culture
				)
			).CastOrToReadOnlyCollection();

			return new(result);
		}

		static string[] GetCultures(_IAttributeInfo culturedTheoryAttribute)
		{
			var ctorArgs = culturedTheoryAttribute.GetConstructorArguments().ToArray();
			var cultures = Reflector.ConvertArguments(ctorArgs, new[] { typeof(string[]) }).Cast<string[]>().Single();

			if (cultures == null || cultures.Length == 0)
				cultures = new[] { "en-US", "fr-FR" };

			return cultures;
		}
	}
}
