using System;
using System.Collections.Generic;
using Xunit;

namespace TestUtility
{
    public class StubTestRunner : ITestRunner
    {
        private readonly IExecutorWrapper executor;
        public static readonly List<string> Actions = new List<string>();

        public StubTestRunner(IExecutorWrapper executor)
        {
            this.executor = executor;
        }

        public TestRunnerResult RunAssembly()
        {
            throw new NotImplementedException();
        }

        public TestRunnerResult RunAssembly(IEnumerable<IResultXmlTransform> transforms)
        {
            throw new NotImplementedException();
        }

        public TestRunnerResult RunClass(string type)
        {
            throw new NotImplementedException();
        }

        public TestRunnerResult RunTest(string type, string method)
        {
            throw new NotImplementedException();
        }

        public TestRunnerResult RunTests(string type, List<string> methods)
        {
            Actions.Add(string.Format("RunTests: Assembly={0} ConfigFile={1} Type={2} Methods={3}",
                                      executor.AssemblyFilename,
                                      executor.ConfigFilename ?? "(null)",
                                      type,
                                      string.Join(", ", methods.ToArray())));

            return TestRunnerResult.Passed;
        }
    }
}
