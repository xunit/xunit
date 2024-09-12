#pragma warning disable CA2002  // The console writer is not cross app-domain

using System;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Logs diagnostic messages to the system console.
/// </summary>
public class ConsoleDiagnosticMessageSink : IMessageSink
{
	readonly ConsoleHelper consoleHelper;
	readonly string displayNewlineReplace;
	readonly string? displayPrefixDiagnostic;
	readonly string? displayPrefixInternal;
	readonly string indent;
	readonly bool noColor;

	ConsoleDiagnosticMessageSink(
		ConsoleHelper consoleHelper,
		bool noColor,
		bool showDiagnosticMessages,
		bool showInternalDiagnosticMessages,
		string? assemblyDisplayName,
		bool indent)
	{
		this.consoleHelper = Guard.ArgumentNotNull(consoleHelper);
		this.noColor = noColor;
		this.indent = indent ? "    " : string.Empty;

		displayPrefixDiagnostic = (showDiagnosticMessages, assemblyDisplayName, noColor) switch
		{
			(false, _, _) => null,
			(true, null, false) => "",
			(true, null, true) => "[D] ",
			(true, _, false) => string.Format(CultureInfo.InvariantCulture, "[{0}] ", assemblyDisplayName),
			(true, _, true) => string.Format(CultureInfo.InvariantCulture, "[D::{0}] ", assemblyDisplayName),
		};
		displayPrefixInternal = (showInternalDiagnosticMessages, assemblyDisplayName, noColor) switch
		{
			(false, _, _) => null,
			(true, null, false) => "",
			(true, null, true) => "[I] ",
			(true, _, false) => string.Format(CultureInfo.InvariantCulture, "[{0}] ", assemblyDisplayName),
			(true, _, true) => string.Format(CultureInfo.InvariantCulture, "[I::{0}] ", assemblyDisplayName),
		};

		displayNewlineReplace = "\n" + new string(' ', (displayPrefixDiagnostic?.Length ?? displayPrefixInternal?.Length ?? 0) + 4);
	}

	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		if (message is IDiagnosticMessage diagnosticMessage && displayPrefixDiagnostic is not null)
		{
			lock (consoleHelper.LockObject)
			{
				if (!noColor)
					consoleHelper.SetForegroundColor(ConsoleColor.Yellow);

				consoleHelper.WriteLine("{0}{1}{2}", indent, displayPrefixDiagnostic, diagnosticMessage.Message.Replace("\n", displayNewlineReplace));

				if (!noColor)
					consoleHelper.ResetColor();
			}
		}

		if (message is IInternalDiagnosticMessage internalDiagnosticMessage && displayPrefixInternal is not null)
		{
			lock (consoleHelper.LockObject)
			{
				if (!noColor)
					consoleHelper.SetForegroundColor(ConsoleColor.DarkGray);

				consoleHelper.WriteLine("{0}{1}{2}", indent, displayPrefixInternal, internalDiagnosticMessage.Message.Replace("\n", displayNewlineReplace));

				if (!noColor)
					consoleHelper.ResetColor();
			}
		}

		return true;
	}

	/// <summary>
	/// Tries to create a new instance of the <see cref="ConsoleDiagnosticMessageSink"/> which will display instances
	/// of <see cref="IDiagnosticMessage"/> and <see cref="IInternalDiagnosticMessage"/> to the <see cref="Console"/>.
	/// May return <c>null</c> if both <paramref name="showDiagnosticMessages"/> and <paramref name="showInternalDiagnosticMessages"/>
	/// are <c>false</c>.
	/// </summary>
	/// <param name="consoleHelper">The helper used to write console messages</param>
	/// <param name="noColor">A flag to indicate that the user has asked for no color</param>
	/// <param name="showDiagnosticMessages">A flag to indicate whether diagnostic messages should be shown</param>
	/// <param name="showInternalDiagnosticMessages">A flag to indicate whether internal diagnostic messages should be shown</param>
	/// <param name="assemblyDisplayName">The optional assembly display name to delineate the messages</param>
	/// <param name="indent">Whether to indent the message</param>
	public static ConsoleDiagnosticMessageSink? TryCreate(
		ConsoleHelper consoleHelper,
		bool noColor,
		bool showDiagnosticMessages = false,
		bool showInternalDiagnosticMessages = false,
		string? assemblyDisplayName = null,
		bool indent = true) =>
			showDiagnosticMessages || showInternalDiagnosticMessages
				? new(consoleHelper, noColor, showDiagnosticMessages, showInternalDiagnosticMessages, assemblyDisplayName, indent)
				: null;
}
