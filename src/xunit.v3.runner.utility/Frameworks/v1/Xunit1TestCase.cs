#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.v1;

/// <summary>
/// Contains the data required to serialize a test case for xUnit.net v1.
/// </summary>
public sealed class Xunit1TestCase : IXunitSerializable
{
	static readonly IReadOnlyDictionary<string, TestAttachment> EmptyAttachments = new Dictionary<string, TestAttachment>();

	string? assemblyUniqueID;
	string? testCollectionUniqueID;
	string? testCaseDisplayName;
	string? testCaseUniqueID;
	string? testClass;
	string? testClassUniqueID;
	string? testMethod;
	string? testMethodUniqueID;
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits = new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Initializes a new instance of the <see cref="Xunit1TestCase"/> class.
	/// </summary>
	public Xunit1TestCase()
	{ }

	/// <summary>
	/// Deserialization constructor.
	/// </summary>
	/// <summary>
	/// Gets the unique ID for the test assembly.
	/// </summary>
	public string AssemblyUniqueID
	{
		get => this.ValidateNullablePropertyValue(assemblyUniqueID, nameof(AssemblyUniqueID));
		set => assemblyUniqueID = Guard.ArgumentNotNull(value, nameof(AssemblyUniqueID));
	}

	/// <summary>
	/// Gets the reason this test is being skipped; will return <c>null</c> when
	/// the test is not skipped.
	/// </summary>
	public string? SkipReason { get; set; }

	/// <summary>
	/// Gets the source file path of the test method, if known.
	/// </summary>
	public string? SourceFilePath { get; set; }

	/// <summary>
	/// Gets the source line number of the test method, if known.
	/// </summary>
	public int? SourceLineNumber { get; set; }

	/// <summary>
	/// Gets the unique ID of the test collection.
	/// </summary>
	public string TestCollectionUniqueID
	{
		get => this.ValidateNullablePropertyValue(testCollectionUniqueID, nameof(TestCollectionUniqueID));
		set => testCollectionUniqueID = Guard.ArgumentNotNull(value, nameof(TestCollectionUniqueID));
	}

	/// <summary>
	/// Gets the display name for the test case.
	/// </summary>
	public string TestCaseDisplayName
	{
		get => this.ValidateNullablePropertyValue(testCaseDisplayName, nameof(TestCaseDisplayName));
		set => testCaseDisplayName = Guard.ArgumentNotNull(value, nameof(TestCaseDisplayName));
	}

	/// <summary>
	/// Gets the unique ID for the test case.
	/// </summary>
	public string TestCaseUniqueID
	{
		get => this.ValidateNullablePropertyValue(testCaseUniqueID, nameof(TestCaseUniqueID));
		set => testCaseUniqueID = Guard.ArgumentNotNull(value, nameof(TestCaseUniqueID));
	}

	/// <summary>
	/// Gets the fully qualified type name of the test class.
	/// </summary>
	public string TestClass
	{
		get => this.ValidateNullablePropertyValue(testClass, nameof(TestClass));
		set => testClass = Guard.ArgumentNotNull(value, nameof(TestClass));
	}

	/// <summary>
	/// Gets the unique ID for the test class.
	/// </summary>
	public string TestClassUniqueID
	{
		get => this.ValidateNullablePropertyValue(testClassUniqueID, nameof(TestClassUniqueID));
		set => testClassUniqueID = Guard.ArgumentNotNull(value, nameof(TestClassUniqueID));
	}

	/// <summary>
	/// Gets the name of the test method.
	/// </summary>
	public string TestMethod
	{
		get => this.ValidateNullablePropertyValue(testMethod, nameof(TestMethod));
		set => testMethod = Guard.ArgumentNotNull(value, nameof(TestMethod));
	}

	/// <summary>
	/// Gets the unique ID of the test method.
	/// </summary>
	public string TestMethodUniqueID
	{
		get => this.ValidateNullablePropertyValue(testMethodUniqueID, nameof(TestMethodUniqueID));
		set => testMethodUniqueID = Guard.ArgumentNotNull(value, nameof(TestMethodUniqueID));
	}

