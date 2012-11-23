using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class XunitTestFramework : ITestFramework
    {
        ISourceInformationProvider sourceProvider;

        public XunitTestFramework()
            : this(new VisualStudioSourceInformationProvider()) { }

        public XunitTestFramework(ISourceInformationProvider sourceProvider)
        {
            this.sourceProvider = sourceProvider;
        }

        public IEnumerable<ITestCase> Find(IAssemblyInfo assembly, bool includeSourceInformation)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            return assembly.GetTypes(includePrivateTypes: false).SelectMany(type => FindImpl(assembly, type, includeSourceInformation));
        }

        public IEnumerable<ITestCase> Find(ITypeInfo type, bool includeSourceInformation)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return FindImpl(type.Assembly, type, includeSourceInformation);
        }

        protected virtual IEnumerable<ITestCase> FindImpl(IAssemblyInfo assembly, ITypeInfo type, bool includeSourceInformation)
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
                            yield return UpdateTestCaseWithSourceInfo(testCase, includeSourceInformation);
                    }
                }
            }
        }

        public IObservable<ITestCaseResult> Run(IEnumerable<ITestCase> testMethods, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        private ITestCase UpdateTestCaseWithSourceInfo(XunitTestCase testCase, bool includeSourceInformation)
        {
            if (includeSourceInformation)
            {
                Tuple<string, int?> sourceInfo = sourceProvider.GetSourceInformation(testCase);
                testCase.SourceFileName = sourceInfo.Item1;
                testCase.SourceFileLine = sourceInfo.Item2;
            }

            return testCase;
        }

        class VisualStudioSourceInformationProvider : ISourceInformationProvider
        {
            public Tuple<string, int?> GetSourceInformation(ITestCase testCase)
            {
                return Tuple.Create<string, int?>(null, null);

                // TODO: Load DiaSession dynamically, since it's only available when running inside of Visual Studio.
                //       Or look at the CCI2 stuff from the Rx framework: https://github.com/Reactive-Extensions/IL2JS/tree/master/CCI2/PdbReader

                //IMethodTestCase methodTestCase = testCase as IMethodTestCase;
                //if (methodTestCase == null)
                //    return Tuple.Create<string, int?>(null, null);
            }
        }
    }
}
