using System;
using System.Globalization;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// Logs diagnostic messages to the system console.
/// </summary>
public class ConsoleDiagnosticMessageSink : _IMessageSink
{
	readonly object consoleLock;
	readonly string displayNewlineReplace;
	readonly string? displayPrefixDiagnostic;
	readonly string? displayPrefixInternal;
	readonly bool noColor;

	ConsoleDiagnosticMessageSink(
		object consoleLock,
		bool noColor,
		bool showDiagnosticMessages,
		bool showInternalDiagnosticMessages,
		string? assemblyDisplayName)
	{
		Guard.ArgumentNotNull(consoleLock);

		this.consoleLock = consoleLock;
		this.noColor = noColor;

		displayPrefixDiagnostic = (showDiagnosticMessages, assemblyDisplayName, noColor) switch
		{
			(false, _, _) => null,
			(true, null, false) => "",
			(true, null, true) => "[D] ",
			(true, _, false) => string.Format(CultureInfo.InvariantCulture, "[{0}] ", assemblyDisplayName),
			(true, _, true) => string.Format(CultureInfo.InvariantCulture, "[D::{0}] ", assemblyDisplayName)
		};
		displayPrefixInternal = (showInternalDiagnosticMessages, assemblyDisplayName, noColor) switch
		{
			(false, _, _) => null,
			(true, null, false) => "",
			(true, null, true) => "[I] ",
			(true, _, false) => string.Format(CultureInfo.InvariantCulture, "[{0}] ", assemblyDisplayName),
			(true, _, true) => string.Format(CultureInfo.InvariantCulture, "[I::{0}] ", assemblyDisplayName)
		};
		displayNewlineReplace = "\n" + new string(' ', (displayPrefixDiagnostic?.Length ?? displayPrefixInternal?.Length ?? 0) + 4);
	}

	/// <inheritdoc/>
	public bool OnMessage(_MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		if (message is _DiagnosticMessage diagnosticMessage && displayPrefixDiagnostic is not null)
		{
			lock (consoleLock)
			{
				if (!noColor)
					ConsoleHelper.SetForegroundColor(ConsoleColor.Yellow);

				Console.WriteLine("    {0}{1}", displayPrefixDiagnostic, diagnosticMessage.Message.Replace("\n", displayNewlineReplace));

				if (!noColor)
					ConsoleHelper.ResetColor();
			}
		}

		if (message is _InternalDiagnosticMessage internalDiagnosticMessage && displayPrefixInternal is not null)
		{
			lock (consoleLock)
			{
				if (!noColor)
					ConsoleHelper.SetForegroundColor(ConsoleColor.DarkGray);

				Console.WriteLine("    {0}{1}", displayPrefixInternal, internalDiagnosticMessage.Message.Replace("\n", displayNewlineReplace));

				if (!noColor)
					ConsoleHelper.ResetColor();
			}
		}

		return true;
	}

	/// <summary>
	/// Tries to create a new instance of the <see cref="ConsoleDiagnosticMessageSink"/> which will display instances
	/// of <see cref="_DiagnosticMessage"/> and <see cref="_InternalDiagnosticMessage"/> to the <see cref="Console"/>.
	/// May return <c>null</c> if both <paramref name="showDiagnosticMessages"/> and <paramref name="showInternalDiagnosticMessages"/>
	/// are <c>false</c>.
	/// </summary>
	/// <param name="consoleLock">The lock object used to prevent multi-threaded code from overlapping out to the console</param>
	/// <param name="noColor">A flag to indicate that the user has asked for no color</param>
	/// <param name="showDiagnosticMessages">A flag to indicate whether diagnostic messages should be shown</param>
	/// <param name="showInternalDiagnosticMessages">A flag to indicate whether internal diagnostic messages should be shown</param>
	/// <param name="assemblyDisplayName">The optional assembly display name to delineate the messages</param>
	public static ConsoleDiagnosticMessageSink? TryCreate(
		object consoleLock,
		bool noColor,
		bool showDiagnosticMessages = false,
		bool showInternalDiagnosticMessages = false,
		string? assemblyDisplayName = null) =>
			showDiagnosticMessages || showInternalDiagnosticMessages ? new(consoleLock, noColor, showDiagnosticMessages, showInternalDiagnosticMessages, assemblyDisplayName) : null;
}
