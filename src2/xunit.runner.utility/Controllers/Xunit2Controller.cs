using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit
{
    public class Xunit2Controller : AppDomainXunitController
    {
        static readonly IXunitControllerFactory factory = new Xunit2ControllerFactory();

        readonly ITestFramework testFramework;

        public Xunit2Controller(string assemblyFileName, string configFileName, bool shadowCopy)
            : base(assemblyFileName, configFileName, shadowCopy, Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit2.dll"))
        {
            try
            {
                // TODO: Detect test framework

                // TODO: CreateObject() assumes that the type in question lives in the xunit2.dll assembly
                testFramework = CreateObject<ITestFramework>("Xunit.Sdk.XunitTestFramework", assemblyFileName);
            }
            catch (TargetInvocationException ex)
            {
                Dispose();
                ex.InnerException.RethrowWithNoStackTraceLoss();
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        public static IXunitControllerFactory Factory
        {
            get { return factory; }
        }

        public override void Find(bool includeSourceInformation, IMessageSink messageSink)
        {
            testFramework.Find(includeSourceInformation, messageSink);
        }

        public override void Find(ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink)
        {
            testFramework.Find(type, includeSourceInformation, messageSink);
        }

        public override void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink)
        {
            testFramework.Run(testMethods, messageSink);
        }

        class Xunit2ControllerFactory : IXunitControllerFactory
        {
            public IXunitController Create(string assemblyFileName, string configFileName, bool shadowCopy)
            {
                return new Xunit2Controller(assemblyFileName, configFileName, shadowCopy);
            }
        }
    }
}
