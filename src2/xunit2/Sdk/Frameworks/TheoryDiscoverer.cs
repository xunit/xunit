using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TheoryDiscoverer : IXunitDiscoverer
    {
        public IEnumerable<XunitTestCase> Discover(IAssemblyInfo assembly, ITypeInfo testClass, IMethodInfo testMethod, IAttributeInfo factAttribute)
        {
            // Special case Skip, because we want a single Skip (not one per data item), and a skipped test may
            // not actually have any data (which is quasi-legal, since it's skipped).
            if (factAttribute.GetPropertyValue<string>("Skip") != null)
                return new[] { new XunitTestCase(assembly, testClass, testMethod, factAttribute) };

            try
            {
                List<XunitTestCase> results = new List<XunitTestCase>();

                var dataAttributes = testMethod.GetCustomAttributes(typeof(DataAttribute));
                foreach (var dataAttribute in dataAttributes)
                {
                    var discovererAttribute = dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).First();
                    Type discovererType = discovererAttribute.GetPropertyValue<Type>("DiscovererType");
                    IDataDiscoverer discoverer = (IDataDiscoverer)Activator.CreateInstance(discovererType);

                    // TODO: Handle null! The discoverer may not know how many data items there are at discovery
                    // time, so in that case, we need to send back a special composite test case.
                    foreach (object[] dataRow in discoverer.GetData(dataAttribute, testMethod))
                        results.Add(new XunitTestCase(assembly, testClass, testMethod, factAttribute, dataRow));
                }

                if (results.Count == 0)
                    results.Add(new LambdaTestCase(assembly, testClass, testMethod, factAttribute, () => { throw new InvalidOperationException("No data found for " + testClass.Name + "." + testMethod.Name); }));

                return results;
            }
            catch (Exception ex)
            {
                return new XunitTestCase[] {
                    new LambdaTestCase(assembly, testClass, testMethod, factAttribute, () => {
                        throw new InvalidOperationException(String.Format("An exception was thrown while getting data for theory {0}.{1}: {2}", testClass.Name, testMethod.Name, ex));
                    })
                };
            }
        }
    }
}