	/// <summary>
	/// Gets the traits that are associated with this test case.
	/// </summary>
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits
	{
		get => traits;
		set
		{
			var newTraits = new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase);
			if (value is not null)
				foreach (var kvp in value)
					newTraits[kvp.Key] = kvp.Value;
			traits = newTraits;
		}
	}

	void IXunitSerializable.Deserialize(IXunitSerializationInfo info)
	{
		AssemblyUniqueID = Guard.NotNull("Could not retrieve AssemblyUniqueID from serialization", info.GetValue<string>("id"));
		SkipReason = info.GetValue<string>("sr");
		SourceFilePath = info.GetValue<string>("sp");
		TestCollectionUniqueID = Guard.NotNull("Could not retrieve TestCollectionUniqueID from serialization", info.GetValue<string>("coid"));
		TestCaseDisplayName = Guard.NotNull("Could not retrieve TestCaseDisplayName from serialization", info.GetValue<string>("cadn"));
		TestCaseUniqueID = Guard.NotNull("Could not retrieve TestCaseUniqueID from serialization", info.GetValue<string>("caid"));
		TestClass = Guard.NotNull("Could not retrieve TestClass from serialization", info.GetValue<string>("cl"));
		TestClassUniqueID = Guard.NotNull("Could not retrieve TestClassUniqueID from serialization", info.GetValue<string>("clid"));
		TestMethod = Guard.NotNull("Could not retrieve TestMethod from serialization", info.GetValue<string>("me"));
		TestMethodUniqueID = Guard.NotNull("Could not retrieve TestMethodUniqueID from serialization", info.GetValue<string>("meid"));

		var traits = Guard.NotNull("Could not retrieve Traits from serialization", info.GetValue<Dictionary<string, HashSet<string>>>("tr"));
		Traits = traits.ToReadOnly();

		var sourceLineNumberText = info.GetValue<string>("SourceLineNumber");
		if (sourceLineNumberText is not null)
			SourceLineNumber = int.Parse(sourceLineNumberText, CultureInfo.InvariantCulture);
	}

	void IXunitSerializable.Serialize(IXunitSerializationInfo info)
	{
		info.AddValue("id", AssemblyUniqueID);
		info.AddValue("sr", SkipReason);
		info.AddValue("sp", SourceFilePath);
		info.AddValue("sl", SourceLineNumber?.ToString(CultureInfo.InvariantCulture));
		info.AddValue("coid", TestCollectionUniqueID);
		info.AddValue("cadn", TestCaseDisplayName);
		info.AddValue("caid", TestCaseUniqueID);
		info.AddValue("cl", TestClass);
		info.AddValue("clid", TestClassUniqueID);
		info.AddValue("me", TestMethod);
		info.AddValue("meid", TestMethodUniqueID);
		info.AddValue("tr", traits.ToReadWrite(StringComparer.OrdinalIgnoreCase));
	}

	/// <summary>
	/// Converts the test case to <see cref="ITestCaseDiscovered"/>, with optional
	/// serialization of the test case.
	/// </summary>
	/// <param name="includeSerialization">A flag to indicate whether serialization is needed.</param>
	public ITestCaseDiscovered ToTestCaseDiscovered(bool includeSerialization)
	{
		var lastDotIdx = TestClass.LastIndexOf('.');
		var @namespace = lastDotIdx > -1 ? TestClass.Substring(0, lastDotIdx) : null;
		var simpleName = lastDotIdx > -1 ? TestClass.Substring(lastDotIdx + 1) : TestClass;

		return new TestCaseDiscovered
		{
			AssemblyUniqueID = AssemblyUniqueID,
			Explicit = false,
			Serialization = includeSerialization ? SerializationHelper.Instance.Serialize(this)! : string.Empty,
			SkipReason = SkipReason,
			SourceFilePath = SourceFilePath,
			SourceLineNumber = SourceLineNumber,
			TestCaseDisplayName = TestCaseDisplayName,
			TestCaseUniqueID = TestCaseUniqueID,
			TestClassMetadataToken = null,
			TestClassName = TestClass,
			TestClassNamespace = @namespace,
			TestClassSimpleName = simpleName,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodMetadataToken = null,
			TestMethodName = TestMethod,
			TestMethodParameterTypesVSTest = null,
			TestMethodReturnTypeVSTest = null,
			TestMethodUniqueID = TestMethodUniqueID,
			Traits = Traits,
		};
	}

	/// <summary>
	/// Converts the test case to <see cref="ITestCaseFinished"/>.
	/// </summary>
	public ITestCaseFinished ToTestCaseFinished(Xunit1RunSummary testCaseResults)
	{
		Guard.ArgumentNotNull(testCaseResults);

		return new TestCaseFinished()
		{
			AssemblyUniqueID = AssemblyUniqueID,
			ExecutionTime = testCaseResults.Time,
			TestCaseUniqueID = TestCaseUniqueID,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodUniqueID = TestMethodUniqueID,
			TestsFailed = testCaseResults.Failed,
			TestsNotRun = 0,
			TestsTotal = testCaseResults.Total,
			TestsSkipped = testCaseResults.Skipped,
		};
	}

	/// <summary>
	/// Converts the test case to <see cref="ITestCaseFinished"/> for a not-run test case.
	/// </summary>
	public ITestCaseFinished ToTestCaseFinishedNotRun() =>
		new TestCaseFinished()
		{
			AssemblyUniqueID = AssemblyUniqueID,
			ExecutionTime = 0m,
			TestCaseUniqueID = TestCaseUniqueID,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodUniqueID = TestMethodUniqueID,
			TestsFailed = 0,
			TestsNotRun = 1,
			TestsTotal = 1,
			TestsSkipped = 0,
		};

	/// <summary>
	/// Converts the test case to <see cref="ITestCaseStarting"/>.
	/// </summary>
	public ITestCaseStarting ToTestCaseStarting()
	{
		var lastDotIdx = TestClass.LastIndexOf('.');
		var @namespace = lastDotIdx > -1 ? TestClass.Substring(0, lastDotIdx) : null;
		var simpleName = lastDotIdx > -1 ? TestClass.Substring(lastDotIdx + 1) : TestClass;

		return new TestCaseStarting()
		{
			AssemblyUniqueID = AssemblyUniqueID,
			Explicit = false,
			SkipReason = SkipReason,
			SourceFilePath = SourceFilePath,
			SourceLineNumber = SourceLineNumber,
			TestCaseDisplayName = TestCaseDisplayName,
			TestCaseUniqueID = TestCaseUniqueID,
			TestClassMetadataToken = null,
			TestClassName = TestClass,
			TestClassNamespace = @namespace,
			TestClassSimpleName = simpleName,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodMetadataToken = null,
			TestMethodName = TestMethod,
			TestMethodParameterTypesVSTest = null,
			TestMethodReturnTypeVSTest = null,
			TestMethodUniqueID = TestMethodUniqueID,
			Traits = Traits,
		};
	}

	/// <summary>
	/// Converts the test case to <see cref="ITestFailed"/>.
	/// </summary>
	public ITestFailed ToTestFailed(
		decimal executionTime,
		string output,
		XmlNode failure,
		int currentTestIndex)
	{
		var (exceptionTypes, messages, stackTraces, exceptionParentIndices) = Xunit1ExceptionUtility.ConvertToErrorMetadata(failure);

		return new TestFailed()
		{
			AssemblyUniqueID = AssemblyUniqueID,
			Cause = FailureCause.Assertion,  // We don't know in v1, so we just assume it's an assertion failure
			ExceptionParentIndices = exceptionParentIndices,
			ExceptionTypes = exceptionTypes,
			ExecutionTime = executionTime,
			FinishTime = DateTimeOffset.UtcNow,
			Messages = messages,
			Output = output,
			StackTraces = stackTraces,
			TestCaseUniqueID = TestCaseUniqueID,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodUniqueID = TestMethodUniqueID,
			TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
			Warnings = null,
		};
	}

	/// <summary>
	/// Converts the test case to <see cref="ITestFinished"/>.
	/// </summary>
	public ITestFinished ToTestFinished(
		decimal executionTime,
		string output,
		int currentTestIndex) =>
			new TestFinished()
			{
				AssemblyUniqueID = AssemblyUniqueID,
				Attachments = EmptyAttachments,
				ExecutionTime = executionTime,
				FinishTime = DateTimeOffset.UtcNow,
				Output = output,
				TestCaseUniqueID = TestCaseUniqueID,
				TestClassUniqueID = TestClassUniqueID,
				TestCollectionUniqueID = TestCollectionUniqueID,
				TestMethodUniqueID = TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
				Warnings = null,
			};

	/// <summary>
	/// Converts the test case to <see cref="ITestFinished"/> for a not-run test.
	/// </summary>
	public ITestFinished ToTestFinishedNotRun(int currentTestIndex) =>
		new TestFinished()
		{
			AssemblyUniqueID = AssemblyUniqueID,
			Attachments = EmptyAttachments,
			ExecutionTime = 0m,
			FinishTime = DateTimeOffset.UtcNow,
			Output = "",
			TestCaseUniqueID = TestCaseUniqueID,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodUniqueID = TestMethodUniqueID,
			TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
			Warnings = null,
		};

	/// <summary>
	/// Converts the test case to <see cref="ITestMethodFinished"/>.
	/// </summary>
	public ITestMethodFinished ToTestMethodFinished(Xunit1RunSummary testMethodResults)
	{
		Guard.ArgumentNotNull(testMethodResults);

		return new TestMethodFinished()
		{
			AssemblyUniqueID = AssemblyUniqueID,
			ExecutionTime = testMethodResults.Time,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodUniqueID = TestMethodUniqueID,
			TestsFailed = testMethodResults.Failed,
			TestsNotRun = 0,
			TestsTotal = testMethodResults.Total,
			TestsSkipped = testMethodResults.Skipped
		};
	}

	/// <summary>
	/// Converts the test case to <see cref="ITestMethodFinished"/> for a not-run test.
	/// </summary>
	public ITestMethodFinished ToTestMethodFinishedNotRun() =>
		new TestMethodFinished()
		{
			AssemblyUniqueID = AssemblyUniqueID,
			ExecutionTime = 0m,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodUniqueID = TestMethodUniqueID,
			TestsFailed = 0,
			TestsNotRun = 1,
			TestsTotal = 1,
			TestsSkipped = 0,
		};

	/// <summary>
	/// Converts the test case to <see cref="ITestMethodStarting"/>.
	/// </summary>
	public ITestMethodStarting ToTestMethodStarting() =>
		new TestMethodStarting()
		{
			AssemblyUniqueID = AssemblyUniqueID,
			MethodName = TestMethod,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodUniqueID = TestMethodUniqueID,
			Traits = Xunit1.EmptyV3Traits,
		};

	/// <summary>
	/// Converts the test case to <see cref="ITestNotRun"/>.
	/// </summary>
	public ITestNotRun ToTestNotRun(int currentTestIndex) =>
		new TestNotRun()
		{
			AssemblyUniqueID = AssemblyUniqueID,
			ExecutionTime = 0m,
			FinishTime = DateTimeOffset.UtcNow,
			Output = "",
			TestCaseUniqueID = TestCaseUniqueID,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodUniqueID = TestMethodUniqueID,
			TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
			Warnings = null,
		};

	/// <summary>
	/// Converts the test case to <see cref="ITestOutput"/>.
	/// </summary>
	public ITestOutput ToTestOutput(
		string output,
		int currentTestIndex) =>
			new TestOutput()
			{
				AssemblyUniqueID = AssemblyUniqueID,
				Output = output,
				TestCaseUniqueID = TestCaseUniqueID,
				TestClassUniqueID = TestClassUniqueID,
				TestCollectionUniqueID = TestCollectionUniqueID,
				TestMethodUniqueID = TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
			};

	/// <summary>
	/// Converts the test case to <see cref="ITestPassed"/>.
	/// </summary>
	public ITestPassed ToTestPassed(
		decimal executionTime,
		string output,
		int currentTestIndex) =>
			new TestPassed()
			{
				AssemblyUniqueID = AssemblyUniqueID,
				ExecutionTime = executionTime,
				FinishTime = DateTimeOffset.UtcNow,
				Output = output,
				TestCaseUniqueID = TestCaseUniqueID,
				TestClassUniqueID = TestClassUniqueID,
				TestCollectionUniqueID = TestCollectionUniqueID,
				TestMethodUniqueID = TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
				Warnings = null,
			};

	/// <summary>
	/// Converts the test case to <see cref="ITestSkipped"/>.
	/// </summary>
	public ITestSkipped ToTestSkipped(
		string reason,
		int currentTestIndex) =>
			new TestSkipped()
			{
				AssemblyUniqueID = AssemblyUniqueID,
				ExecutionTime = 0m,
				FinishTime = DateTimeOffset.UtcNow,
				Output = "",
				Reason = reason,
				TestCaseUniqueID = TestCaseUniqueID,
				TestClassUniqueID = TestClassUniqueID,
				TestCollectionUniqueID = TestCollectionUniqueID,
				TestMethodUniqueID = TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
				Warnings = null,
			};

	/// <summary>
	/// Converts the test case to <see cref="ITestStarting"/>.
	/// </summary>
	public ITestStarting ToTestStarting(
		string testDisplayName,
		int currentTestIndex) =>
			new TestStarting()
			{
				AssemblyUniqueID = AssemblyUniqueID,
				Explicit = false,
				StartTime = DateTimeOffset.UtcNow,
				TestCaseUniqueID = TestCaseUniqueID,
				TestClassUniqueID = TestClassUniqueID,
				TestCollectionUniqueID = TestCollectionUniqueID,
				TestDisplayName = testDisplayName,
				TestMethodUniqueID = TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
				Timeout = 0,
				Traits = Xunit1.EmptyV3Traits,
			};
}

#endif
