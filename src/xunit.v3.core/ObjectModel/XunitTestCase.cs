using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="IXunitTestCase"/> for xUnit.net v3 that supports test methods decorated with
/// <see cref="FactAttribute"/>. Test methods decorated with derived attributes may use this as a base class
/// to build from.
/// </summary>
[DebuggerDisplay(@"\{ class = {TestMethod.TestClass.Class.Name}, method = {TestMethod.Method.Name}, display = {TestCaseDisplayName}, skip = {SkipReason} \}")]
public class XunitTestCase : IXunitTestCase, IXunitSerializable, IAsyncDisposable
{
	// Values that must be round-tripped in serialization
	string? testCaseDisplayName;
	IXunitTestMethod? testMethod;
	object?[]? testMethodArguments;
	Dictionary<string, HashSet<string>>? traits;
	string? uniqueID;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public XunitTestCase()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestCase"/> class.
	/// </summary>
	/// <param name="testMethod">The test method this test case belongs to.</param>
	/// <param name="testCaseDisplayName">The display name for the test case.</param>
	/// <param name="uniqueID">The unique ID for the test case.</param>
	/// <param name="explicit">Indicates whether the test case was marked as explicit.</param>
	/// <param name="skipExceptions">The value obtained from <see cref="IFactAttribute.SkipExceptions"/>.</param>
	/// <param name="skipReason">The value obtained from <see cref="IFactAttribute.Skip"/>.</param>
	/// <param name="skipType">The value obtained from <see cref="IFactAttribute.SkipType"/>.</param>
	/// <param name="skipUnless">The value obtained from <see cref="IFactAttribute.SkipUnless"/>.</param>
	/// <param name="skipWhen">The value obtained from <see cref="IFactAttribute.SkipWhen"/>.</param>
	/// <param name="traits">The optional traits list.</param>
	/// <param name="testMethodArguments">The optional arguments for the test method.</param>
	/// <param name="sourceFilePath">The optional source file in where this test case originated.</param>
	/// <param name="sourceLineNumber">The optional source line number where this test case originated.</param>
	/// <param name="timeout">The optional timeout for the test case (in milliseconds).</param>
	public XunitTestCase(
		IXunitTestMethod testMethod,
		string testCaseDisplayName,
		string uniqueID,
		bool @explicit,
		Type[]? skipExceptions = null,
		string? skipReason = null,
		Type? skipType = null,
		string? skipUnless = null,
		string? skipWhen = null,
		Dictionary<string, HashSet<string>>? traits = null,
		object?[]? testMethodArguments = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		int? timeout = null)
	{
		this.testMethod = Guard.ArgumentNotNull(testMethod);
		this.testCaseDisplayName = Guard.ArgumentNotNull(testCaseDisplayName);
		this.uniqueID = Guard.ArgumentNotNull(uniqueID);
		Explicit = @explicit;
		SkipExceptions = skipExceptions;
		SkipReason = skipReason;
		SkipType = skipType;
		SkipUnless = skipUnless;
		SkipWhen = skipWhen;
		SourceFilePath = sourceFilePath;
		SourceLineNumber = sourceLineNumber;
		Timeout = timeout ?? 0;

		this.traits = new(StringComparer.OrdinalIgnoreCase);
		if (traits is not null)
			foreach (var kvp in traits)
				this.traits.AddOrGet(kvp.Key).AddRange(kvp.Value);

		this.testMethodArguments = testMethodArguments ?? [];
		foreach (var testMethodArgument in TestMethodArguments)
			DisposalTracker.Add(testMethodArgument);
	}

	/// <summary>
	/// Used to dispose of test method arguments when the test case is disposed.
	/// </summary>
	public DisposalTracker DisposalTracker { get; } = new();

	/// <inheritdoc/>
	public bool Explicit { get; private set; }

	/// <inheritdoc/>
	public Type[]? SkipExceptions { get; private set; }

	/// <inheritdoc/>
	public string? SkipReason { get; protected set; }

