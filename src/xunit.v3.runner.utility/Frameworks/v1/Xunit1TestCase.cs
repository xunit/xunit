#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v1;

/// <summary>
/// Contains the data required to serialize a test case for xUnit.net v1.
/// </summary>
public class Xunit1TestCase : IXunitSerializable
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
		get => assemblyUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(AssemblyUniqueID)} on an uninitialized '{GetType().FullName}' object");
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
		get => testCollectionUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCollectionUniqueID)} on an uninitialized '{GetType().FullName}' object");
		set => testCollectionUniqueID = Guard.ArgumentNotNull(value, nameof(TestCollectionUniqueID));
	}

	/// <summary>
	/// Gets the display name for the test case.
	/// </summary>
	public string TestCaseDisplayName
	{
		get => testCaseDisplayName ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCaseDisplayName)} on an uninitialized '{GetType().FullName}' object");
		set => testCaseDisplayName = Guard.ArgumentNotNull(value, nameof(TestCaseDisplayName));
	}

	/// <summary>
	/// Gets the unique ID for the test case.
	/// </summary>
	public string TestCaseUniqueID
	{
		get => testCaseUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCaseUniqueID)} on an uninitialized '{GetType().FullName}' object");
		set => testCaseUniqueID = Guard.ArgumentNotNull(value, nameof(TestCaseUniqueID));
	}

	/// <summary>
	/// Gets the fully qualified type name of the test class.
	/// </summary>
	public string TestClass
	{
		get => testClass ?? throw new InvalidOperationException($"Attempted to get {nameof(TestClass)} on an uninitialized '{GetType().FullName}' object");
		set => testClass = Guard.ArgumentNotNull(value, nameof(TestClass));
	}

	/// <summary>
	/// Gets the unique ID for the test class.
	/// </summary>
	public string TestClassUniqueID
	{
		get => testClassUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(TestClassUniqueID)} on an uninitialized '{GetType().FullName}' object");
		set => testClassUniqueID = Guard.ArgumentNotNull(value, nameof(TestClassUniqueID));
	}

	/// <summary>
	/// Gets the name of the test method.
	/// </summary>
	public string TestMethod
	{
		get => testMethod ?? throw new InvalidOperationException($"Attempted to get {nameof(TestMethod)} on an uninitialized '{GetType().FullName}' object");
		set => testMethod = Guard.ArgumentNotNull(value, nameof(TestMethod));
	}

	/// <summary>
	/// Gets the unique ID of the test method.
	/// </summary>
	public string TestMethodUniqueID
	{
		get => testMethodUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(TestMethodUniqueID)} on an uninitialized '{GetType().FullName}' object");
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
			if (value != null)
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
		if (sourceLineNumberText != null)
			SourceLineNumber = int.Parse(sourceLineNumberText);
	}

	void IXunitSerializable.Serialize(IXunitSerializationInfo info)
	{
		info.AddValue("id", AssemblyUniqueID);
		info.AddValue("sr", SkipReason);
		info.AddValue("sp", SourceFilePath);
		info.AddValue("sl", SourceLineNumber?.ToString());
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
	/// <returns>The converted test case</returns>
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
}

#endif
