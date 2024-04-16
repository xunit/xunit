using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that the discovery process is starting for
/// the requested assembly.
/// </summary>
[JsonTypeID("discovery-starting")]
public sealed class _DiscoveryStarting : _TestAssemblyMessage, _IAssemblyMetadata
{
	string? assemblyName;

	/// <inheritdoc/>
	public string AssemblyName
	{
		get => this.ValidateNullablePropertyValue(assemblyName, nameof(AssemblyName));
		set => assemblyName = Guard.ArgumentNotNullOrEmpty(value, nameof(AssemblyName));
	}

	/// <inheritdoc/>
	public string? AssemblyPath { get; set; }

	/// <inheritdoc/>
	public string? ConfigFilePath { get; set; }

	internal override void Serialize(JsonObjectSerializer serializer)
	{
		base.Serialize(serializer);

		serializer.SerializeAssemblyMetadata(this);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} name={1} path={2} config={3}", base.ToString(), assemblyName.Quoted(), AssemblyPath.Quoted(), ConfigFilePath.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(assemblyName, nameof(AssemblyName), invalidProperties);
	}
}
