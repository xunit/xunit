using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Xunit.Abstractions;
using Xunit.Runners;
using Xunit.Runners.UI;

namespace Xunit.Runners
{
    public class MonoTestCase
    {
        private string fqTestMethodName;
        public string AssemblyFileName { get; private set; }
        public ITestCase TestCase { get; private set; }

#if __IOS__ || MAC
        public string DisplayName { get { return RunnerOptions.Current.GetDisplayName(TestCase.DisplayName, TestCase.Method.Name, fqTestMethodName); } }
#else
        public string DisplayName { get { return RunnerOptions.GetDisplayName(TestCase.DisplayName, TestCase.Method.Name, fqTestMethodName); } }
#endif
        public string UniqueName { get; private set; }

        public MonoTestCase(string assemblyFileName, ITestCase testCase, bool forceUniqueNames)
        {
            if (assemblyFileName == null) throw new ArgumentNullException("assemblyFileName");
            if (testCase == null) throw new ArgumentNullException("testCase");

            fqTestMethodName = String.Format("{0}.{1}", testCase.Class.Name, testCase.Method.Name);
            UniqueName = forceUniqueNames ? String.Format("{0} ({1})", fqTestMethodName, testCase.UniqueID) : fqTestMethodName;
            AssemblyFileName = assemblyFileName;
            TestCase = testCase;

            Result = TestState.NotRun;

        }



        

        public TestState Result { get; private set; }


        public void UpdateTestState(ITestResultMessage message)
        {
            Output = message.Output;
            Message = null;
            StackTrace = null;

            if (message is ITestPassed)
            {
                Result = TestState.Passed;
                Message = "Passed";
            }
            if (message is ITestFailed)
            {
                Result = TestState.Failed;
                var failedMessage = (ITestFailed)message;
                Message = ExceptionUtility.CombineMessages(failedMessage);
                StackTrace = ExceptionUtility.CombineStackTraces(failedMessage);
            }
            if (message is ITestSkipped)
            {
                Result = TestState.Skipped;

                var skipped = (ITestSkipped)message;
                Message = skipped.Reason;
            }
        }

        public string Message { get; private set; }
        public string Output { get; private set; }
        public string StackTrace { get; private set; }
    }
}