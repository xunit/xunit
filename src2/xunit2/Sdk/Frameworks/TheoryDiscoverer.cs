using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Implementation of <see cref="IXunitDiscoverer"/> that supports finding test cases
    /// on methods decorated with <see cref="TheoryAttribute"/>.
    /// </summary>
    public class TheoryDiscoverer : IXunitDiscoverer
    {
        /// <inheritdoc/>
        public IEnumerable<IXunitTestCase> Discover(IAssemblyInfo assembly, ITypeInfo testClass, IMethodInfo testMethod, IAttributeInfo factAttribute)
        {
            // Special case Skip, because we want a single Skip (not one per data item), and a skipped test may
            // not actually have any data (which is quasi-legal, since it's skipped).
            if (factAttribute.GetNamedArgument<string>("Skip") != null)
                return new[] { new XunitTestCase(assembly, testClass, testMethod, factAttribute) };

            try
            {
                List<XunitTestCase> results = new List<XunitTestCase>();

                var dataAttributes = testMethod.GetCustomAttributes(typeof(DataAttribute));
                foreach (var dataAttribute in dataAttributes)
                {
                    var discovererAttribute = dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).First();
                    var args = discovererAttribute.GetConstructorArguments().Cast<string>().ToList();
                    var discovererType = Reflector.GetType(args[0], args[1]);
                    IDataDiscoverer discoverer = (IDataDiscoverer)Activator.CreateInstance(discovererType);

                    // GetData may return null, but that's okay; we'll let the NullRef happen and then catch it
                    // down below so that we get the composite test case.
                    foreach (object[] dataRow in discoverer.GetData(dataAttribute, testMethod))
                        results.Add(new XunitTestCase(assembly, testClass, testMethod, factAttribute, dataRow));
                }

                // REVIEW: Could we re-write LambdaTestCase to just be for exceptions?
                if (results.Count == 0)
                    results.Add(new LambdaTestCase(assembly, testClass, testMethod, factAttribute, () => { throw new InvalidOperationException("No data found for " + testClass.Name + "." + testMethod.Name); }));

                return results;
            }
            catch
            {
                return new XunitTestCase[] { new XunitTheoryTestCase(assembly, testClass, testMethod, factAttribute) };
            }
        }
    }
}