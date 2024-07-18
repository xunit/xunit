using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The default implementation of <see cref="IXunitTestCollection"/> for xUnit.net v3.
/// </summary>
[DebuggerDisplay(@"\{ name = {TestCollectionDisplayName}, id = {UniqueID} \}")]
public class XunitTestCollection : IXunitTestCollection, IXunitSerializable
{
	// Values that must be round-tripped in serialization
	IXunitTestAssembly? testAssembly;
	string? testCollectionDisplayName;
	string? uniqueID;

	// Lazy accessors based on serialized values
	readonly Lazy<IReadOnlyCollection<IBeforeAfterTestAttribute>> beforeAfterTestAttributes;
	readonly Lazy<IReadOnlyCollection<Type>> classFixtureTypes;
	readonly Lazy<IReadOnlyCollection<Type>> collectionFixtureTypes;
	readonly Lazy<ITestCaseOrderer?> testCaseOrderer;
	readonly Lazy<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> traits;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public XunitTestCollection()
	{
		beforeAfterTestAttributes = new(() => ExtensibilityPointFactory.GetCollectionBeforeAfterTestAttributes(CollectionDefinition, TestAssembly.BeforeAfterTestAttributes));
		classFixtureTypes = new(() => ExtensibilityPointFactory.GetCollectionClassFixtureTypes(CollectionDefinition));
		collectionFixtureTypes = new(() => ExtensibilityPointFactory.GetCollectionCollectionFixtureTypes(CollectionDefinition));
		testCaseOrderer = new(() => ExtensibilityPointFactory.GetCollectionTestCaseOrderer(CollectionDefinition));
		traits = new(() => ExtensibilityPointFactory.GetCollectionTraits(CollectionDefinition, TestAssembly.Traits));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestCollection"/> class.
	/// </summary>
	/// <param name="testAssembly">The test assembly the collection belongs to</param>
	/// <param name="collectionDefinition">The optional type which contains the collection definition</param>
	/// <param name="disableParallelization">A flag to indicate whether this test collection opts out of parallelization</param>
	/// <param name="displayName">The display name for the test collection</param>
	/// <param name="uniqueID">The unique ID for the test collection (only used to override default behavior in testing scenarios)</param>
	public XunitTestCollection(
		IXunitTestAssembly testAssembly,
		Type? collectionDefinition,
		bool disableParallelization,
		string displayName,
		string? uniqueID = null)
#pragma warning disable CS0618
			: this()
#pragma warning restore CS0618
	{
		this.testAssembly = Guard.ArgumentNotNull(testAssembly);
		CollectionDefinition = collectionDefinition;
		DisableParallelization = disableParallelization;
		testCollectionDisplayName = Guard.ArgumentNotNull(displayName);
		this.uniqueID = uniqueID ?? UniqueIDGenerator.ForTestCollection(testAssembly.UniqueID, testCollectionDisplayName, collectionDefinition?.SafeName());
	}

	/// <inheritdoc/>
	public IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes =>
		beforeAfterTestAttributes.Value;

	/// <inheritdoc/>
	public IReadOnlyCollection<Type> ClassFixtureTypes =>
		classFixtureTypes.Value;

	/// <inheritdoc/>
	public Type? CollectionDefinition { get; private set; }

	/// <inheritdoc/>
	public IReadOnlyCollection<Type> CollectionFixtureTypes =>
		collectionFixtureTypes.Value;

	/// <inheritdoc/>
	public bool DisableParallelization { get; private set; }

	/// <inheritdoc/>
	public IXunitTestAssembly TestAssembly =>
		this.ValidateNullablePropertyValue(testAssembly, nameof(TestAssembly));

	ITestAssembly ITestCollection.TestAssembly => TestAssembly;

	/// <inheritdoc/>
	public ITestCaseOrderer? TestCaseOrderer =>
		testCaseOrderer.Value;

	/// <inheritdoc/>
	public string? TestCollectionClassName =>
		CollectionDefinition?.SafeName();

	/// <inheritdoc/>
	public string TestCollectionDisplayName =>
		this.ValidateNullablePropertyValue(testCollectionDisplayName, nameof(TestCollectionDisplayName));

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits =>
		traits.Value;

	/// <inheritdoc/>
	public string UniqueID =>
		this.ValidateNullablePropertyValue(uniqueID, nameof(UniqueID));

	/// <inheritdoc/>
	public void Deserialize(IXunitSerializationInfo info)
	{
		DisableParallelization = Guard.NotNull("Could not retrieve DisableParallelization from serialization", info.GetValue<bool?>("dp"));
		testCollectionDisplayName = Guard.NotNull("Could not retrieve TestCollectionDisplayName from serialization", info.GetValue<string>("dn"));
		testAssembly = Guard.NotNull("Could not retrieve TestAssembly from serialization", info.GetValue<IXunitTestAssembly>("ta"));
		uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("id"));

		var definitionAssemblyName = info.GetValue<string>("dan");
		var definitionTypeName = info.GetValue<string>("dtn");

		if (!string.IsNullOrWhiteSpace(definitionAssemblyName) && !string.IsNullOrWhiteSpace(definitionTypeName))
			CollectionDefinition =
				TypeHelper.GetType(definitionAssemblyName, definitionTypeName)
					?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Failed to deserialize type '{0}' in assembly '{1}'", definitionTypeName, definitionAssemblyName));
	}

	/// <inheritdoc/>
	public void Serialize(IXunitSerializationInfo info)
	{
		info.AddValue("dp", DisableParallelization);
		info.AddValue("ta", TestAssembly);
		info.AddValue("dn", TestCollectionDisplayName);
		info.AddValue("id", UniqueID);

		if (CollectionDefinition is not null)
		{
			info.AddValue("dan", CollectionDefinition.Assembly.FullName);
			info.AddValue("dtn", CollectionDefinition.SafeName());
		}
	}
}
