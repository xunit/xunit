using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Xunit.Runner.VisualStudio.TestAdapter
{
    public class TestCaseFilterHelper
    {
        private const string DisplayNameString = "DisplayName";
        private const string FullyQualifiedNameString = "FullyQualifiedName";
        private HashSet<string> knownTraits;
        private List<string> supportedPropertyNames;

        public TestCaseFilterHelper(HashSet<string> knownTraits)
        {
            this.knownTraits = knownTraits;
            this.supportedPropertyNames = this.GetSupportedPropertyNames();
        }

        public IGrouping<string, TestCase> GetFilteredTestList(IGrouping<string, TestCase> grouping, IRunContext runContext, IMessageLogger logger, Stopwatch stopwatch, string assemblyFileName)
        {
            ITestCaseFilterExpression filter = null;
            if (this.GetTestCaseFilterExpression(runContext, logger, stopwatch, assemblyFileName, out filter))
            {
                if (filter != null)
                {
                    return new Grouping<string, TestCase>(grouping.Key, grouping.Where(testCase => filter.MatchTestCase(testCase, (p) => this.PropertyProvider(testCase, p))).ToList());
                }
            }
            else
            {
                // Error while filtering, ensure discovered test list is empty
                return new Grouping<string, TestCase>(assemblyFileName, new List<TestCase>());
            }

            // No filter is specified return the original list
            return grouping;
        }

        public object PropertyProvider(TestCase testCase, string name)
        {
            // Traits filtering
            if (this.knownTraits.Contains(name))
            {
                List<string> result = new List<string>();
                foreach (Trait t in testCase.Traits)
                {
                    if (String.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase))
                        result.Add(t.Value);
                }

                if (result.Count > 0)
                {
                    return result.ToArray();
                }
            }
            else
            {
                // Handle the displayName and fullyQualifierNames independently
                if (String.Equals(name, FullyQualifiedNameString, StringComparison.OrdinalIgnoreCase))
                    return testCase.FullyQualifiedName;
                if (String.Equals(name, DisplayNameString, StringComparison.OrdinalIgnoreCase))
                    return testCase.DisplayName;
            }

            return null;
        }

        private bool GetTestCaseFilterExpression(IRunContext runContext, IMessageLogger logger, Stopwatch stopwatch, string assemblyFileName, out ITestCaseFilterExpression filter)
        {
            filter = null;
            try
            {
                // In Microsoft.VisualStudio.TestPlatform.ObjectModel V11 IRunContext provides a TestCaseFilter property
                // GetTestCaseFilter only exists in V12+
                MethodInfo getTestCaseFilterMethod = runContext.GetType().GetMethod("GetTestCaseFilter");
                if (getTestCaseFilterMethod != null)
                {
                    filter = (ITestCaseFilterExpression)getTestCaseFilterMethod.Invoke(runContext, new object[] { this.supportedPropertyNames, null });
                }
                return true;
            }
            catch (TargetInvocationException e)
            {
                var formatException = e.InnerException as TestPlatformFormatException;
                if (formatException != null)
                {
                    logger.SendMessage(TestMessageLevel.Error,
                        String.Format("[xUnit.net {0}] Exception discovering tests from {1}: Invalid filter string {2}", stopwatch.Elapsed, Path.GetFileName(assemblyFileName), formatException.FilterValue));
                }
            }
            return false;
        }

        private List<string> GetSupportedPropertyNames()
        {
            // Returns the set of well-known property names usually used with the Test Plugins (Used Test Traits + DisplayName + FullyQualifiedName)
            if (this.supportedPropertyNames == null)
            {
                this.supportedPropertyNames = this.knownTraits.ToList();
                this.supportedPropertyNames.Add(DisplayNameString);
                this.supportedPropertyNames.Add(FullyQualifiedNameString);
            }
            return this.supportedPropertyNames;
        }
    }
}
