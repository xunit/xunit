using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
	public static class JsonExtensions
	{
		/// <summary />
		public static string ToJson(this IDictionary<string, object?> data)
		{
			Guard.ArgumentNotNull(nameof(data), data);

			var sb = new StringBuilder();

			foreach (var kvp in data)
				AddValue(sb, kvp.Key, kvp.Value);

			return "{" + sb.ToString() + "}";
		}

		static void AddValue(StringBuilder sb, string name, object? value)
		{
			if (value == null)
				return;

			if (sb.Length != 0)
				sb.Append(',');

			if (value is int || value is long || value is float || value is double || value is decimal)
				sb.AppendFormat(@"""{0}"":{1}", name, Convert.ToString(value, CultureInfo.InvariantCulture));
			else if (value is bool)
				sb.AppendFormat(@"""{0}"":{1}", name, value.ToString()!.ToLower());
			else if (value is DateTime dt)
				sb.AppendFormat(@"""{0}"":""{1}""", name, dt.ToString("o", CultureInfo.InvariantCulture));
			else if (value is IDictionary<string, object?> dict)
				sb.AppendFormat(@"""{0}"":{1}", name, dict.ToJson()); // sub-object
			else
				sb.AppendFormat(@"""{0}"":""{1}""", name, JsonEscape(value.ToString()!));
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

		delegate void AddFields(object obj, Dictionary<string, object?> dict);

		static readonly AddFields AddFieldsForITestCollectionMessage = (obj, dict) =>
		{
			if (!(obj is ITestCollectionMessage testCollectionMesage))
				return;

			dict["assembly"] = testCollectionMesage.TestAssembly.Assembly.Name;
			dict["collectionName"] = testCollectionMesage.TestCollection.DisplayName;
			dict["collectionId"] = testCollectionMesage.TestCollection.UniqueID;
		};

		static readonly AddFields AddFieldsForIFinishedMessage = (obj, dict) =>
		{
			if (!(obj is IFinishedMessage finishedMessage))
				return;

			dict["executionTime"] = finishedMessage.ExecutionTime;
			dict["testsFailed"] = finishedMessage.TestsFailed;
			dict["testsRun"] = finishedMessage.TestsRun;
			dict["testsSkipped"] = finishedMessage.TestsSkipped;
		};

		static readonly AddFields AddFieldsForITestResultMessage = (obj, dict) =>
		{
			if (!(obj is ITestResultMessage testResultMessage))
				return;

			dict["executionTime"] = testResultMessage.ExecutionTime;
			dict["output"] = testResultMessage.Output;
		};

		static readonly AddFields AddFieldsForITestMessage = (obj, dict) =>
		{
			if (!(obj is ITestMessage testMessage))
				return;

			dict["testName"] = testMessage.Test.DisplayName;
		};

		static readonly AddFields AddFieldsForIFailureInformation = (obj, dict) =>
		{
			if (!(obj is IFailureInformation failureInformation))
				return;

			dict["errorMessages"] = ExceptionUtility.CombineMessages(failureInformation);
			dict["stackTraces"] = ExceptionUtility.CombineStackTraces(failureInformation);
		};

		static readonly Dictionary<Type, List<AddFields>> TypeToFieldAdders = new Dictionary<Type, List<AddFields>>()
		{
			{ typeof(_TestCollectionStarting), new List<AddFields> { AddFieldsForITestCollectionMessage } },
			{ typeof(ITestStarting), new List<AddFields> { AddFieldsForITestMessage } },
			{ typeof(ITestSkipped), new List<AddFields> { AddFieldsForITestResultMessage, AddFieldsForITestMessage } },
			{ typeof(IErrorMessage), new List<AddFields> { AddFieldsForIFailureInformation } },
			{ typeof(ITestPassed), new List<AddFields> { AddFieldsForITestResultMessage } },
			{ typeof(ITestFailed), new List<AddFields> { AddFieldsForITestResultMessage, AddFieldsForIFailureInformation, AddFieldsForITestMessage } },
			{ typeof(ITestMethodCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } },
			{ typeof(ITestCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } },
			{ typeof(_TestCollectionFinished), new List<AddFields> { AddFieldsForITestCollectionMessage, AddFieldsForIFinishedMessage } },
			{ typeof(ITestCollectionCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation, AddFieldsForITestCollectionMessage } },
			{ typeof(ITestClassCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } },
			{ typeof(ITestAssemblyCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } },
			{ typeof(ITestCaseCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } }
		};

		static Dictionary<string, object?> InitObject(string messageName, object message, Type messageType, string? flowId = null)
		{
			TypeToFieldAdders[typeof(ITestCollectionStarting)].Add(AddFieldsForITestCollectionMessage);

			var dict = new Dictionary<string, object?> { { "message", messageName } };
			if (flowId != null)
				dict["flowId"] = flowId;

			foreach (var adder in TypeToFieldAdders[messageType])
				adder(message, dict);

			return dict;
		}

		// Json Serializations

		/// <summary />
		public static string ToJson(this _TestCollectionStarting testCollectionStarting)
		{
			Guard.ArgumentNotNull(nameof(testCollectionStarting), testCollectionStarting);

			var json = InitObject("testCollectionStarting", testCollectionStarting, typeof(_TestCollectionStarting), testCollectionStarting.TestCollectionUniqueID);

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this _TestCollectionFinished testCollectionFinished, string flowId)
		{
			Guard.ArgumentNotNull(nameof(testCollectionFinished), testCollectionFinished);
			Guard.ArgumentNotNull(nameof(flowId), flowId);

			var json = InitObject("testCollectionFinished", testCollectionFinished, typeof(_TestCollectionFinished), flowId);

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this ITestFailed testFailed, string flowId)
		{
			Guard.ArgumentNotNull(nameof(testFailed), testFailed);
			Guard.ArgumentNotNull(nameof(flowId), flowId);

			var json = InitObject("testFailed", testFailed, typeof(ITestFailed), flowId);

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this ITestSkipped testSkipped, string flowId)
		{
			Guard.ArgumentNotNull(nameof(testSkipped), testSkipped);
			Guard.ArgumentNotNull(nameof(flowId), flowId);

			var json = InitObject("testSkipped", testSkipped, typeof(ITestSkipped), flowId);
			json["reason"] = testSkipped.Reason;
			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this ITestStarting testStarting, string flowId)
		{
			Guard.ArgumentNotNull(nameof(testStarting), testStarting);
			Guard.ArgumentNotNull(nameof(flowId), flowId);

			var json = InitObject("testStarting", testStarting, typeof(ITestStarting), flowId);

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this IErrorMessage errorMessage)
		{
			Guard.ArgumentNotNull(nameof(errorMessage), errorMessage);

			var json = InitObject("fatalError", errorMessage, typeof(IErrorMessage));

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this ITestPassed testPassed, string flowId)
		{
			Guard.ArgumentNotNull(nameof(testPassed), testPassed);
			Guard.ArgumentNotNull(nameof(flowId), flowId);

			var json = InitObject("testPassed", testPassed, typeof(ITestPassed), flowId);

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this ITestMethodCleanupFailure testMethodCleanupFailure)
		{
			Guard.ArgumentNotNull(nameof(testMethodCleanupFailure), testMethodCleanupFailure);

			var json = InitObject("testMethodCleanupFailure", testMethodCleanupFailure, typeof(ITestMethodCleanupFailure));

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this ITestCleanupFailure testCleanupFailure)
		{
			Guard.ArgumentNotNull(nameof(testCleanupFailure), testCleanupFailure);

			var json = InitObject("testCleanupFailure", testCleanupFailure, typeof(ITestCleanupFailure));

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this ITestCollectionCleanupFailure testCollectionCleanupFailure)
		{
			Guard.ArgumentNotNull(nameof(testCollectionCleanupFailure), testCollectionCleanupFailure);

			var json = InitObject("testCollectionCleanupFailure", testCollectionCleanupFailure, typeof(ITestCollectionCleanupFailure));

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this ITestClassCleanupFailure testClassCleanupFailure)
		{
			Guard.ArgumentNotNull(nameof(testClassCleanupFailure), testClassCleanupFailure);

			var json = InitObject("testClassCleanupFailure", testClassCleanupFailure, typeof(ITestClassCleanupFailure));

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this ITestAssemblyCleanupFailure testAssemblyCleanupFailure)
		{
			Guard.ArgumentNotNull(nameof(testAssemblyCleanupFailure), testAssemblyCleanupFailure);

			var json = InitObject("testAssemblyCleanupFailure", testAssemblyCleanupFailure, typeof(ITestAssemblyCleanupFailure));

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this ITestCaseCleanupFailure testAssemblyCleanupFailure)
		{
			Guard.ArgumentNotNull(nameof(testAssemblyCleanupFailure), testAssemblyCleanupFailure);

			var json = InitObject("testAssemblyCleanupFailure", testAssemblyCleanupFailure, typeof(ITestCaseCleanupFailure));

			return ToJson(json);
		}
	}
}
