using Xunit.Internal;

namespace Xunit.Runner.v3
{
	/// <summary>
	/// Information about the <see cref="TcpRunnerEngine"/>, typically sent in JSON encoded form when sending
	/// an <see cref="TcpEngineMessages.Runner.Info"/> message.
	/// </summary>
	public class TcpRunnerEngineInfo
	{
		string protocolVersion = TcpEngineProtocolVersion.v1_0;

		/// <summary>
		/// Gets or sets the version of the runner protocol that the execution engine supports.
		/// First supported protocol: <see cref="TcpEngineProtocolVersion.v1_0"/>.
		/// </summary>
		public string ProtocolVersion
		{
			get => protocolVersion;
			set => protocolVersion = Guard.ArgumentNotNullOrEmpty(nameof(ProtocolVersion), value);
		}
	}
}
