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
			{ typeof(_TestStarting), new List<AddFields> { AddFieldsForITestMessage } },
			{ typeof(_TestSkipped), new List<AddFields> { AddFieldsForITestResultMessage, AddFieldsForITestMessage } },
			{ typeof(_ErrorMessage), new List<AddFields> { AddFieldsForIFailureInformation } },
			{ typeof(_TestPassed), new List<AddFields> { AddFieldsForITestResultMessage } },
			{ typeof(_TestFailed), new List<AddFields> { AddFieldsForITestResultMessage, AddFieldsForIFailureInformation, AddFieldsForITestMessage } },
			{ typeof(_TestMethodCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } },
			{ typeof(_TestCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } },
			{ typeof(_TestCollectionFinished), new List<AddFields> { AddFieldsForITestCollectionMessage, AddFieldsForIFinishedMessage } },
			{ typeof(_TestCollectionCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation, AddFieldsForITestCollectionMessage } },
			{ typeof(_TestClassCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } },
			{ typeof(_TestAssemblyCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } },
			{ typeof(_TestCaseCleanupFailure), new List<AddFields> { AddFieldsForIFailureInformation } }
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
		public static string ToJson(this _TestFailed testFailed, string flowId)
		{
			Guard.ArgumentNotNull(nameof(testFailed), testFailed);
			Guard.ArgumentNotNull(nameof(flowId), flowId);

			var json = InitObject("testFailed", testFailed, typeof(_TestFailed), flowId);

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this _TestSkipped testSkipped, string flowId)
		{
			Guard.ArgumentNotNull(nameof(testSkipped), testSkipped);
			Guard.ArgumentNotNull(nameof(flowId), flowId);

			var json = InitObject("testSkipped", testSkipped, typeof(_TestSkipped), flowId);
			json["reason"] = testSkipped.Reason;
			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this _TestStarting testStarting, string flowId)
		{
			Guard.ArgumentNotNull(nameof(testStarting), testStarting);
			Guard.ArgumentNotNull(nameof(flowId), flowId);

			var json = InitObject("testStarting", testStarting, typeof(_TestStarting), flowId);

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this _ErrorMessage errorMessage)
		{
			Guard.ArgumentNotNull(nameof(errorMessage), errorMessage);

			var json = InitObject("fatalError", errorMessage, typeof(_ErrorMessage));

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this _TestPassed testPassed, string flowId)
		{
			Guard.ArgumentNotNull(nameof(testPassed), testPassed);
			Guard.ArgumentNotNull(nameof(flowId), flowId);

			var json = InitObject("testPassed", testPassed, typeof(_TestPassed), flowId);

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this _TestMethodCleanupFailure testMethodCleanupFailure)
		{
			Guard.ArgumentNotNull(nameof(testMethodCleanupFailure), testMethodCleanupFailure);

			var json = InitObject("testMethodCleanupFailure", testMethodCleanupFailure, typeof(_TestMethodCleanupFailure));

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this _TestCleanupFailure testCleanupFailure)
		{
			Guard.ArgumentNotNull(nameof(testCleanupFailure), testCleanupFailure);

			var json = InitObject("testCleanupFailure", testCleanupFailure, typeof(_TestCleanupFailure));

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this _TestCollectionCleanupFailure testCollectionCleanupFailure)
		{
			Guard.ArgumentNotNull(nameof(testCollectionCleanupFailure), testCollectionCleanupFailure);

			var json = InitObject("testCollectionCleanupFailure", testCollectionCleanupFailure, typeof(_TestCollectionCleanupFailure));

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this _TestClassCleanupFailure testClassCleanupFailure)
		{
			Guard.ArgumentNotNull(nameof(testClassCleanupFailure), testClassCleanupFailure);

			var json = InitObject("testClassCleanupFailure", testClassCleanupFailure, typeof(_TestClassCleanupFailure));

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this _TestAssemblyCleanupFailure testAssemblyCleanupFailure)
		{
			Guard.ArgumentNotNull(nameof(testAssemblyCleanupFailure), testAssemblyCleanupFailure);

			var json = InitObject("testAssemblyCleanupFailure", testAssemblyCleanupFailure, typeof(_TestAssemblyCleanupFailure));

			return ToJson(json);
		}

		/// <summary />
		public static string ToJson(this _TestCaseCleanupFailure testAssemblyCleanupFailure)
		{
			Guard.ArgumentNotNull(nameof(testAssemblyCleanupFailure), testAssemblyCleanupFailure);

			var json = InitObject("testAssemblyCleanupFailure", testAssemblyCleanupFailure, typeof(_TestCaseCleanupFailure));

			return ToJson(json);
		}
	}
}
