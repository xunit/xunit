namespace Xunit.Runner.v3
{
	/// <summary>
	/// Represents the potential states for <see cref="TcpRunnerEngine"/> or <see cref="T:Xunit.Runner.v3.TcpExecutionEngine"/>
	/// </summary>
	public enum TcpEngineState
	{
		/// <summary>
		/// Engine state is currently unknown, usually only occurs before the engine reaches
		/// the <see cref="Initialized"/> state.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// The engine is initialized but has not been used yet.
		/// </summary>
		Initialized,

		/// <summary>
		/// Listening for incoming connections. Only valid for runner engines.
		/// </summary>
		Listening,

		/// <summary>
		/// Connecting. Only valid for execution engines.
		/// </summary>
		Connecting,

		/// <summary>
		/// Negotiating (sending INFO messages back and forth).
		/// </summary>
		Negotiating,

		/// <summary>
		/// Connected and ready for commands.
		/// </summary>
		Connected,

		/// <summary>
		/// In the process of disconnecting.
		/// </summary>
		Disconnecting,

		/// <summary>
		/// Disconnected (and no longer usable).
		/// </summary>
		Disconnected,
	}
}
