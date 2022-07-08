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
	string? displayName;
	readonly DisposalTracker disposalTracker = new();
	DisplayNameFormatter? formatter;
	_IMethodInfo? method;
	_ITestMethod? testMethod;
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
	/// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
	/// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
	/// <param name="testMethod">The test method this test case belongs to.</param>
	/// <param name="testMethodArguments">The optional arguments for the test method.</param>
	/// <param name="skipReason">The optional reason for skipping the test.</param>
	/// <param name="traits">The optional traits list.</param>
	/// <param name="uniqueID">The optional unique ID for the test case; if not provided, will be calculated.</param>
	/// <param name="displayName">The optional display name for the test</param>
	protected TestMethodTestCase(
		TestMethodDisplay defaultMethodDisplay,
		TestMethodDisplayOptions defaultMethodDisplayOptions,
		_ITestMethod testMethod,
		object?[]? testMethodArguments = null,
		string? skipReason = null,
		Dictionary<string, List<string>>? traits = null,
		string? uniqueID = null,
		string? displayName = null)
	{
		DefaultMethodDisplay = defaultMethodDisplay;
		DefaultMethodDisplayOptions = defaultMethodDisplayOptions;
		SkipReason = skipReason;
		this.testMethod = Guard.ArgumentNotNull(testMethod);
		TestMethodArguments = testMethodArguments;

		if (traits != null)
			this.traits = new Dictionary<string, List<string>>(traits, StringComparer.OrdinalIgnoreCase);
		else
			this.traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		formatter = new DisplayNameFormatter(defaultMethodDisplay, defaultMethodDisplayOptions);

		var baseDisplayName = displayName ?? BaseDisplayName;
		var initResults = Initialize(baseDisplayName, testMethod, TestMethodArguments);

		this.displayName = initResults.displayName;
		InitializationException = initResults.initException;
		method = initResults.method;
		MethodGenericTypes = initResults.methodGenericTypes;
		this.uniqueID = uniqueID ?? UniqueIDGenerator.ForTestCase(TestMethod.UniqueID, MethodGenericTypes, TestMethodArguments);

		if (TestMethodArguments != null)
			foreach (var testMethodArgument in TestMethodArguments)
				disposalTracker.Add(testMethodArgument);
	}

	static (string displayName, Exception? initException, _IMethodInfo method, _ITypeInfo[]? methodGenericTypes) Initialize(
		string baseDisplayName,
		_ITestMethod testMethod,
		object?[]? testMethodArguments)
	{
		string? displayName = null;
		Exception? initException = null;
		_ITypeInfo[]? methodGenericTypes = null;

		var method = testMethod.Method;

		if (testMethodArguments != null)
		{
			if (method is _IReflectionMethodInfo reflectionMethod)
			{
				try
				{
					testMethodArguments = reflectionMethod.MethodInfo.ResolveMethodArguments(testMethodArguments);
				}
				catch (Exception ex)
				{
					initException = ex;
					testMethodArguments = null;
					displayName = $"{baseDisplayName}(???)";
				}
			}
		}

		if (testMethodArguments != null && method.IsGenericMethodDefinition)
		{
			methodGenericTypes = method.ResolveGenericTypes(testMethodArguments);
			method = method.MakeGenericMethod(methodGenericTypes);
		}

		if (displayName == null)
			displayName = method.GetDisplayNameWithArguments(baseDisplayName, testMethodArguments, methodGenericTypes);

		return (displayName, initException, method, methodGenericTypes);
	}

	/// <summary>
	/// Returns the base display name for a test; the actual value depends on <see cref="DefaultMethodDisplay"/>.
	/// "TestClassName.MethodName" for <see cref="TestMethodDisplay.ClassAndMethod"/>, or "MethodName"
	/// for <see cref="TestMethodDisplay.Method"/>.
	/// </summary>
	protected string BaseDisplayName
	{
		get
		{
			if (DefaultMethodDisplay == TestMethodDisplay.ClassAndMethod)
				return Formatter.Format($"{TestMethod.TestClass.Class.Name}.{TestMethod.Method.Name}");

			return Formatter.Format(TestMethod.Method.Name);
		}
	}

	/// <summary>
	/// Returns the default method display to use (when not customized).
	/// </summary>
	protected internal TestMethodDisplay DefaultMethodDisplay { get; private set; }

	/// <summary>
	/// Returns the default method display options to use (when not customized).
	/// </summary>
	protected internal TestMethodDisplayOptions DefaultMethodDisplayOptions { get; private set; }

	/// <summary>
	/// Returns the display name formatter used to format the display name.
	/// </summary>
	protected DisplayNameFormatter Formatter =>
		formatter ?? throw new InvalidOperationException($"Attempted to get {nameof(Formatter)} on an uninitialized '{GetType().FullName}' object");

	/// <summary>
	/// Gets or sets the exception that happened during initialization. When this is set, then
	/// the test execution should fail with this exception.
	/// </summary>
	public Exception? InitializationException { get; private set; }

	/// <inheritdoc/>
	public _IMethodInfo Method =>
		method ?? throw new InvalidOperationException($"Attempted to get {nameof(Method)} on an uninitialized '{GetType().FullName}' object");

	/// <summary>
	/// Gets the generic types that were used to close the generic test method, if
	/// applicable; <c>null</c>, if the test method was not an open generic.
	/// </summary>
	protected _ITypeInfo[]? MethodGenericTypes { get; private set; }

	/// <inheritdoc/>
	public string? SkipReason { get; protected set; }

	/// <inheritdoc/>
	public string? SourceFilePath { get; set; }

	/// <inheritdoc/>
	public int? SourceLineNumber { get; set; }

	/// <inheritdoc/>
	public string TestCaseDisplayName
	{
		get => displayName ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCaseDisplayName)} on an uninitialized '{GetType().FullName}' object");
		protected set => displayName = Guard.ArgumentNotNull(value, nameof(TestCaseDisplayName));
	}

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

	string _ITestCaseMetadata.TestMethodName =>
		TestMethod.Method.Name;

	/// <inheritdoc/>
	public object?[]? TestMethodArguments { get; private set; }

	/// <inheritdoc/>
	public Dictionary<string, List<string>> Traits =>
		traits ?? throw new InvalidOperationException($"Attempted to get {nameof(Traits)} on an uninitialized '{GetType().FullName}' object");

	IReadOnlyDictionary<string, IReadOnlyList<string>> _ITestCaseMetadata.Traits =>
		Traits.ToReadOnly();

	/// <inheritdoc/>
	public virtual string UniqueID
	{
		get => uniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(UniqueID)} on an uninitialized '{GetType().FullName}' object");
		protected set => uniqueID = Guard.ArgumentNotNull(value, nameof(UniqueID));
	}

	/// <inheritdoc/>
	protected virtual void Deserialize(IXunitSerializationInfo info)
	{
		DefaultMethodDisplay = info.GetValue<TestMethodDisplay>("dmd");
		DefaultMethodDisplayOptions = info.GetValue<TestMethodDisplayOptions>("dmo");
		displayName = Guard.NotNull("Could not retrieve DisplayName from serialization", info.GetValue<string>("dn"));
		SkipReason = info.GetValue<string>("sr");
		testMethod = Guard.NotNull("Could not retrieve TestMethod from serialization", info.GetValue<_ITestMethod>("tm"));
		TestMethodArguments = info.GetValue<object[]>("tma");
		traits = info.GetValue<Dictionary<string, List<string>>>("tr") ?? new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
		uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("id"));

		formatter = new DisplayNameFormatter(DefaultMethodDisplay, DefaultMethodDisplayOptions);

		var initResults = Initialize(BaseDisplayName, TestMethod, TestMethodArguments);

		InitializationException = initResults.initException;
		method = initResults.method;
		MethodGenericTypes = initResults.methodGenericTypes;

		if (TestMethodArguments != null)
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
		info.AddValue("dmd", DefaultMethodDisplay);
		info.AddValue("dmo", DefaultMethodDisplayOptions);
		info.AddValue("dn", TestCaseDisplayName);
		info.AddValue("sr", SkipReason);
		info.AddValue("tm", TestMethod);
		info.AddValue("tma", TestMethodArguments);
		info.AddValue("tr", Traits);
		info.AddValue("id", UniqueID);
	}

	void IXunitSerializable.Serialize(IXunitSerializationInfo info) =>
		Serialize(info);
}
