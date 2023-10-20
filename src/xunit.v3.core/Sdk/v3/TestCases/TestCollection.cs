using System;
using System.Diagnostics;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The default implementation of <see cref="_ITestCollection"/>.
/// </summary>
[DebuggerDisplay(@"\{ id = {UniqueID}, display = {DisplayName} \}")]
public class TestCollection : _ITestCollection, IXunitSerializable
{
	string? displayName;
	_ITestAssembly? testAssembly;
	string? uniqueID;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public TestCollection()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TestCollection"/> class.
	/// </summary>
	/// <param name="testAssembly">The test assembly the collection belongs to</param>
	/// <param name="collectionDefinition">The optional type which contains the collection definition</param>
	/// <param name="displayName">The display name for the test collection</param>
	/// <param name="uniqueID">The unique ID for the test collection (only used to override default behavior in testing scenarios)</param>
	public TestCollection(
		_ITestAssembly testAssembly,
		_ITypeInfo? collectionDefinition,
		string displayName,
		string? uniqueID = null)
	{
		CollectionDefinition = collectionDefinition;

		this.displayName = Guard.ArgumentNotNull(displayName);
		this.testAssembly = Guard.ArgumentNotNull(testAssembly);
		this.uniqueID = uniqueID ?? UniqueIDGenerator.ForTestCollection(testAssembly.UniqueID, this.displayName, CollectionDefinition?.Name);
	}

	/// <inheritdoc/>
	public _ITypeInfo? CollectionDefinition { get; private set; }

	/// <inheritdoc/>
	public string DisplayName =>
		this.ValidateNullablePropertyValue(displayName, nameof(DisplayName));

	/// <inheritdoc/>
	public _ITestAssembly TestAssembly =>
		this.ValidateNullablePropertyValue(testAssembly, nameof(TestAssembly));

	/// <inheritdoc/>
	public string UniqueID =>
		this.ValidateNullablePropertyValue(uniqueID, nameof(UniqueID));

	/// <inheritdoc/>
	public void Deserialize(IXunitSerializationInfo info)
	{
		displayName = Guard.NotNull("Could not retrieve DisplayName from serialization", info.GetValue<string>("dn"));
		testAssembly = Guard.NotNull("Could not retrieve TestAssembly from serialization", info.GetValue<_ITestAssembly>("ta"));
		uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("id"));

		var definitionAssemblyName = info.GetValue<string>("dan");
		var definitionTypeName = info.GetValue<string>("dtn");

		if (!string.IsNullOrWhiteSpace(definitionAssemblyName) && !string.IsNullOrWhiteSpace(definitionTypeName))
		{
			var type = TypeHelper.GetType(definitionAssemblyName, definitionTypeName);
			if (type is null)
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Failed to deserialize type '{0}' in assembly '{1}'", definitionTypeName, definitionAssemblyName));

			CollectionDefinition = Reflector.Wrap(type);
		}
	}

	/// <inheritdoc/>
	public void Serialize(IXunitSerializationInfo info)
	{
		info.AddValue("dn", DisplayName);
		info.AddValue("ta", TestAssembly);
		info.AddValue("id", UniqueID);

		if (CollectionDefinition is not null)
		{
			info.AddValue("dan", CollectionDefinition.Assembly.Name);
			info.AddValue("dtn", CollectionDefinition.Name);
		}
	}
}
