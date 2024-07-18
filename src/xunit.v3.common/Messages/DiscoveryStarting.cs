using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that the discovery process is starting for
/// the requested assembly.
/// </summary>
[JsonTypeID("discovery-starting")]
public sealed class DiscoveryStarting : TestAssemblyMessage, IAssemblyMetadata
{
	static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> EmptyTraits = new Dictionary<string, IReadOnlyCollection<string>>();

	string? assemblyName;
	string? assemblyPath;

	/// <inheritdoc/>
	public required string AssemblyName
	{
		get => this.ValidateNullablePropertyValue(assemblyName, nameof(AssemblyName));
		set => assemblyName = Guard.ArgumentNotNullOrEmpty(value, nameof(AssemblyName));
	}

	/// <inheritdoc/>
	public required string AssemblyPath
	{
		get => this.ValidateNullablePropertyValue(assemblyPath, nameof(AssemblyPath));
		set => assemblyPath = Guard.ArgumentNotNullOrEmpty(value, nameof(AssemblyPath));
	}

	/// <inheritdoc/>
	public required string? ConfigFilePath { get; set; }

	IReadOnlyDictionary<string, IReadOnlyCollection<string>> IAssemblyMetadata.Traits => EmptyTraits;

	string IAssemblyMetadata.UniqueID =>
		AssemblyUniqueID;

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		assemblyName = JsonDeserializer.TryGetString(root, nameof(IAssemblyMetadata.AssemblyName));
		assemblyPath = JsonDeserializer.TryGetString(root, nameof(IAssemblyMetadata.AssemblyPath));
		ConfigFilePath = JsonDeserializer.TryGetString(root, nameof(IAssemblyMetadata.ConfigFilePath));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(IAssemblyMetadata.AssemblyName), AssemblyName);
		serializer.Serialize(nameof(IAssemblyMetadata.AssemblyPath), AssemblyPath);
		serializer.Serialize(nameof(IAssemblyMetadata.ConfigFilePath), ConfigFilePath);
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
