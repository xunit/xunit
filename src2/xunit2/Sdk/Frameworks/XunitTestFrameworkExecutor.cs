using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class XunitTestFrameworkExecutor : LongLivedMarshalByRefObject, ITestFrameworkExecutor
    {
        IAssemblyInfo assemblyInfo;

        public XunitTestFrameworkExecutor(string assemblyFileName)
        {
            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFileName));
            assemblyInfo = Reflector.Wrap(assembly);
        }

        public void Dispose() { }

        public void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink)
        {
            bool cancelled = false;
            int totalRun = 0;
            decimal totalTime = 0M;

            if (messageSink.OnMessage(new TestAssemblyStarting { Assembly = assemblyInfo }))
            {
                var classGroups = testMethods.Cast<XunitTestCase>().GroupBy(tc => tc.Class).ToList();

                if (classGroups.Count > 0)
                {
                    if (messageSink.OnMessage(new TestCollectionStarting { Assembly = assemblyInfo }))
                    {
                        foreach (var group in classGroups)
                        {
                            if (messageSink.OnMessage(new TestClassStarting { Assembly = assemblyInfo, ClassName = group.Key.FullName }))
                            {
                                foreach (XunitTestCase testCase in group)
                                {
                                    var delegatingSink = new DelegatingMessageSink<ITestCaseFinished>(messageSink);

                                    // REVIEW: testCase.Run() returning bool implies synchronous behavior, which will probably
                                    // not be true once we start supporting parallelization. This could be achieved by always
                                    // using a delegating sink (like above) and watching for cancellation there, then checking
                                    // for the cancellation result in the delegating sink after work is finished.

                                    cancelled = testCase.Run(delegatingSink);
                                    delegatingSink.Finished.WaitOne();

                                    totalRun += delegatingSink.FinalMessage.TestsRun;
                                    totalTime += delegatingSink.FinalMessage.ExecutionTime;

                                    if (cancelled)
                                        break;
                                }
                            }

                            messageSink.OnMessage(new TestClassFinished { Assembly = assemblyInfo, ClassName = group.Key.FullName, TestsRun = totalRun });

                            if (cancelled)
                                break;
                        }
                    }

                    messageSink.OnMessage(new TestCollectionFinished { Assembly = assemblyInfo, TestsRun = totalRun });
                }
            }

            messageSink.OnMessage(new TestAssemblyFinished { Assembly = assemblyInfo, TestsRun = totalRun, ExecutionTime = totalTime });
        }
    }
}