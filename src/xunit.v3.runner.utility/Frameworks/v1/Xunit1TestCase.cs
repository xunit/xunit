#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v1;

/// <summary>
/// Contains the data required to serialize a test case for xUnit.net v1.
/// </summary>
public sealed class Xunit1TestCase : IXunitSerializable
{
	string? assemblyUniqueID;
	string? testCollectionUniqueID;
	string? testCaseDisplayName;
	string? testCaseUniqueID;
	string? testClass;
	string? testClassUniqueID;
	string? testMethod;
	string? testMethodUniqueID;
	IReadOnlyDictionary<string, IReadOnlyList<string>> traits = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

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
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits
	{
		get => traits;
		set
		{
			var newTraits = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
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

		var traits = Guard.NotNull("Could not retrieve Traits from serialization", info.GetValue<Dictionary<string, List<string>>>("tr"));
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
	/// Converts the test case to <see cref="_TestCaseDiscovered"/>, with optional
	/// serialization of the test case.
	/// </summary>
	/// <param name="includeSerialization">A flag to indicate whether serialization is needed.</param>
	public _TestCaseDiscovered ToTestCaseDiscovered(bool includeSerialization)
	{
		string? @namespace = null;
		string? @class;

		var namespaceIdx = TestClass.LastIndexOf('.');
		if (namespaceIdx < 0)
			@class = TestClass;
		else
		{
			@namespace = TestClass.Substring(0, namespaceIdx);
			@class = TestClass.Substring(namespaceIdx + 1);

			var innerClassIdx = @class.LastIndexOf('+');
			if (innerClassIdx >= 0)
				@class = @class.Substring(innerClassIdx + 1);
		}

		var result = new _TestCaseDiscovered
		{
			AssemblyUniqueID = AssemblyUniqueID,
			SkipReason = SkipReason,
			SourceFilePath = SourceFilePath,
			SourceLineNumber = SourceLineNumber,
			TestCaseDisplayName = TestCaseDisplayName,
			TestCaseUniqueID = TestCaseUniqueID,
			TestClassName = @class,
			TestClassNamespace = @namespace,
			TestClassNameWithNamespace = TestClass,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodName = TestMethod,
			TestMethodUniqueID = TestMethodUniqueID,
			Traits = Traits
		};

		if (includeSerialization)
			result.Serialization = SerializationHelper.Serialize(this)!;

		return result;
	}

	/// <summary>
	/// Converts the test case to <see cref="_TestCaseFinished"/>.
	/// </summary>
	public _TestCaseFinished ToTestCaseFinished(Xunit1RunSummary testCaseResults)
	{
		Guard.ArgumentNotNull(testCaseResults);

		return new()
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
	/// Converts the test case to <see cref="_TestCaseFinished"/> for a not-run test case.
	/// </summary>
	public _TestCaseFinished ToTestCaseFinishedNotRun() =>
		new()
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
	/// Converts the test case to <see cref="_TestCaseStarting"/>.
	/// </summary>
	public _TestCaseStarting ToTestCaseStarting()
	{
		var lastDotIdx = TestClass.LastIndexOf('.');
		var @namespace = lastDotIdx > -1 ? TestClass.Substring(0, lastDotIdx) : null;
		var testClassWithoutNamespace = lastDotIdx > -1 ? TestClass.Substring(lastDotIdx + 1) : TestClass;

		return new()
		{
			AssemblyUniqueID = AssemblyUniqueID,
			SkipReason = SkipReason,
			SourceFilePath = SourceFilePath,
			SourceLineNumber = SourceLineNumber,
			TestCaseDisplayName = TestCaseDisplayName,
			TestCaseUniqueID = TestCaseUniqueID,
			TestClassName = testClassWithoutNamespace,
			TestClassNamespace = @namespace,
			TestClassNameWithNamespace = TestClass,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodName = TestMethod,
			TestMethodUniqueID = TestMethodUniqueID,
			Traits = Traits,
		};
	}

	/// <summary>
	/// Converts the test case to <see cref="_TestFailed"/>.
	/// </summary>
	public _TestFailed ToTestFailed(
		decimal executionTime,
		string output,
		XmlNode failure,
		int currentTestIndex)
	{
		var (exceptionTypes, messages, stackTraces, exceptionParentIndices) = Xunit1ExceptionUtility.ConvertToErrorMetadata(failure);

		return new()
		{
			AssemblyUniqueID = AssemblyUniqueID,
			Cause = FailureCause.Assertion,  // We don't know in v1, so we just assume it's an assertion failure
			ExceptionParentIndices = exceptionParentIndices,
			ExceptionTypes = exceptionTypes,
			ExecutionTime = executionTime,
			Messages = messages,
			Output = output,
			StackTraces = stackTraces,
			TestCaseUniqueID = TestCaseUniqueID,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodUniqueID = TestMethodUniqueID,
			TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
		};
	}

	/// <summary>
	/// Converts the test case to <see cref="_TestFinished"/>.
	/// </summary>
	public _TestFinished ToTestFinished(
		decimal executionTime,
		string output,
		int currentTestIndex) =>
			new()
			{
				AssemblyUniqueID = AssemblyUniqueID,
				ExecutionTime = executionTime,
				Output = output,
				TestCaseUniqueID = TestCaseUniqueID,
				TestClassUniqueID = TestClassUniqueID,
				TestCollectionUniqueID = TestCollectionUniqueID,
				TestMethodUniqueID = TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
			};

	/// <summary>
	/// Converts the test case to <see cref="_TestFinished"/> for a not-run test.
	/// </summary>
	public _TestFinished ToTestFinishedNotRun(int currentTestIndex) =>
		new()
		{
			AssemblyUniqueID = AssemblyUniqueID,
			ExecutionTime = 0m,
			Output = "",
			TestCaseUniqueID = TestCaseUniqueID,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodUniqueID = TestMethodUniqueID,
			TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
		};

	/// <summary>
	/// Converts the test case to <see cref="_TestMethodFinished"/>.
	/// </summary>
	public _TestMethodFinished ToTestMethodFinished(Xunit1RunSummary testMethodResults)
	{
		Guard.ArgumentNotNull(testMethodResults);

		return new()
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
	/// Converts the test case to <see cref="_TestMethodFinished"/> for a not-run test.
	/// </summary>
	public _TestMethodFinished ToTestMethodFinishedNotRun() =>
		new()
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
	/// Converts the test case to <see cref="_TestMethodStarting"/>.
	/// </summary>
	public _TestMethodStarting ToTestMethodStarting() =>
		new()
		{
			AssemblyUniqueID = AssemblyUniqueID,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethod = TestMethod,
			TestMethodUniqueID = TestMethodUniqueID,
		};

	/// <summary>
	/// Converts the test case to <see cref="_TestNotRun"/>.
	/// </summary>
	public _TestNotRun ToTestNotRun(int currentTestIndex) =>
		new()
		{
			AssemblyUniqueID = AssemblyUniqueID,
			ExecutionTime = 0m,
			Output = "",
			TestCaseUniqueID = TestCaseUniqueID,
			TestClassUniqueID = TestClassUniqueID,
			TestCollectionUniqueID = TestCollectionUniqueID,
			TestMethodUniqueID = TestMethodUniqueID,
			TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
		};

	/// <summary>
	/// Converts the test case to <see cref="_TestOutput"/>.
	/// </summary>
	public _TestOutput ToTestOutput(
		string output,
		int currentTestIndex) =>
			new()
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
	/// Converts the test case to <see cref="_TestPassed"/>.
	/// </summary>
	public _TestPassed ToTestPassed(
		decimal executionTime,
		string output,
		int currentTestIndex) =>
			new()
			{
				AssemblyUniqueID = AssemblyUniqueID,
				ExecutionTime = executionTime,
				Output = output,
				TestCaseUniqueID = TestCaseUniqueID,
				TestClassUniqueID = TestClassUniqueID,
				TestCollectionUniqueID = TestCollectionUniqueID,
				TestMethodUniqueID = TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
			};

	/// <summary>
	/// Converts the test case to <see cref="_TestSkipped"/>.
	/// </summary>
	public _TestSkipped ToTestSkipped(
		string reason,
		int currentTestIndex) =>
			new()
			{
				AssemblyUniqueID = AssemblyUniqueID,
				ExecutionTime = 0m,
				Output = "",
				Reason = reason,
				TestCaseUniqueID = TestCaseUniqueID,
				TestClassUniqueID = TestClassUniqueID,
				TestCollectionUniqueID = TestCollectionUniqueID,
				TestMethodUniqueID = TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
			};

	/// <summary>
	/// Converts the test case to <see cref="_TestStarting"/>.
	/// </summary>
	public _TestStarting ToTestStarting(
		string testDisplayName,
		int currentTestIndex) =>
			new()
			{
				AssemblyUniqueID = AssemblyUniqueID,
				TestCaseUniqueID = TestCaseUniqueID,
				TestClassUniqueID = TestClassUniqueID,
				TestCollectionUniqueID = TestCollectionUniqueID,
				TestDisplayName = testDisplayName,
				TestMethodUniqueID = TestMethodUniqueID,
				TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, currentTestIndex),
			};
}

#endif