	// Contractually, we don't want to return a non-null SkipReason when there is
	// a setting for SkipUnless or SkipWhen, since the contract is to get the reason
	// for a statically skipped test.
	string? ITestCaseMetadata.SkipReason =>
		SkipUnless is null && SkipWhen is null ? SkipReason : null;

	/// <inheritdoc/>
	public Type? SkipType { get; protected set; }

	/// <inheritdoc/>
	public string? SkipUnless { get; protected set; }

	/// <inheritdoc/>
	public string? SkipWhen { get; protected set; }

	/// <inheritdoc/>
	public string? SourceFilePath { get; set; }

	/// <inheritdoc/>
	public int? SourceLineNumber { get; set; }

	/// <inheritdoc/>
	public string TestCaseDisplayName =>
		this.ValidateNullablePropertyValue(testCaseDisplayName, nameof(TestCaseDisplayName));

	/// <inheritdoc/>
	public IXunitTestCollection TestCollection =>
		TestMethod.TestClass.TestCollection;

	ITestCollection ITestCase.TestCollection => TestCollection;

	/// <inheritdoc/>
	public IXunitTestClass TestClass =>
		TestMethod.TestClass;

	ITestClass ITestCase.TestClass => TestClass;

	/// <inheritdoc/>
	public int TestClassMetadataToken =>
		TestMethod.TestClass.Class.MetadataToken;

	int? ITestCaseMetadata.TestClassMetadataToken => TestClassMetadataToken;

	/// <inheritdoc/>
	public string TestClassName =>
		TestMethod.TestClass.TestClassName;

	/// <inheritdoc/>
	public string? TestClassNamespace =>
		TestMethod.TestClass.TestClassNamespace;

	/// <inheritdoc/>
	public string TestClassSimpleName =>
		TestMethod.TestClass.Class.ToSimpleName();

	/// <inheritdoc/>
	public IXunitTestMethod TestMethod =>
		this.ValidateNullablePropertyValue(testMethod, nameof(TestMethod));

	ITestMethod ITestCase.TestMethod => TestMethod;

	/// <inheritdoc/>
	public object?[] TestMethodArguments =>
		this.ValidateNullablePropertyValue(testMethodArguments, nameof(TestMethodArguments));

	/// <inheritdoc/>
	public int TestMethodMetadataToken =>
		TestMethod.Method.MetadataToken;

	int? ITestCaseMetadata.TestMethodMetadataToken => TestMethodMetadataToken;

	/// <inheritdoc/>
	public string TestMethodName =>
		TestMethod.MethodName;

	/// <inheritdoc/>
	public string[] TestMethodParameterTypesVSTest =>
		TestMethod.Parameters.Select(p => p.ParameterType.ToVSTestTypeName(TestMethod.Method, TestClass.Class)).ToArray();

	/// <inheritdoc/>
	public string TestMethodReturnTypeVSTest =>
		TestMethod.ReturnType.ToVSTestTypeName();

	/// <inheritdoc/>
	public int Timeout { get; private set; }

	/// <summary>
	/// Gets the traits associated with this test case.
	/// </summary>
	public Dictionary<string, HashSet<string>> Traits =>
		this.ValidateNullablePropertyValue(traits, nameof(Traits));

	/// <inheritdoc/>
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> ITestCaseMetadata.Traits =>
		Traits.ToReadOnly();

	/// <inheritdoc/>
	public virtual string UniqueID =>
		this.ValidateNullablePropertyValue(uniqueID, nameof(UniqueID));

	/// <remarks>
	/// By default, this method returns a single <see cref="XunitTest"/> that is appropriate
	/// for a one-to-one mapping between test and test case. Override this method to change the
	/// tests that are associated with this test case.
	/// </remarks>
	/// <inheritdoc/>
	public virtual ValueTask<IReadOnlyCollection<IXunitTest>> CreateTests() =>
		new([
			new XunitTest(
				this,
				TestMethod,
				Explicit,
				SkipReason,
				TestCaseDisplayName,
				testIndex: 0,
				Traits.ToReadOnly(),
				Timeout,
				ResolveTestMethodArguments(TestMethod.Parameters.CastOrToArray(), TestMethodArguments)
			)
		]);

