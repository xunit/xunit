namespace Xunit.Runner.v3
{
	/// <summary>
	/// Extension methods for <see cref="TcpEngineState"/>.
	/// </summary>
	public static class TcpEngineStateExtensions
	{
		/// <summary>
		/// Determines if the <see cref="TcpEngineState"/> has at least reach the connected state
		/// (<see cref="TcpEngineState.Connected"/>, <see cref="TcpEngineState.Disconnected"/>,
		/// or <see cref="TcpEngineState.Disconnected"/>).
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public static bool HasReachedConnectedState(this TcpEngineState state)
		{
			switch (state)
			{
				case TcpEngineState.Connected:
				case TcpEngineState.Disconnecting:
				case TcpEngineState.Disconnected:
					return true;

				default:
					return false;
			}
		}
	}
}
