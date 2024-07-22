using System;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class TestAssemblyStarting
{
	string? assemblyName;
	string? assemblyPath;
	string? testEnvironment;
	string? testFrameworkDisplayName;
	DateTimeOffset? startTime;
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;

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

	/// <inheritdoc/>
	public required int? Seed { get; set; }

	/// <inheritdoc/>
	public required DateTimeOffset StartTime
	{
		get => this.ValidateNullablePropertyValue(startTime, nameof(StartTime));
		set => startTime = value;
	}

	/// <inheritdoc/>
	public required string? TargetFramework { get; set; }

	/// <inheritdoc/>
	public required string TestEnvironment
	{
		get => this.ValidateNullablePropertyValue(testEnvironment, nameof(TestEnvironment));
		set => testEnvironment = Guard.ArgumentNotNullOrEmpty(value, nameof(TestEnvironment));
	}

	/// <inheritdoc/>
	public required string TestFrameworkDisplayName
	{
		get => this.ValidateNullablePropertyValue(testFrameworkDisplayName, nameof(TestFrameworkDisplayName));
		set => testFrameworkDisplayName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestFrameworkDisplayName));
	}

	/// <inheritdoc/>
	public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits
	{
		get => this.ValidateNullablePropertyValue(traits, nameof(Traits));
		set => traits = Guard.ArgumentNotNull(value, nameof(Traits));
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(assemblyName, nameof(AssemblyName), invalidProperties);
		ValidatePropertyIsNotNull(assemblyPath, nameof(AssemblyPath), invalidProperties);
		ValidatePropertyIsNotNull(startTime, nameof(StartTime), invalidProperties);
		ValidatePropertyIsNotNull(testEnvironment, nameof(TestEnvironment), invalidProperties);
		ValidatePropertyIsNotNull(testFrameworkDisplayName, nameof(TestFrameworkDisplayName), invalidProperties);
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);
	}
}
