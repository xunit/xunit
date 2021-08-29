﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	public class CulturedFactAttributeDiscoverer : IXunitTestCaseDiscoverer
	{
		readonly _IMessageSink diagnosticMessageSink;

		public CulturedFactAttributeDiscoverer(_IMessageSink diagnosticMessageSink)
		{
			this.diagnosticMessageSink = diagnosticMessageSink;
		}

		public ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestMethod testMethod,
			_IAttributeInfo factAttribute)
		{
			var ctorArgs = factAttribute.GetConstructorArguments().ToArray();
			var cultures = Reflector.ConvertArguments(ctorArgs, new[] { typeof(string[]) }).Cast<string[]>().Single();

			if (cultures == null || cultures.Length == 0)
				cultures = new[] { "en-US", "fr-FR" };

			var methodDisplay = discoveryOptions.MethodDisplayOrDefault();
			var methodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();

			var result =
				cultures
					.Select(culture => CreateTestCase(testMethod, culture, methodDisplay, methodDisplayOptions))
					.CastOrToReadOnlyCollection();

			return new(result);
		}

		CulturedXunitTestCase CreateTestCase(
			_ITestMethod testMethod,
			string culture,
			TestMethodDisplay methodDisplay,
			TestMethodDisplayOptions methodDisplayOptions)
		{
			return new CulturedXunitTestCase(
				diagnosticMessageSink,
				methodDisplay,
				methodDisplayOptions,
				testMethod,
				culture
			);
		}
	}
}
