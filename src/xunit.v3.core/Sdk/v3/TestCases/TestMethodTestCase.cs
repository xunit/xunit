using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class implementation of <see cref="_ITestCase"/> which is based on test cases being
/// related directly to test methods.
/// </summary>
public abstract class TestMethodTestCase : _ITestCase, IXunitSerializable, IAsyncDisposable
{
	readonly DisposalTracker disposalTracker = new();
	string? testCaseDisplayName;
	_ITestMethod? testMethod;
	object?[]? testMethodArguments;
	Dictionary<string, List<string>>? traits;
	string? uniqueID;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	protected TestMethodTestCase()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TestMethodTestCase"/> class.
	/// </summary>
	/// <param name="testMethod">The test method this test case belongs to.</param>
	/// <param name="testCaseDisplayName">The display name for the test case.</param>
	/// <param name="uniqueID">The unique ID for the test case.</param>
	/// <param name="skipReason">The optional reason for skipping the test.</param>
	/// <param name="traits">The optional traits list.</param>
	/// <param name="testMethodArguments">The optional arguments for the test method.</param>
	/// <param name="sourceFilePath">The optional source file in where this test case originated.</param>
	/// <param name="sourceLineNumber">The optional source line number where this test case originated.</param>
	protected TestMethodTestCase(
		_ITestMethod testMethod,
		string testCaseDisplayName,
		string uniqueID,
		string? skipReason = null,
		Dictionary<string, List<string>>? traits = null,
		object?[]? testMethodArguments = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null)
	{
		SkipReason = skipReason;
		SourceFilePath = sourceFilePath;
		SourceLineNumber = sourceLineNumber;

		this.testMethod = Guard.ArgumentNotNull(testMethod);
		this.testMethodArguments = testMethodArguments ?? Array.Empty<object?>();
		this.testCaseDisplayName = Guard.ArgumentNotNull(testCaseDisplayName);
		this.uniqueID = Guard.ArgumentNotNull(uniqueID);

		if (traits != null)
			this.traits = new Dictionary<string, List<string>>(traits, StringComparer.OrdinalIgnoreCase);
		else
			this.traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		foreach (var testMethodArgument in TestMethodArguments)
			disposalTracker.Add(testMethodArgument);
	}

	/// <inheritdoc/>
	public string? SkipReason { get; protected set; }

	/// <inheritdoc/>
	public string? SourceFilePath { get; set; }

	/// <inheritdoc/>
	public int? SourceLineNumber { get; set; }

	/// <inheritdoc/>
	public string TestCaseDisplayName =>
		testCaseDisplayName ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCaseDisplayName)} on an uninitialized '{GetType().FullName}' object");

	/// <inheritdoc/>
	public _ITestCollection TestCollection =>
		TestMethod.TestClass.TestCollection;

	/// <inheritdoc/>
	public _ITestClass TestClass =>
		TestMethod.TestClass;

	string? _ITestCaseMetadata.TestClassName =>
		TestMethod.TestClass.Class.SimpleName;

	string? _ITestCaseMetadata.TestClassNamespace =>
		TestMethod.TestClass.Class.Namespace;

	string? _ITestCaseMetadata.TestClassNameWithNamespace =>
		TestMethod.TestClass.Class.Name;

	/// <inheritdoc/>
	public _ITestMethod TestMethod =>
		testMethod ?? throw new InvalidOperationException($"Attempted to get {nameof(TestMethod)} on an uninitialized '{GetType().FullName}' object");

	/// <inheritdoc/>
	public object?[] TestMethodArguments =>
		testMethodArguments ?? throw new InvalidOperationException($"Attempted to get {nameof(TestMethodArguments)} on an uninitialized '{GetType().FullName}' object");

	string _ITestCaseMetadata.TestMethodName =>
		TestMethod.Method.Name;

	/// <inheritdoc/>
	public Dictionary<string, List<string>> Traits =>
		traits ?? throw new InvalidOperationException($"Attempted to get {nameof(Traits)} on an uninitialized '{GetType().FullName}' object");

	IReadOnlyDictionary<string, IReadOnlyList<string>> _ITestCaseMetadata.Traits =>
		Traits.ToReadOnly();

	/// <inheritdoc/>
	public virtual string UniqueID =>
		uniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(UniqueID)} on an uninitialized '{GetType().FullName}' object");

	/// <inheritdoc/>
	protected virtual void Deserialize(IXunitSerializationInfo info)
	{
		testCaseDisplayName = Guard.NotNull("Could not retrieve TestCaseDisplayName from serialization", info.GetValue<string>("dn"));
		testMethod = Guard.NotNull("Could not retrieve TestMethod from serialization", info.GetValue<_ITestMethod>("tm"));
		uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("id"));

		SkipReason = info.GetValue<string>("sr");
		SourceFilePath = info.GetValue<string>("sf");
		SourceLineNumber = info.GetValue<int?>("sl");
		testMethodArguments = info.GetValue<object[]>("tma") ?? Array.Empty<object?>();
		traits = info.GetValue<Dictionary<string, List<string>>>("tr") ?? new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		foreach (var testMethodArgument in TestMethodArguments)
			disposalTracker.Add(testMethodArgument);
	}

	void IXunitSerializable.Deserialize(IXunitSerializationInfo info) =>
		Deserialize(info);

	/// <inheritdoc/>
	public virtual ValueTask DisposeAsync() =>
		disposalTracker.DisposeAsync();

	/// <inheritdoc/>
	protected virtual void Serialize(IXunitSerializationInfo info)
	{
		info.AddValue("dn", TestCaseDisplayName);
		info.AddValue("tm", TestMethod);
		info.AddValue("id", UniqueID);

		if (SkipReason != null)
			info.AddValue("sr", SkipReason);
		if (SourceFilePath != null)
			info.AddValue("sf", SourceFilePath);
		if (SourceLineNumber.HasValue)
			info.AddValue("sl", SourceLineNumber.Value);
		if (TestMethodArguments.Length > 0)
			info.AddValue("tma", TestMethodArguments);
		if (Traits.Count > 0)
			info.AddValue("tr", Traits);
	}

	void IXunitSerializable.Serialize(IXunitSerializationInfo info) =>
		Serialize(info);
}
