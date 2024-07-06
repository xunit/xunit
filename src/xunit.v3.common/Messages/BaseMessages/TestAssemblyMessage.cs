using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Base message for all messages related to test assemblies.
/// </summary>
public abstract class TestAssemblyMessage : MessageSinkMessage
{
	string? assemblyUniqueID;

	/// <summary>
	/// Gets the assembly's unique ID. Can be used to correlate test messages with the appropriate
	/// assembly that they're related to.
	/// </summary>
	public string AssemblyUniqueID
	{
		get => this.ValidateNullablePropertyValue(assemblyUniqueID, nameof(AssemblyUniqueID));
		set => assemblyUniqueID = Guard.ArgumentNotNullOrEmpty(value, nameof(AssemblyUniqueID));
	}

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		assemblyUniqueID = JsonDeserializer.TryGetString(root, nameof(AssemblyUniqueID));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(AssemblyUniqueID), AssemblyUniqueID);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0}({1})", GetType().Name, assemblyUniqueID.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(assemblyUniqueID, nameof(AssemblyUniqueID), invalidProperties);
	}
}
