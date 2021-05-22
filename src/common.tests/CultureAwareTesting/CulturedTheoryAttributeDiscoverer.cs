using System.Collections.Generic;
using System.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	public class CulturedTheoryAttributeDiscoverer : TheoryDiscoverer
	{
		public CulturedTheoryAttributeDiscoverer(_IMessageSink diagnosticMessageSink)
			: base(diagnosticMessageSink) { }

		protected override IReadOnlyCollection<IXunitTestCase> CreateTestCasesForDataRow(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestMethod testMethod,
			_IAttributeInfo theoryAttribute,
			object?[] dataRow)
		{
			var cultures = GetCultures(theoryAttribute);

			return cultures.Select(
				culture => new CulturedXunitTestCase(
					DiagnosticMessageSink,
					discoveryOptions.MethodDisplayOrDefault(),
					discoveryOptions.MethodDisplayOptionsOrDefault(),
					testMethod,
					culture,
					dataRow
				)
			).CastOrToReadOnlyCollection();
		}

		protected override IReadOnlyCollection<IXunitTestCase> CreateTestCasesForTheory(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestMethod testMethod,
			_IAttributeInfo theoryAttribute)
		{
			var cultures = GetCultures(theoryAttribute);
			return cultures.Select(
				culture => new CulturedXunitTheoryTestCase(
					DiagnosticMessageSink,
					discoveryOptions.MethodDisplayOrDefault(),
					discoveryOptions.MethodDisplayOptionsOrDefault(),
					testMethod,
					culture
				)
			).CastOrToReadOnlyCollection();
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
