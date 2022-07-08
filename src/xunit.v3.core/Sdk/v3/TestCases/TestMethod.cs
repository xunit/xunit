using System;
using System.Diagnostics;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The default implementation of <see cref="_ITestMethod"/>.
/// </summary>
[DebuggerDisplay(@"\{ class = {TestClass.Class.Name}, method = {Method.Name} \}")]
public class TestMethod : _ITestMethod, IXunitSerializable
{
	_IMethodInfo? method;
	_ITestClass? testClass;
	string? uniqueID;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public TestMethod()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TestMethod"/> class.
	/// </summary>
	/// <param name="testClass">The test class</param>
	/// <param name="method">The test method</param>
	/// <param name="uniqueID">The unique ID for the test method (only used to override default behavior in testing scenarios)</param>
	public TestMethod(
		_ITestClass testClass,
		_IMethodInfo method,
		string? uniqueID = null)
	{
		this.testClass = Guard.ArgumentNotNull(testClass);
		this.method = Guard.ArgumentNotNull(method);

		this.uniqueID = uniqueID ?? UniqueIDGenerator.ForTestMethod(testClass.UniqueID, this.method.Name);
	}

	/// <inheritdoc/>
	public _IMethodInfo Method =>
		method ?? throw new InvalidOperationException($"Attempted to get {nameof(Method)} on an uninitialized '{GetType().FullName}' object");

	/// <inheritdoc/>
	public _ITestClass TestClass =>
		testClass ?? throw new InvalidOperationException($"Attempted to get {nameof(TestClass)} on an uninitialized '{GetType().FullName}' object");

	/// <inheritdoc/>
	public string UniqueID =>
		uniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(UniqueID)} on an uninitialized '{GetType().FullName}' object");

	void IXunitSerializable.Deserialize(IXunitSerializationInfo info)
	{
		testClass = Guard.NotNull("Could not retrieve TestClass from serialization", info.GetValue<_ITestClass>("tc"));
		uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("id"));

		var methodName = Guard.NotNull("Could not retrieve MethodName from serialization", info.GetValue<string>("mn"));
		method = Guard.NotNull($"Could not find test method {methodName} on test class {testClass.Class.Name}", TestClass.Class.GetMethod(methodName, true));
	}

	void IXunitSerializable.Serialize(IXunitSerializationInfo info)
	{
		info.AddValue("mn", Method.Name);
		info.AddValue("tc", TestClass);
		info.AddValue("id", UniqueID);
	}
}
