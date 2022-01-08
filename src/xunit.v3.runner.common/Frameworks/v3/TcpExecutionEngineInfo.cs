using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.v3;

/// <summary>
/// Information about the <see cref="T:Xunit.Runner.v3.TcpExecutionEngine"/>, typically sent in JSON encoded
/// form when sending an <see cref="TcpEngineMessages.Execution.Info"/> message.
/// </summary>
public class TcpExecutionEngineInfo
{
	string protocolVersion = TcpEngineProtocolVersion.v1_0;
	string? testAssemblyUniqueID;
	string? testFrameworkDisplayName;

	/// <summary>
	/// Gets or sets the version of the runner protocol that the execution engine supports.
	/// First supported protocol: <see cref="TcpEngineProtocolVersion.v1_0"/>.
	/// </summary>
	public string ProtocolVersion
	{
		get => protocolVersion;
		set => protocolVersion = Guard.ArgumentNotNullOrEmpty(value, nameof(ProtocolVersion));
	}

	/// <summary>
	/// Gets or sets the unique ID for the current test assembly.
	/// First supported protocol: <see cref="TcpEngineProtocolVersion.v1_0"/>.
	/// </summary>
	public string TestAssemblyUniqueID
	{
		get => testAssemblyUniqueID ?? throw new UnsetPropertyException(nameof(TestAssemblyUniqueID), GetType());
		set => testAssemblyUniqueID = Guard.ArgumentNotNullOrEmpty(value, nameof(TestAssemblyUniqueID));
	}

	/// <summary>
	/// Gets or sets the display name for the current test framework.
	/// First supported protocol: <see cref="TcpEngineProtocolVersion.v1_0"/>.
	/// </summary>
	public string TestFrameworkDisplayName
	{
		get => testFrameworkDisplayName ?? throw new UnsetPropertyException(nameof(TestFrameworkDisplayName), GetType());
		set => testFrameworkDisplayName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestFrameworkDisplayName));
	}
}
