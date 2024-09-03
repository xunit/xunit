using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The default implementation of <see cref="IXunitTestMethod"/> for xUnit.net v3 based on <see cref="MethodInfo"/>.
/// </summary>
[DebuggerDisplay(@"\{ class = {TestClass.TestClassName}, method = {Method.Name} \}")]
public class XunitTestMethod : IXunitTestMethod, IXunitSerializable
{
	// Values that must be round-tripped in serialization
	MethodInfo? method;
	IXunitTestClass? testClass;
	object?[]? testMethodArguments;
	string? uniqueID;

	// Lazy accessors based on serialized values
	readonly Lazy<IReadOnlyCollection<IBeforeAfterTestAttribute>> beforeAfterTestAttributes;
	readonly Lazy<IReadOnlyCollection<IDataAttribute>> dataAttributes;
	readonly Lazy<IReadOnlyCollection<IFactAttribute>> factAttributes;
	readonly Lazy<IReadOnlyCollection<ParameterInfo>> parameters;
	readonly Lazy<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> traits;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public XunitTestMethod()
	{
		beforeAfterTestAttributes = new(() => ExtensibilityPointFactory.GetMethodBeforeAfterTestAttributes(Method, TestClass.BeforeAfterTestAttributes));
		dataAttributes = new(() => ExtensibilityPointFactory.GetMethodDataAttributes(Method));
		factAttributes = new(() => ExtensibilityPointFactory.GetMethodFactAttributes(Method));
#pragma warning disable IDE0200 // The lambda is necessary to prevent prematurely dereferencing the Method properly
		parameters = new(() => Method.GetParameters());
#pragma warning restore IDE0200
		traits = new(() => ExtensibilityPointFactory.GetMethodTraits(Method, TestClass.Traits));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestMethod"/> class.
	/// </summary>
	/// <param name="testClass">The test class</param>
	/// <param name="method">The test method</param>
	/// <param name="testMethodArguments">The arguments to pass to the test method</param>
	/// <param name="uniqueID">The unique ID for the test method (only used to override default behavior in testing scenarios)</param>
	public XunitTestMethod(
		IXunitTestClass testClass,
		MethodInfo method,
		object?[] testMethodArguments,
		string? uniqueID = null)
#pragma warning disable CS0618
			: this()
#pragma warning restore CS0618
	{
		this.testClass = Guard.ArgumentNotNull(testClass);
		this.method = Guard.ArgumentNotNull(method);
		this.testMethodArguments = Guard.ArgumentNotNull(testMethodArguments);
		this.uniqueID = uniqueID ?? UniqueIDGenerator.ForTestMethod(testClass.UniqueID, this.method.Name);
	}

	/// <inheritdoc/>
	public IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes =>
		beforeAfterTestAttributes.Value;

	/// <inheritdoc/>
	public IReadOnlyCollection<IDataAttribute> DataAttributes =>
		dataAttributes.Value;

	/// <inheritdoc/>
	public IReadOnlyCollection<IFactAttribute> FactAttributes =>
		factAttributes.Value;

	/// <inheritdoc/>
	public bool IsGenericMethodDefinition =>
		Method.IsGenericMethodDefinition;

	/// <inheritdoc/>
	public MethodInfo Method =>
		this.ValidateNullablePropertyValue(method, nameof(Method));

	/// <inheritdoc/>
	public string MethodName =>
		Method.Name;

	/// <inheritdoc/>
	public IReadOnlyCollection<ParameterInfo> Parameters =>
		parameters.Value;

	/// <inheritdoc/>
	public Type ReturnType =>
		Method.ReturnType;

	/// <inheritdoc/>
	public IXunitTestClass TestClass =>
		this.ValidateNullablePropertyValue(testClass, nameof(TestClass));

	ITestClass ITestMethod.TestClass => TestClass;

	/// <inheritdoc/>
	public object?[] TestMethodArguments =>
		this.ValidateNullablePropertyValue(testMethodArguments, nameof(TestMethodArguments));

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits =>
		traits.Value;

	/// <inheritdoc/>
	public string UniqueID =>
		this.ValidateNullablePropertyValue(uniqueID, nameof(UniqueID));

	/// <inheritdoc/>
	public void Deserialize(IXunitSerializationInfo info)
	{
		testClass = Guard.NotNull("Could not retrieve TestClass from serialization", info.GetValue<IXunitTestClass>("tc"));
		uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("id"));

		var className = Guard.NotNull("Could not retrieve the class name of the test method", info.GetValue<string>("cn"));
		var @class = Guard.NotNull(() => "Could not look up type " + className, TypeHelper.GetType(className));
		var methodName = Guard.NotNull("Could not retrieve MethodName from serialization", info.GetValue<string>("mn"));
		method = Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Could not find test method {0} on test class {1}", methodName, testClass.TestClassName), @class.GetMethod(methodName, XunitTestClass.MethodBindingFlags));
		testMethodArguments = Guard.NotNull("Could not retrieve TestMethodArguments from serialization", info.GetValue<object?[]>("ma"));

		var genericArguments = info.GetValue<Type[]>("ga");
		if (genericArguments is not null)
			method = method.MakeGenericMethod(genericArguments);
	}

	/// <inheritdoc/>
	public string GetDisplayName(
		string baseDisplayName,
		object?[]? testMethodArguments,
		Type[]? methodGenericTypes) =>
			Method.GetDisplayNameWithArguments(baseDisplayName, testMethodArguments, methodGenericTypes);

	/// <inheritdoc/>
	public MethodInfo MakeGenericMethod(Type[] genericTypes) =>
		Method.MakeGenericMethod(genericTypes);

	/// <inheritdoc/>
	public Type[]? ResolveGenericTypes(object?[] arguments) =>
		Method.IsGenericMethodDefinition
			? Method.ResolveGenericTypes(arguments)
			: null;

	/// <inheritdoc/>
	public object?[] ResolveMethodArguments(object?[] arguments) =>
		Method.ResolveMethodArguments(arguments);

	/// <inheritdoc/>
	public void Serialize(IXunitSerializationInfo info)
	{
		Guard.NotNull("Method does not appear to come from a reflected type", Method.ReflectedType);
		Guard.NotNull("Method's reflected type does not have an assembly qualified name", Method.ReflectedType.AssemblyQualifiedName);

		info.AddValue("cn", Method.ReflectedType.AssemblyQualifiedName);
		info.AddValue("mn", Method.Name);
		info.AddValue("tc", TestClass);
		info.AddValue("ma", TestMethodArguments);
		info.AddValue("id", UniqueID);

		if (Method.IsGenericMethod && !Method.ContainsGenericParameters)
			info.AddValue("ga", Method.GetGenericArguments());
	}
}
