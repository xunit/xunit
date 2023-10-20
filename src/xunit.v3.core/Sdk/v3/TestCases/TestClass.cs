using System;
using System.Diagnostics;
using System.Globalization;
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
		this.ValidateNullablePropertyValue(@class, nameof(Class));

	/// <inheritdoc/>
	public _ITestCollection TestCollection =>
		this.ValidateNullablePropertyValue(testCollection, nameof(TestCollection));

	/// <inheritdoc/>
	public string UniqueID =>
		this.ValidateNullablePropertyValue(uniqueID, nameof(UniqueID));

	/// <inheritdoc/>
	public void Deserialize(IXunitSerializationInfo info)
	{
		testCollection = Guard.NotNull("Could not retrieve TestCollection from serialization", info.GetValue<_ITestCollection>("tc"));
		uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("id"));

		var assemblyName = Guard.NotNull("Could not retrieve ClassAssemblyName from serialization", info.GetValue<string>("can"));
		var typeName = Guard.NotNull("Could not retrieve ClassTypeName from serialization", info.GetValue<string>("ctn"));

		var type = TypeHelper.GetType(assemblyName, typeName);
		if (type is null)
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Failed to deserialize type '{0}' in assembly '{1}'", typeName, assemblyName));

		@class = Reflector.Wrap(type);
	}

	/// <inheritdoc/>
	public void Serialize(IXunitSerializationInfo info)
	{
		info.AddValue("tc", TestCollection);
		info.AddValue("can", Class.Assembly.Name);
		info.AddValue("ctn", Class.Name);
		info.AddValue("id", UniqueID);
	}
}
