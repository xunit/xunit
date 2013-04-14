using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestFrameworkExecutor"/> that supports execution
    /// of unit tests linked against xunit2.dll.
    /// </summary>
    public class XunitTestFrameworkExecutor : LongLivedMarshalByRefObject, ITestFrameworkExecutor
    {
        IAssemblyInfo assemblyInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestFrameworkExecutor"/> class.
        /// </summary>
        /// <param name="assemblyFileName">Path of the test assembly.</param>
        public XunitTestFrameworkExecutor(string assemblyFileName)
        {
            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFileName));
            assemblyInfo = Reflector.Wrap(assembly);
        }

        /// <inheritdoc/>
        public ITestCase Deserialize(string value)
        {
            return TestCaseSerializer.Deserialize(value);
        }

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink)
        {
            bool cancelled = false;
            int totalRun = 0;
            int totalFailed = 0;
            int totalSkipped = 0;
            decimal totalTime = 0M;

            string currentDirectory = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyInfo.AssemblyPath));

                if (messageSink.OnMessage(new TestAssemblyStarting()))
                {
                    var classGroups = testMethods.Cast<XunitTestCase>().GroupBy(tc => tc.Class).ToList();

                    if (classGroups.Count > 0)
                    {
                        if (messageSink.OnMessage(new TestCollectionStarting()))
                        {
                            foreach (var group in classGroups)
                            {
                                if (!messageSink.OnMessage(new TestClassStarting { ClassName = group.Key.Name }))
                                    cancelled = true;
                                else
                                {
                                    var methodGroups = group.GroupBy(tc => tc.Method);

                                    foreach (var method in methodGroups)
                                    {
                                        if (!messageSink.OnMessage(new TestMethodStarting { ClassName = group.Key.Name, MethodName = method.Key.Name }))
                                            cancelled = true;
                                        else
                                        {
                                            foreach (XunitTestCase testCase in method)
                                            {
                                                var delegatingSink = new DelegatingMessageSink<ITestCaseFinished>(messageSink);

                                                // REVIEW: testCase.Run() returning bool implies synchronous behavior, which will probably
                                                // not be true once we start supporting parallelization. This could be achieved by always
                                                // using a delegating sink (like above) and watching for cancellation there, then checking
                                                // for the cancellation result in the delegating sink after work is finished.

                                                cancelled = testCase.Run(delegatingSink);
                                                delegatingSink.Finished.WaitOne();

                                                totalRun += delegatingSink.FinalMessage.TestsRun;
                                                totalFailed += delegatingSink.FinalMessage.TestsFailed;
                                                totalSkipped += delegatingSink.FinalMessage.TestsSkipped;
                                                totalTime += delegatingSink.FinalMessage.ExecutionTime;

                                                if (cancelled)
                                                    break;
                                            }
                                        }

                                        if (!messageSink.OnMessage(new TestMethodFinished { ClassName = group.Key.Name, MethodName = method.Key.Name }))
                                            cancelled = true;

                                        if (cancelled)
                                            break;
                                    }
                                }

                                if (!messageSink.OnMessage(new TestClassFinished { Assembly = assemblyInfo, ClassName = group.Key.Name, TestsRun = totalRun }))
                                    cancelled = true;

                                if (cancelled)
                                    break;
                            }
                        }

                        messageSink.OnMessage(new TestCollectionFinished { Assembly = assemblyInfo, TestsRun = totalRun });
                    }
                }

                messageSink.OnMessage(new TestAssemblyFinished { Assembly = assemblyInfo, TestsRun = totalRun, TestsFailed = totalFailed, TestsSkipped = totalSkipped, ExecutionTime = totalTime });
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);
            }
        }
    }
}