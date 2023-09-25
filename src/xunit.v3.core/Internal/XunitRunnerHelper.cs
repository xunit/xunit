using System.Collections.Generic;
using System.Globalization;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class XunitRunnerHelper
{
	/// <summary/>
	public static RunSummary FailTestCases(
		IReadOnlyCollection<_ITestCase> testCases,
		IMessageBus messageBus,
		string messageFormat)
	{
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);

		var result = new RunSummary();

		foreach (var testCase in testCases)
		{
			var assemblyUniqueID = testCase.TestCollection.TestAssembly.UniqueID;
			var testUniqueID = UniqueIDGenerator.ForTest(testCase.UniqueID, -1);

			var caseStarting = new _TestCaseStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				SkipReason = testCase.SkipReason,
				SourceFilePath = testCase.SourceFilePath,
				SourceLineNumber = testCase.SourceLineNumber,
				TestCaseDisplayName = testCase.TestCaseDisplayName,
				TestCaseUniqueID = testCase.UniqueID,
				TestClassName = testCase.TestClassName,
				TestClassNamespace = testCase.TestClassNamespace,
				TestClassNameWithNamespace = testCase.TestClassNameWithNamespace,
				TestClassUniqueID = testCase.TestClass?.UniqueID,
				TestCollectionUniqueID = testCase.TestCollection.UniqueID,
				TestMethodName = testCase.TestMethod?.Method.Name,
				TestMethodUniqueID = testCase.TestMethod?.UniqueID,
				Traits = testCase.Traits,
			};
			messageBus.QueueMessage(caseStarting);

			var testStarting = new _TestStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				Explicit = false,
				TestCaseUniqueID = testCase.UniqueID,
				TestClassUniqueID = testCase.TestClass?.UniqueID,
				TestCollectionUniqueID = testCase.TestCollection.UniqueID,
				TestDisplayName = testCase.TestCaseDisplayName,
				TestMethodUniqueID = testCase.TestMethod?.UniqueID,
				TestUniqueID = testUniqueID,
				Timeout = 0,
				Traits = testCase.Traits,
			};
			messageBus.QueueMessage(testStarting);

			var failed = new _TestFailed
			{
				AssemblyUniqueID = assemblyUniqueID,
				Cause = FailureCause.Exception,
				ExceptionParentIndices = new[] { -1 },
				ExceptionTypes = new string?[] { null },
				ExecutionTime = 0m,
				Messages = new[] { string.Format(CultureInfo.CurrentCulture, messageFormat, testCase.TestCaseDisplayName) },
				Output = string.Empty,
				StackTraces = new string?[] { null },
				TestCaseUniqueID = testCase.UniqueID,
				TestClassUniqueID = testCase.TestClass?.UniqueID,
				TestCollectionUniqueID = testCase.TestCollection.UniqueID,
				TestMethodUniqueID = testCase.TestMethod?.UniqueID,
				TestUniqueID = testUniqueID,
			};
			messageBus.QueueMessage(failed);

			var testFinished = new _TestFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = 0m,
				Output = string.Empty,
				TestCaseUniqueID = testCase.UniqueID,
				TestClassUniqueID = testCase.TestClass?.UniqueID,
				TestCollectionUniqueID = testCase.TestCollection.UniqueID,
				TestMethodUniqueID = testCase.TestMethod?.UniqueID,
				TestUniqueID = testUniqueID,
			};
			messageBus.QueueMessage(testFinished);

			var caseFinished = new _TestCaseFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = 0m,
				TestCaseUniqueID = testCase.UniqueID,
				TestClassUniqueID = testCase.TestClass?.UniqueID,
				TestCollectionUniqueID = testCase.TestCollection.UniqueID,
				TestMethodUniqueID = testCase.TestMethod?.UniqueID,
				TestsFailed = 1,
				TestsNotRun = 0,
				TestsSkipped = 0,
				TestsTotal = 1,
			};
			messageBus.QueueMessage(caseFinished);

			result.Total++;
			result.Failed++;
		}

		return result;
	}
}
