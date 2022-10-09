namespace Xunit.Runner.v3;

/// <summary>
/// Byte sequences which represent commands issued between the runner and execution engines.
/// </summary>
public static class TcpEngineMessages
{
	/// <summary>
	/// Delineates the end of a message from one engine to the other. Guaranteed to always
	/// be a single byte.
	/// </summary>
	public static readonly byte[] EndOfMessage = new[] { (byte)'\n' };
}
