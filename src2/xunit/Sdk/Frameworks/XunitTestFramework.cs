using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class XunitTestFramework : ITestFramework
    {
        public IEnumerable<ITestCase> Find(IAssemblyInfo assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            return assembly.GetTypes(includePrivateTypes: false).SelectMany(type => FindImpl(assembly, type));
        }

        public IEnumerable<ITestCase> Find(ITypeInfo type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return FindImpl(type.Assembly, type);
        }

        protected virtual IEnumerable<ITestCase> FindImpl(IAssemblyInfo assembly, ITypeInfo type)
        {
            foreach (IMethodInfo method in type.GetMethods(includePrivateMethods: true))
            {
                IAttributeInfo factAttribute = method.GetCustomAttributes(typeof(Fact2Attribute)).FirstOrDefault();
                if (factAttribute != null)
                {
                    IAttributeInfo discovererAttribute = factAttribute.GetCustomAttributes(typeof(XunitDiscovererAttribute)).FirstOrDefault();
                    if (discovererAttribute != null)
                    {
                        Type discovererType = discovererAttribute.GetPropertyValue<Type>("DiscovererType");
                        IXunitDiscoverer discoverer = (IXunitDiscoverer)Activator.CreateInstance(discovererType);

                        foreach (XunitTestCase testCase in discoverer.Discover(assembly, type, method, factAttribute))
                            yield return testCase;
                    }
                }
            }
        }

        public IObservable<ITestCaseResult> Run(IEnumerable<ITestCase> testMethods, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
