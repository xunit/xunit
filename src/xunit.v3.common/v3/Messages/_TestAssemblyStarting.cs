using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Versioning;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that the execution process is about to start for
/// the requested assembly.
/// </summary>
public class _TestAssemblyStarting : _TestAssemblyMessage, _IAssemblyMetadata
{
	string? assemblyName;
	string? testEnvironment;
	string? testFrameworkDisplayName;

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

	/// <summary>
	/// Gets or sets the seed value used for randomization. If <c>null</c>, then the test framework
	/// does not support getting or setting a randomization seed. (For stock versions of xUnit.net,
	/// support for settable randomization seeds started with v3.)
	/// </summary>
	public int? Seed { get; set; }

	/// <summary>
	/// Gets or sets the date and time when the test assembly execution began.
	/// </summary>
	public DateTimeOffset StartTime { get; set; }

	/// <summary>
	/// Gets or sets the target framework that the assembly was compiled against.
	/// Examples: ".NETFramework,Version=v4.7.2", ".NETCoreApp,Version=v6.0". This information
	/// is read from <see cref="TargetFrameworkAttribute"/> on the test assembly, which
	/// is normally auto-generated (but could be missing or empty).
	/// </summary>
	public string? TargetFramework { get; set; }

	/// <summary>
	/// Gets or sets a display string that describes the test execution environment.
	/// Examples: "32-bit .NET Framework 4.8.4220.0", "64-bit .NET Core 4.6.29220.03"
	/// </summary>
	public string TestEnvironment
	{
		get => this.ValidateNullablePropertyValue(testEnvironment, nameof(TestEnvironment));
		set => testEnvironment = Guard.ArgumentNotNullOrEmpty(value, nameof(TestEnvironment));
	}

	/// <summary>
	/// Gets or sets a display string which describes the test framework and version number.
	/// Examples: "xUnit.net v3 0.1.0-pre.15", "xUnit.net 2.4.1"
	/// </summary>
	public string TestFrameworkDisplayName
	{
		get => this.ValidateNullablePropertyValue(testFrameworkDisplayName, nameof(TestFrameworkDisplayName));
		set => testFrameworkDisplayName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestFrameworkDisplayName));
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(
			CultureInfo.CurrentCulture,
			"{0} name={1} path={2} config={3}{4}",
			base.ToString(),
			assemblyName.Quoted(),
			AssemblyPath.Quoted(),
			ConfigFilePath.Quoted(),
			Seed is null ? "" : string.Format(CultureInfo.CurrentCulture, " seed={0}", Seed)
		);

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidateNullableProperty(assemblyName, nameof(AssemblyName), invalidProperties);
		ValidateNullableProperty(testEnvironment, nameof(TestEnvironment), invalidProperties);
		ValidateNullableProperty(testFrameworkDisplayName, nameof(TestFrameworkDisplayName), invalidProperties);
	}
}