	/// <summary>
	/// Called when the test case should populate itself with data from the serialization info.
	/// </summary>
	/// <param name="info">The info to get the object data from</param>
	protected virtual void Deserialize(IXunitSerializationInfo info)
	{
		testCaseDisplayName = Guard.NotNull("Could not retrieve TestCaseDisplayName from serialization", info.GetValue<string>("dn"));
		testMethod = Guard.NotNull("Could not retrieve TestMethod from serialization", info.GetValue<IXunitTestMethod>("tm"));
		uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("id"));

		SkipExceptions = info.GetValue<Type[]>("se");
		SkipReason = info.GetValue<string>("sr");
		SkipType = info.GetValue<Type>("st");
		SkipUnless = info.GetValue<string>("su");
		SkipWhen = info.GetValue<string>("sw");
		SourceFilePath = info.GetValue<string>("sf");
		SourceLineNumber = info.GetValue<int?>("sl");
		testMethodArguments = info.GetValue<object[]>("tma") ?? Array.Empty<object?>();
		traits = info.GetValue<Dictionary<string, HashSet<string>>>("tr") ?? new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

		foreach (var testMethodArgument in TestMethodArguments)
			DisposalTracker.Add(testMethodArgument);

		Explicit = info.GetValue<bool>("ex");
		Timeout = info.GetValue<int>("to");
	}

	void IXunitSerializable.Deserialize(IXunitSerializationInfo info) =>
		Deserialize(info);

	/// <inheritdoc/>
	public virtual ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		return DisposalTracker.DisposeAsync();
	}

	/// <inheritdoc/>
	public virtual void PostInvoke()
	{ }

	/// <inheritdoc/>
	public virtual void PreInvoke()
	{ }

	/// <summary>
	/// Computes values from the test case and resolves the test method arguments just before execution.
	/// Typically used from <see cref="CreateTests"/> so that the executed test has an appropriately
	/// typed argument, regardless of the type that was used to serialize the argument.
	/// </summary>
	protected static object?[] ResolveTestMethodArguments(
		ParameterInfo[] parameters,
		object?[] arguments)
	{
		Guard.ArgumentNotNull(parameters);
		Guard.ArgumentNotNull(arguments);

		var parameterTypes = new Type[parameters.Length];
		for (var i = 0; i < parameters.Length; i++)
			parameterTypes[i] = parameters[i].ParameterType;

		return TypeHelper.ConvertArguments(arguments, parameterTypes);
	}

	/// <summary>
	/// Called when the test case should store its serialized values into the serialization info.
	/// </summary>
	/// <param name="info">The info to store the object data into</param>
	protected virtual void Serialize(IXunitSerializationInfo info)
	{
		info.AddValue("dn", TestCaseDisplayName);
		info.AddValue("tm", TestMethod);
		info.AddValue("id", UniqueID);

		if (SkipExceptions is not null)
			info.AddValue("se", SkipExceptions);
		if (SkipReason is not null)
			info.AddValue("sr", SkipReason);
		if (SkipType is not null)
			info.AddValue("st", SkipType);
		if (SkipUnless is not null)
			info.AddValue("su", SkipUnless);
		if (SkipWhen is not null)
			info.AddValue("sw", SkipWhen);
		if (SourceFilePath is not null)
			info.AddValue("sf", SourceFilePath);
		if (SourceLineNumber.HasValue)
			info.AddValue("sl", SourceLineNumber.Value);
		if (TestMethodArguments.Length > 0)
			info.AddValue("tma", TestMethodArguments);
		if (Traits.Count > 0)
			info.AddValue("tr", Traits);

		info.AddValue("ex", Explicit);
		info.AddValue("to", Timeout);
	}

	void IXunitSerializable.Serialize(IXunitSerializationInfo info) =>
		Serialize(info);
}
