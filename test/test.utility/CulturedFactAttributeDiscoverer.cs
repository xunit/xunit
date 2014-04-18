using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace TestUtility
{
    public class CulturedFactAttributeDiscoverer : IXunitTestCaseDiscoverer
    {
        public IEnumerable<IXunitTestCase> Discover(ITestCollection testCollection, IAssemblyInfo assembly, ITypeInfo testClass, IMethodInfo testMethod, IAttributeInfo factAttribute)
        {
            var ctorArgs = factAttribute.GetConstructorArguments().ToArray();
            var cultures = Reflector.ConvertArguments(ctorArgs, new[] { typeof(string[]) }).Cast<string[]>().Single();

            if (cultures == null || cultures.Length == 0)
                cultures = new[] { "en-US", "fr-FR" };

            return cultures.Select(culture => new CulturedXunitTestCase(testCollection, assembly, testClass, testMethod, factAttribute, culture)).ToList();
        }
    }
}
