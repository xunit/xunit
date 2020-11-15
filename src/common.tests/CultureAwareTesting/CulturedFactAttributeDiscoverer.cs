using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Runner.v2;
using Xunit.v3;

namespace Xunit.Sdk
{
	public class CulturedFactAttributeDiscoverer : IXunitTestCaseDiscoverer
	{
		readonly _IMessageSink diagnosticMessageSink;

		public CulturedFactAttributeDiscoverer(_IMessageSink diagnosticMessageSink)
		{
			this.diagnosticMessageSink = diagnosticMessageSink;
		}

		public IEnumerable<IXunitTestCase> Discover(
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			ITestMethod testMethod,
			IAttributeInfo factAttribute)
		{
			var ctorArgs = factAttribute.GetConstructorArguments().ToArray();
			var cultures = Reflector.ConvertArguments(ctorArgs, new[] { typeof(string[]) }).Cast<string[]>().Single();

			if (cultures == null || cultures.Length == 0)
				cultures = new[] { "en-US", "fr-FR" };

			var methodDisplay = discoveryOptions.MethodDisplayOrDefault();
			var methodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();

			var assemblyUniqueID = FactDiscoverer.ComputeUniqueID(testMethod.TestClass.TestCollection.TestAssembly);
			var collectionUniqueID = FactDiscoverer.ComputeUniqueID(assemblyUniqueID, testMethod.TestClass.TestCollection);
			var classUniqueID = FactDiscoverer.ComputeUniqueID(collectionUniqueID, testMethod.TestClass);
			var methodUniqueID = FactDiscoverer.ComputeUniqueID(classUniqueID, testMethod);

			return
				cultures
					.Select(culture => CreateTestCase(assemblyUniqueID, collectionUniqueID, classUniqueID, methodUniqueID, testMethod, culture, methodDisplay, methodDisplayOptions))
					.ToList();
		}

		CulturedXunitTestCase CreateTestCase(
			string testAssemblyUniqueID,
			string testCollectionUniqueID,
			string? testClassUniqueID,
			string? testMethodUniqueID,
			ITestMethod testMethod,
			string culture,
			TestMethodDisplay methodDisplay,
			TestMethodDisplayOptions methodDisplayOptions)
		{
			return new CulturedXunitTestCase(
				testAssemblyUniqueID,
				testCollectionUniqueID,
				testClassUniqueID,
				testMethodUniqueID,
				diagnosticMessageSink,
				methodDisplay,
				methodDisplayOptions,
				testMethod,
				culture
			);
		}
	}
}
