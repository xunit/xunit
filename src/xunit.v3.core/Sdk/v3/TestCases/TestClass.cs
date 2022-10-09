using System;
using System.Diagnostics;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The default implementation of <see cref="_ITestClass"/>.
/// </summary>
[DebuggerDisplay(@"\{ class = {Class.Name} \}")]
public class TestClass : _ITestClass, IXunitSerializable
{
	_ITypeInfo? @class;
	_ITestCollection? testCollection;
	string? uniqueID;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public TestClass()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TestClass"/> class.
	/// </summary>
	/// <param name="testCollection">The test collection the class belongs to</param>
	/// <param name="class">The test class</param>
	/// <param name="uniqueID">The unique ID for the test class (only used to override default behavior in testing scenarios)</param>
	public TestClass(
		_ITestCollection testCollection,
		_ITypeInfo @class,
		string? uniqueID = null)
	{
		this.@class = Guard.ArgumentNotNull(@class);
		this.testCollection = Guard.ArgumentNotNull(testCollection);
		this.uniqueID = uniqueID ?? UniqueIDGenerator.ForTestClass(TestCollection.UniqueID, Class.Name);
	}

	/// <inheritdoc/>
	public _ITypeInfo Class =>
		@class ?? throw new InvalidOperationException($"Attempted to get {nameof(Class)} on an uninitialized '{GetType().FullName}' object");

	/// <inheritdoc/>
	public _ITestCollection TestCollection =>
		testCollection ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCollection)} on an uninitialized '{GetType().FullName}' object");

	/// <inheritdoc/>
	public string UniqueID =>
		uniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(UniqueID)} on an uninitialized '{GetType().FullName}' object");

	void IXunitSerializable.Deserialize(IXunitSerializationInfo info)
	{
		testCollection = Guard.NotNull("Could not retrieve TestCollection from serialization", info.GetValue<_ITestCollection>("tc"));
		uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("id"));

		var assemblyName = Guard.NotNull("Could not retrieve ClassAssemblyName from serialization", info.GetValue<string>("can"));
		var typeName = Guard.NotNull("Could not retrieve ClassTypeName from serialization", info.GetValue<string>("ctn"));

		var type = TypeHelper.GetType(assemblyName, typeName);
		if (type == null)
			throw new InvalidOperationException($"Failed to deserialize type '{typeName}' in assembly '{assemblyName}'");

		@class = Reflector.Wrap(type);
	}

	void IXunitSerializable.Serialize(IXunitSerializationInfo info)
	{
		info.AddValue("tc", TestCollection);
		info.AddValue("can", Class.Assembly.Name);
		info.AddValue("ctn", Class.Name);
		info.AddValue("id", UniqueID);
	}
}
