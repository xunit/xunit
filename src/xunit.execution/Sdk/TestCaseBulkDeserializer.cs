using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// INTERNAL CLASS. DO NOT USE.
    /// </summary>
    public class TestCaseBulkDeserializer : LongLivedMarshalByRefObject
    {
        static IMessageSink DiagnosticMessageSink = new NullMessageSink();

        /// <summary/>
        public TestCaseBulkDeserializer(object discovererObject, object executorObject, object serializedTestCasesObject, object callbackObject)
        {
            var discoverer = discovererObject as XunitTestFrameworkDiscoverer;
            var executor = (ITestFrameworkExecutor)executorObject;
            var serializedTestCases = (List<string>)serializedTestCasesObject;
            var callback = (Action<List<KeyValuePair<string, ITestCase>>>)callbackObject;
            var results = serializedTestCases.Select(serialization => Deserialize(discoverer, executor, serialization)).ToList();

            callback(results);
        }

        private KeyValuePair<string, ITestCase> Deserialize(XunitTestFrameworkDiscoverer discoverer, 
                                                            ITestFrameworkExecutor executor,
                                                            string serialization)
        {
            var testCase = default(ITestCase);

            try
            {
                if (serialization.Length > 3 && serialization.StartsWith(":F:"))
                {
                    // Format from TestCaseDescriptorFactory: ":F:{typeName}:{methodName}:{defaultMethodDisplay}"
                    var parts = serialization.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 3)
                    {
                        var typeInfo = discoverer.AssemblyInfo.GetType(parts[1]);
                        var testClass = discoverer.CreateTestClass(typeInfo);
                        var methodInfo = testClass.Class.GetMethod(parts[2], true);
                        var testMethod = new TestMethod(testClass, methodInfo);
                        var defaultMethodDisplay = (TestMethodDisplay)int.Parse(parts[3]);
                        testCase = new XunitTestCase(DiagnosticMessageSink, defaultMethodDisplay, testMethod);
                    }
                }

                if (testCase == null)
                    testCase = executor.Deserialize(serialization);

                return new KeyValuePair<string, ITestCase>(testCase.UniqueID, testCase);
            }
            catch (Exception ex)
            {
                return new KeyValuePair<string, ITestCase>(ex.ToString(), null);
            }
        }
    }
}
