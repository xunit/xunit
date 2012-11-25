using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    // REVIEW: If all test frameworks must be MBRO, maybe we should use a base class instead of/in addition to an interface
    public class XunitTestFramework : LongLivedMarshalByRefObject, ITestFramework
    {
        IAssemblyInfo assemblyInfo;
        ISourceInformationProvider sourceProvider;

        public XunitTestFramework(string assemblyFileName)
        {
            var assembly = Assembly.LoadFile(assemblyFileName);
            assemblyInfo = Reflector2.Wrap(assembly);
            sourceProvider = new VisualStudioSourceInformationProvider();
        }

        /// <summary>
        /// This constructor is for unit testing purposes only. Do not use in production code.
        /// </summary>
        public XunitTestFramework(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider = null)
        {
            this.assemblyInfo = assemblyInfo;
            this.sourceProvider = sourceProvider ?? new VisualStudioSourceInformationProvider();
        }

        public void Find(bool includeSourceInformation, IMessageSink messageSink)
        {
            Guard.ArgumentNotNull("messageSink", messageSink);

            foreach (var type in assemblyInfo.GetTypes(includePrivateTypes: false))
                FindImpl(type, includeSourceInformation, messageSink);

            messageSink.OnMessage(new DiscoveryCompleteMessage());
        }

        public void Find(ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink)
        {
            Guard.ArgumentNotNull("type", type);
            Guard.ArgumentNotNull("messageSink", messageSink);

            FindImpl(type, includeSourceInformation, messageSink);

            messageSink.OnMessage(new DiscoveryCompleteMessage());
        }

        protected virtual void FindImpl(ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink)
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

                        foreach (XunitTestCase testCase in discoverer.Discover(assemblyInfo, type, method, factAttribute))
                            messageSink.OnMessage(new TestCaseDiscoveryMessage { TestCase = UpdateTestCaseWithSourceInfo(testCase, includeSourceInformation) });
                    }
                }
            }
        }

        public void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink)
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
