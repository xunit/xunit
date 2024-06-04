using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// Base message for all messages related to test assemblies.
/// </summary>
public abstract class _TestAssemblyMessage : _MessageSinkMessage
{
	string? assemblyUniqueID;

	/// <summary>
	/// Gets the assembly's unique ID. Can be used to correlate test messages with the appropriate
	/// assembly that they're related to. Assembly metadata (as <see cref="_IAssemblyMetadata"/>) is
	/// provided via <see cref="_DiscoveryStarting"/> (during discovery) and/or
	/// <see cref="_TestAssemblyStarting"/> (during execution) and should be cached as needed.
	/// </summary>
	public string AssemblyUniqueID
	{
		get => this.ValidateNullablePropertyValue(assemblyUniqueID, nameof(AssemblyUniqueID));
		set => assemblyUniqueID = Guard.ArgumentNotNullOrEmpty(value, nameof(AssemblyUniqueID));
	}

	internal override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		assemblyUniqueID = TryGetString(root, nameof(AssemblyUniqueID));
	}

	internal override void Serialize(JsonObjectSerializer serializer)
	{
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

