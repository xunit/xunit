using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that the discovery process is starting for
/// the requested assembly.
/// </summary>
[JsonTypeID("discovery-starting")]
public sealed class DiscoveryStarting : TestAssemblyMessage, IAssemblyMetadata, IWritableAssemblyMetadata
{
	static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> EmptyTraits = new Dictionary<string, IReadOnlyList<string>>();

	string? assemblyName;
	string? assemblyPath;

	/// <inheritdoc/>
	public string AssemblyName
	{
		get => this.ValidateNullablePropertyValue(assemblyName, nameof(AssemblyName));
		set => assemblyName = Guard.ArgumentNotNullOrEmpty(value, nameof(AssemblyName));
	}

	/// <inheritdoc/>
	public string AssemblyPath
	{
		get => this.ValidateNullablePropertyValue(assemblyPath, nameof(AssemblyPath));
		set => assemblyPath = Guard.ArgumentNotNullOrEmpty(value, nameof(AssemblyPath));
	}

	/// <inheritdoc/>
	public string? ConfigFilePath { get; set; }

	IReadOnlyDictionary<string, IReadOnlyList<string>> IAssemblyMetadata.Traits => EmptyTraits;

	IReadOnlyDictionary<string, IReadOnlyList<string>> IWritableAssemblyMetadata.Traits { get => EmptyTraits; set { } }

	string IAssemblyMetadata.UniqueID =>
		AssemblyUniqueID;

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		root.DeserializeAssemblyMetadata(this);
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.SerializeAssemblyMetadata(this, excludeTraits: true);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} name={1} path={2} config={3}", base.ToString(), assemblyName.Quoted(), AssemblyPath.Quoted(), ConfigFilePath.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(assemblyName, nameof(AssemblyName), invalidProperties);
		ValidatePropertyIsNotNull(assemblyPath, nameof(AssemblyPath), invalidProperties);
	}
}
