namespace Xunit.Runner.v3
{
	/// <summary>
	/// Definitions of the protocol version, and description of the behavior they encompass
	/// </summary>
	public static class TcpEngineProtocolVersion
	{
		/// <summary>
		/// Represents engine protocol "1.0", the first supported protocol version.
		/// (1) Runner protocol commands:
		///   <see cref="TcpEngineMessages.Runner.Cancel"/>,
		///   <see cref="TcpEngineMessages.Runner.Find"/>,
		///   <see cref="TcpEngineMessages.Runner.Info"/>,
		///   <see cref="TcpEngineMessages.Runner.Quit"/>,
		///   <see cref="TcpEngineMessages.Runner.Run"/>;
		/// (2) Runner protocol INFO message fields:
		///   <see cref="TcpRunnerEngineInfo.ProtocolVersion"/>;
		/// (3) Engine protocol commands:
		///   <see cref="TcpEngineMessages.Execution.Info"/>,
		///   <see cref="TcpEngineMessages.Execution.Message"/>;
		/// (4) Engine protocol INFO meesage fields:
		///   <see cref="TcpExecutionEngineInfo.ProtocolVersion"/>,
		///   <see cref="TcpExecutionEngineInfo.TestAssemblyUniqueID"/>,
		///   <see cref="TcpExecutionEngineInfo.TestFrameworkDisplayName"/>.
		/// </summary>
		public const string v1_0 = "1.0";
	}
}
