using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit
{
    public class Xunit2Controller : AppDomainXunitController, IMessageSink
    {
        static readonly IXunitControllerFactory factory = new Xunit2ControllerFactory();

        readonly object executor;

        public Xunit2Controller(string assemblyFileName, string configFileName, bool shadowCopy)
            : base(assemblyFileName, configFileName, shadowCopy, Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit2.dll"))
        {
            try
            {
                executor = CreateObject("Xunit.Sdk.Executor2", AssemblyFileName, this);
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

        event Action<ITestMessage> MessageArrived;

        public static IXunitControllerFactory Factory
        {
            get { return factory; }
        }

        public override IEnumerable<ITestCase> EnumerateTests()
        {
            List<ITestCase> results = new List<ITestCase>();
            ManualResetEvent finished = new ManualResetEvent(initialState: false);
            Action<ITestMessage> handler = null;

            handler = msg =>
            {
                var discoveryMessage = msg as ITestCaseDiscoveryMessage;
                if (discoveryMessage != null)
                {
                    results.Add(discoveryMessage.TestCase);
                    return;
                }

                var completeMessage = msg as IDiscoveryCompleteMessage;
                if (completeMessage != null)
                {
                    MessageArrived -= handler;
                    finished.Set();
                }
            };

            MessageArrived += handler;

            CreateObject("Xunit.Sdk.Executor2+EnumerateTests", executor, /*includeSourceInformation*/ false);

            finished.WaitOne();
            return results;
        }

        class Xunit2ControllerFactory : IXunitControllerFactory
        {
            public IXunitController Create(string assemblyFileName, string configFileName, bool shadowCopy)
            {
                return new Xunit2Controller(assemblyFileName, configFileName, shadowCopy);
            }
        }

        public void OnMessage(ITestMessage message)
        {
            if (MessageArrived != null)
                MessageArrived(message);
        }
    }
}
