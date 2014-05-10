using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.SpriteKit;
using MonoTouch.UIKit;
using Xunit.Abstractions;
using Xunit.Runners.UI;

namespace Xunit.Runner.iOS
{
    class MonoTestCase
    {
        public string AssemblyFileName { get; private set; }
        public ITestCase TestCase { get; private set; }

        public string DisplayName { get; private set; }
        public string UniqueName { get; private set; }

        public MonoTestCase(string assemblyFileName, ITestCase testCase, bool forceUniqueNames)
        {
            if (assemblyFileName == null) throw new ArgumentNullException("assemblyFileName");
            if (testCase == null) throw new ArgumentNullException("testCase");

            var fqTestMethodName = String.Format("{0}.{1}", testCase.Class.Name, testCase.Method.Name);

            DisplayName = GetDisplayName(testCase.DisplayName, testCase.Method.Name, fqTestMethodName);
            UniqueName = forceUniqueNames ? String.Format("{0} ({1})", fqTestMethodName, testCase.UniqueID) : fqTestMethodName;
            AssemblyFileName = assemblyFileName;
            TestCase = testCase;

            Result = TestState.NotRun;

        }

        static string GetDisplayName(string displayName, string shortMethodName, string fullyQualifiedMethodName)
        {
            if (TouchOptions.Current.NameDisplay == NameDisplay.Full)
                return displayName;
            if (displayName == fullyQualifiedMethodName || displayName.StartsWith(fullyQualifiedMethodName + "("))
                return shortMethodName + displayName.Substring(fullyQualifiedMethodName.Length);
            return displayName;
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