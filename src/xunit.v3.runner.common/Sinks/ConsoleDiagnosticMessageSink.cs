using System;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Logs diagnostic messages to the system console.
	/// </summary>
	public class ConsoleDiagnosticMessageSink : _IMessageSink
	{
		readonly string? assemblyDisplayName;
		readonly object consoleLock;
		readonly ConsoleColor displayColor;
		readonly bool noColor;

		ConsoleDiagnosticMessageSink(
			object consoleLock,
			string? assemblyDisplayName,
			bool noColor,
			ConsoleColor displayColor)
		{
			Guard.ArgumentNotNull(consoleLock);

			this.consoleLock = consoleLock;
			this.assemblyDisplayName = assemblyDisplayName;
			this.noColor = noColor;
			this.displayColor = displayColor;
		}

		/// <summary>
		/// Creates a message sink for public diagnostics.
		/// </summary>
		/// <param name="consoleLock">The console lock, used to prevent console contention</param>
		/// <param name="assemblyDisplayName">The display name for the test assembly</param>
		/// <param name="showDiagnostics">A flag to indicate whether to show public diagnostics</param>
		/// <param name="noColor">A flag to indicate whether to disable color output</param>
		public static ConsoleDiagnosticMessageSink? ForDiagnostics(
			object consoleLock,
			string assemblyDisplayName,
			bool showDiagnostics,
			bool noColor) =>
				showDiagnostics ? new(consoleLock, assemblyDisplayName, noColor, ConsoleColor.Yellow) : null;

		/// <summary>
		/// Creates a message sink for internal diagnostics.
		/// </summary>
		/// <param name="consoleLock">The console lock, used to prevent console contention</param>
		/// <param name="assemblyDisplayName">The display name for the test assembly</param>
		/// <param name="showDiagnostics">A flag to indicate whether to show internal diagnostics</param>
		/// <param name="noColor">A flag to indicate whether to disable color output</param>
		public static ConsoleDiagnosticMessageSink? ForInternalDiagnostics(
			object consoleLock,
			string assemblyDisplayName,
			bool showDiagnostics,
			bool noColor) =>
				showDiagnostics ? new(consoleLock, assemblyDisplayName, noColor, ConsoleColor.DarkGray) : null;

		/// <summary>
		/// Creates a message sink for internal diagnostics.
		/// </summary>
		/// <param name="consoleLock">The console lock, used to prevent console contention</param>
		/// <param name="showDiagnostics">A flag to indicate whether to show internal diagnostics</param>
		/// <param name="noColor">A flag to indicate whether to disable color output</param>
		public static ConsoleDiagnosticMessageSink? ForInternalDiagnostics(
			object consoleLock,
			bool showDiagnostics,
			bool noColor) =>
				showDiagnostics ? new(consoleLock, null, noColor, ConsoleColor.DarkGray) : null;

		/// <inheritdoc/>
		public bool OnMessage(_MessageSinkMessage message)
		{
			Guard.ArgumentNotNull(message);

			if (message is _DiagnosticMessage diagnosticMessage)
			{
				lock (consoleLock)
				{
					if (!noColor)
						ConsoleHelper.SetForegroundColor(displayColor);

					if (assemblyDisplayName == null)
						Console.WriteLine(diagnosticMessage.Message);
					else
						Console.WriteLine($"   {assemblyDisplayName}: {diagnosticMessage.Message}");

					if (!noColor)
						ConsoleHelper.ResetColor();
				}
			}

			return true;
		}
	}
}
