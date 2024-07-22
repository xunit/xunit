using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class TestAssemblyMessage
{
	string? assemblyUniqueID;

	/// <inheritdoc/>
	public required string AssemblyUniqueID
	{
		get => this.ValidateNullablePropertyValue(assemblyUniqueID, nameof(AssemblyUniqueID));
		set => assemblyUniqueID = Guard.ArgumentNotNullOrEmpty(value, nameof(AssemblyUniqueID));
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties) =>
		ValidatePropertyIsNotNull(assemblyUniqueID, nameof(AssemblyUniqueID), invalidProperties);
}
