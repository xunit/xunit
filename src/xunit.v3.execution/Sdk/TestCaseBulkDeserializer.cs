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
            var discoverer = (ITestFrameworkDiscoverer)discovererObject;
            var executor = (ITestFrameworkExecutor)executorObject;
            var serializedTestCases = (List<string>)serializedTestCasesObject;
            var callback = (Action<List<KeyValuePair<string, ITestCase>>>)callbackObject;
            var results = serializedTestCases.Select(serialization => Deserialize(discoverer, executor, serialization)).ToList();

            callback(results);
        }

        private static KeyValuePair<string, ITestCase> Deserialize(ITestFrameworkDiscoverer discoverer,
                                                                   ITestFrameworkExecutor executor,
                                                                   string serialization)
        {
            try
            {
                var testCase = executor.Deserialize(serialization);
                return new KeyValuePair<string, ITestCase>(testCase.UniqueID, testCase);
            }
            catch (Exception ex)
            {
                return new KeyValuePair<string, ITestCase>($"Test case deserialization failure: {ex}", null);
            }
        }
    }
}
