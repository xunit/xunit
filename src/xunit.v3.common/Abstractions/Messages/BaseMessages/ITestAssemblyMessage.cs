namespace Xunit.Sdk;

/// <summary>
/// Base interface for all messages related to test assemblies.
/// </summary>
public interface ITestAssemblyMessage : IMessageSinkMessage
{
	/// <summary>
	/// Gets the assembly's unique ID. Can be used to correlate test messages with the appropriate
	/// assembly that they're related to.
	/// </summary>
	string AssemblyUniqueID { get; }
}
