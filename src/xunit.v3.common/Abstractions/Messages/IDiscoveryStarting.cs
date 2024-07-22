namespace Xunit.Sdk;

/// <summary>
/// This message indicates that the discovery process is starting for
/// the requested assembly.
/// </summary>
public interface IDiscoveryStarting : ITestAssemblyMessage
{
	/// <summary>
	/// Gets the assembly name. May return a simple assembly name (i.e., "mscorlib"), or may return a
	/// fully qualified name (i.e., "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").
	/// </summary>
	string AssemblyName { get; }

	/// <summary>
	/// Gets the on-disk location of the assembly under test.
	/// </summary>
	string AssemblyPath { get; }

	/// <summary>
	/// Gets the full path of the configuration file name, if one is present.
	/// May be <c>null</c> if there is no configuration file.
	/// </summary>
	string? ConfigFilePath { get; }
}
