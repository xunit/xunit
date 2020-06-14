using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace TestUtility
{
    public class CulturedFactAttributeDiscoverer : IXunitTestCaseDiscoverer
    {
        readonly IMessageSink diagnosticMessageSink;

        public CulturedFactAttributeDiscoverer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            var ctorArgs = factAttribute.GetConstructorArguments().ToArray();
            var cultures = Reflector.ConvertArguments(ctorArgs, new[] { typeof(string[]) }).Cast<string[]>().Single();

            if (cultures == null || cultures.Length == 0)
                cultures = new[] { "en-US", "fr-FR" };

            var methodDisplay = discoveryOptions.MethodDisplayOrDefault();
            var methodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();

            return cultures.Select(culture => new CulturedXunitTestCase(diagnosticMessageSink, methodDisplay, methodDisplayOptions, testMethod, culture)).ToList();
        }
    }
}
