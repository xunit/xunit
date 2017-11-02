using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace TestUtility
{
    public class CulturedTheoryAttributeDiscoverer : TheoryDiscoverer
    {
        public CulturedTheoryAttributeDiscoverer(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink) { }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[] dataRow)
        {
            var cultures = GetCultures(theoryAttribute);
            return cultures.Select(culture => new CulturedXunitTestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, culture, dataRow)).ToList();
        }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute)
        {
            var cultures = GetCultures(theoryAttribute);
            return cultures.Select(culture => new CulturedXunitTheoryTestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, culture)).ToList();
        }

        static string[] GetCultures(IAttributeInfo culturedTheoryAttribute)
        {
            var ctorArgs = culturedTheoryAttribute.GetConstructorArguments().ToArray();
            var cultures = Reflector.ConvertArguments(ctorArgs, new[] { typeof(string[]) }).Cast<string[]>().Single();

            if (cultures == null || cultures.Length == 0)
                cultures = new[] { "en-US", "fr-FR" };

            return cultures;
        }
    }
}
