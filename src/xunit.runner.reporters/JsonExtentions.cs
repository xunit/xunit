using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public static class JsonExtentions
    {
        public static string ToJson(this IDictionary<string, object> data)
        {
            var sb = new StringBuilder();

            foreach (var kvp in data)
                AddValue(sb, kvp.Key, kvp.Value);

            return "{" + sb.ToString() + "}";
        }

        static void AddValue(StringBuilder sb, string name, object value)
        {
            if (value == null)
                return;

            if (sb.Length != 0)
                sb.Append(',');

            if (value is int || value is long || value is float || value is double || value is decimal)
                sb.AppendFormat(@"""{0}"":{1}", name, Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
            else if (value is bool)
                sb.AppendFormat(@"""{0}"":{1}", name, value.ToString().ToLower());
            else if (value is DateTime dt)
                sb.AppendFormat(@"""{0}"":""{1}""", name, dt.ToString("o", CultureInfo.InvariantCulture));
            else if (value is IDictionary<string, object> dict)
                sb.AppendFormat(@"""{0}"":{1}", name, dict.ToJson()); // sub-object
            else
                sb.AppendFormat(@"""{0}"":""{1}""", name, JsonEscape(value.ToString()));
        }

        static string JsonEscape(string value)
        {
            var sb = new StringBuilder();

            foreach (var c in value)
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append(@"\"""); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\n': sb.Append("\\n"); break;
                    default:
                        if (c < 32)
                            sb.AppendFormat("\\u{0:X4}", (int)c);
                        else
                            sb.Append(c);
                        break;
                }

            return sb.ToString();
        }

        // Field Adders

        delegate void AddFields(object obj, Dictionary<string, object> dict);

        static AddFields AddFieldsForITestCollectionMessage = (obj, dict) =>
        {
            var testCollectionMesage = obj as ITestCollectionMessage;
            if (testCollectionMesage == null)
                return;

            dict["assembly"] = testCollectionMesage.TestAssembly.Assembly.Name;
            dict["collectionName"] = testCollectionMesage.TestCollection.DisplayName;
            dict["collectionId"] = testCollectionMesage.TestCollection.UniqueID;
        };

        static AddFields AddFieldsForIFinishedMessage = (obj, dict) =>
        {
            var finishedMessage = obj as IFinishedMessage;
            if (finishedMessage == null)
                return;

            dict["executionTime"] = finishedMessage.ExecutionTime;
            dict["testsFailed"] = finishedMessage.TestsFailed;
            dict["testsRun"] = finishedMessage.TestsRun;
            dict["testsSkipped"] = finishedMessage.TestsSkipped;
        };

        static AddFields AddFieldsForITestResultMessage = (obj, dict) =>
        {
            var testResultMessage = obj as ITestResultMessage;
            if (testResultMessage == null)
                return;

            dict["executionTime"] = testResultMessage.ExecutionTime;
            dict["output"] = testResultMessage.Output;
        };

        static AddFields AddFieldsForITestMessage = (obj, dict) =>
        {
            var testMessage = obj as ITestMessage;
            if (testMessage == null)
                return;

            dict["testName"] = testMessage.Test.DisplayName;
        };

        static AddFields AddFieldsForIFailureInformation = (obj, dict) =>
        {
            var failureInformation = obj as IFailureInformation;
            if (failureInformation == null)
                return;

            dict["errorMessages"] = ExceptionUtility.CombineMessages(failureInformation);
            dict["stackTraces"] = ExceptionUtility.CombineStackTraces(failureInformation);
        };

        static Dictionary<Type, List<AddFields>> TypeToFieldAdders = new Dictionary<Type, List<AddFields>>()
        {
            { typeof(ITestCollectionStarting), new List<AddFields> { AddFieldsForITestCollectionMessage } },
            { typeof(ITestStarting), new List<AddFields> { AddFieldsForITestMessage } },
            { typeof(ITestSkipped), new List<AddFields> { AddFieldsForITestResultMessage, AddFieldsForITestMessage } },
            { typeof(IErrorMessage), new List<AddFields> { AddFieldsForIFailureInformation } },
            { typeof(ITestPassed), new List<AddFields> { AddFieldsForITestResultMessage } },
            { typeof(ITestFailed), new List<AddFields> { AddFieldsForITestResultMessage, AddFieldsForIFailureInformation, AddFieldsForITestMessage } },
            { typeof(ITestMethodCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } },
            { typeof(ITestCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } },
            { typeof(ITestCollectionFinished), new List<AddFields> { AddFieldsForITestCollectionMessage, AddFieldsForIFinishedMessage } },
            { typeof(ITestCollectionCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation, AddFieldsForITestCollectionMessage } },
            { typeof(ITestClassCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } },
            { typeof(ITestAssemblyCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } },
            { typeof(ITestCaseCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } }
        };

        static Dictionary<string, object> initObject(string messageName, object message, Type messageType, string flowId = null)
        {
            TypeToFieldAdders[typeof(ITestCollectionStarting)].Add(AddFieldsForITestCollectionMessage);

            var dict = new Dictionary<string, object> { { "message", messageName } };
            if (flowId != null)
                dict["flowId"] = flowId;

            foreach (var adder in TypeToFieldAdders[messageType])
                adder(message, dict);

            return dict;
        }

        // Json Serializations

        public static string ToJson(this ITestCollectionStarting testCollectionStarting, string flowId)
        {
            var json = initObject("testCollectionStarting", testCollectionStarting, typeof(ITestCollectionStarting), flowId);

            return ToJson(json);
        }

        public static string ToJson(this ITestCollectionFinished testCollectionFinished, string flowId)
        {
            var json = initObject("testCollectionFinished", testCollectionFinished, typeof(ITestCollectionFinished), flowId);

            return ToJson(json);
        }

        public static string ToJson(this ITestFailed testFailed, string flowId)
        {
            var json = initObject("testFailed", testFailed, typeof(ITestFailed), flowId);

            return ToJson(json);
        }

        public static string ToJson(this ITestSkipped testSkipped, string flowId)
        {
            var json = initObject("testSkipped", testSkipped, typeof(ITestSkipped), flowId);
            json["reason"] = testSkipped.Reason;
            return ToJson(json);
        }

        public static string ToJson(this ITestStarting testStarting, string flowId)
        {
            var json = initObject("testStarting", testStarting, typeof(ITestStarting), flowId);

            return ToJson(json);
        }

        public static string ToJson(this IErrorMessage errorMessage)
        {
            var json = initObject("fatalError", errorMessage, typeof(IErrorMessage));

            return ToJson(json);
        }

        public static string ToJson(this ITestPassed testPassed, string flowId)
        {
            var json = initObject("testPassed", testPassed, typeof(ITestPassed), flowId);

            return ToJson(json);
        }

        public static string ToJson(this ITestMethodCleanupFailure ITestMethodCleanupFailure)
        {
            var json = initObject("testMethodCleanupFailure", ITestMethodCleanupFailure, typeof(ITestMethodCleanupFailure));

            return ToJson(json);
        }

        public static string ToJson(this ITestCleanupFailure testCleanupFailure)
        {
            var json = initObject("testCleanupFailure", testCleanupFailure, typeof(ITestCleanupFailure));

            return ToJson(json);
        }

        public static string ToJson(this ITestCollectionCleanupFailure testCollectionCleanupFailure)
        {
            var json = initObject("testCollectionCleanupFailure", testCollectionCleanupFailure, typeof(ITestCollectionCleanupFailure));

            return ToJson(json);
        }

        public static string ToJson(this ITestClassCleanupFailure testClassCleanupFailure)
        {
            var json = initObject("testClassCleanupFailure", testClassCleanupFailure, typeof(ITestClassCleanupFailure));

            return ToJson(json);
        }

        public static string ToJson(this ITestAssemblyCleanupFailure testAssemblyCleanupFailure)
        {
            var json = initObject("testAssemblyCleanupFailure", testAssemblyCleanupFailure, typeof(ITestAssemblyCleanupFailure));

            return ToJson(json);
        }

        public static string ToJson(this ITestCaseCleanupFailure testAssemblyCleanupFailure)
        {
            var json = initObject("testAssemblyCleanupFailure", testAssemblyCleanupFailure, typeof(ITestCaseCleanupFailure));

            return ToJson(json);
        }
    }
}
