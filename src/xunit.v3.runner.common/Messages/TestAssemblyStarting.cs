using System;
using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public partial class TestAssemblyStarting
{
	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="MessageSinkMessage.UnsetStringPropertyValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required string AssemblyName { get; set; } = UnsetStringPropertyValue;

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="MessageSinkMessage.UnsetStringPropertyValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required string AssemblyPath { get; set; } = UnsetStringPropertyValue;

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required string? ConfigFilePath { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required int? Seed { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="DateTimeOffset.MinValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required DateTimeOffset StartTime { get; set; } = DateTimeOffset.MinValue;

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required string? TargetFramework { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="MessageSinkMessage.UnsetStringPropertyValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required string TestEnvironment { get; set; } = UnsetStringPropertyValue;

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="MessageSinkMessage.UnsetStringPropertyValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required string TestFrameworkDisplayName { get; set; } = UnsetStringPropertyValue;

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be an empty dictionary if there was no value provided during deserialization.
	/// </remarks>
	public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; } = EmptyTraits;

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		AssemblyName = JsonDeserializer.TryGetString(root, nameof(AssemblyName)) ?? AssemblyName;
		AssemblyPath = JsonDeserializer.TryGetString(root, nameof(AssemblyPath)) ?? AssemblyPath;
		ConfigFilePath = JsonDeserializer.TryGetString(root, nameof(ConfigFilePath));
		Seed = JsonDeserializer.TryGetInt(root, nameof(Seed));
		StartTime = JsonDeserializer.TryGetDateTimeOffset(root, nameof(StartTime)) ?? StartTime;
		TargetFramework = JsonDeserializer.TryGetString(root, nameof(TargetFramework)) ?? TargetFramework;
		TestEnvironment = JsonDeserializer.TryGetString(root, nameof(TestEnvironment)) ?? TestEnvironment;
		TestFrameworkDisplayName = JsonDeserializer.TryGetString(root, nameof(TestFrameworkDisplayName)) ?? TestFrameworkDisplayName;
		Traits = JsonDeserializer.TryGetTraits(root, nameof(Traits)) ?? Traits;
	}
}
