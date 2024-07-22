using System;
using System.Runtime.Versioning;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that the execution process is about to start for
/// the requested assembly.
/// </summary>
public interface ITestAssemblyStarting : ITestAssemblyMessage, IAssemblyMetadata
{
	/// <summary>
	/// Gets the seed value used for randomization. If <c>null</c>, then the test framework
	/// does not support getting or setting a randomization seed. (For stock versions of xUnit.net,
	/// support for settable randomization seeds started with v3.)
	/// </summary>
	int? Seed { get; }

	/// <summary>
	/// Gets the date and time when the test assembly execution began.
	/// </summary>
	DateTimeOffset StartTime { get; }

	/// <summary>
	/// Gets the target framework that the assembly was compiled against.
	/// Examples: ".NETFramework,Version=v4.7.2", ".NETCoreApp,Version=v6.0". This information
	/// is read from <see cref="TargetFrameworkAttribute"/> on the test assembly, which
	/// is normally auto-generated (but could be missing or empty).
	/// </summary>
	string? TargetFramework { get; }

	/// <summary>
	/// Gets a display string that describes the test execution environment.
	/// Examples: "32-bit .NET Framework 4.8.4220.0", "64-bit .NET Core 4.6.29220.03"
	/// </summary>
	string TestEnvironment { get; }

	/// <summary>
	/// Gets a display string which describes the test framework and version number.
	/// Examples: "xUnit.net v3 0.1.0-pre.15", "xUnit.net 2.4.1"
	/// </summary>
	string TestFrameworkDisplayName { get; }
}
