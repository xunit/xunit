using System;
using Xunit.Internal;

namespace Xunit.Runner.v3
{
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
			set => protocolVersion = Guard.ArgumentNotNullOrEmpty(nameof(ProtocolVersion), value);
		}

		/// <summary>
		/// Gets or sets the unique ID for the current test assembly.
		/// First supported protocol: <see cref="TcpEngineProtocolVersion.v1_0"/>.
		/// </summary>
		public string TestAssemblyUniqueID
		{
			get => testAssemblyUniqueID ?? throw new InvalidOperationException($"Attempted to get {nameof(TestAssemblyUniqueID)} on an uninitialized '{GetType().FullName}' object");
			set => testAssemblyUniqueID = Guard.ArgumentNotNullOrEmpty(nameof(TestAssemblyUniqueID), value);
		}

		/// <summary>
		/// Gets or sets the display name for the current test framework.
		/// First supported protocol: <see cref="TcpEngineProtocolVersion.v1_0"/>.
		/// </summary>
		public string TestFrameworkDisplayName
		{
			get => testFrameworkDisplayName ?? throw new InvalidOperationException($"Attempted to get {nameof(TestFrameworkDisplayName)} on an uninitialized '{GetType().FullName}' object");
			set => testFrameworkDisplayName = Guard.ArgumentNotNullOrEmpty(nameof(TestFrameworkDisplayName), value);
		}
	}
}
