using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class XunitTestFrameworkExecutor : LongLivedMarshalByRefObject, ITestFrameworkExecutor
    {
        IAssemblyInfo assemblyInfo;

        public XunitTestFrameworkExecutor(string assemblyFileName)
        {
            var assembly = Assembly.LoadFile(assemblyFileName);
            assemblyInfo = Reflector.Wrap(assembly);
        }

        public void Dispose() { }

        public void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink)
        {
            messageSink.OnMessage(new TestAssemblyStarting { Assembly = assemblyInfo });

            int totalRun = 0;

            foreach (XunitTestCase testCase in testMethods)
            {
                messageSink.OnMessage(new TestCollectionStarting { Assembly = assemblyInfo });
                messageSink.OnMessage(new TestClassStarting { Assembly = assemblyInfo, ClassName = testCase.Class.FullName });

                var delegatingSink = new DelegatingMessageSink<ITestCaseFinished>(messageSink);
                testCase.Run(delegatingSink);
                delegatingSink.Finished.WaitOne();

                totalRun += delegatingSink.FinalMessage.TestsRun;

                messageSink.OnMessage(new TestClassFinished { Assembly = assemblyInfo, ClassName = testCase.Class.FullName, TestsRun = totalRun });
                messageSink.OnMessage(new TestCollectionFinished { Assembly = assemblyInfo, TestsRun = totalRun });
            }

            messageSink.OnMessage(new TestAssemblyFinished { Assembly = assemblyInfo, TestsRun = totalRun });
        }
    }
}