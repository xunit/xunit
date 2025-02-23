using System.Globalization;
using Microsoft.Build.Utilities;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.MSBuild;

/// <summary/>
internal class MSBuildDiagnosticMessageSink : TestMessageSink
{
	MSBuildDiagnosticMessageSink()
	{ }

	/// <summary>
	/// Tries to create a new instance of the <see cref="MSBuildDiagnosticMessageSink"/> which will display instances
	/// of <see cref="IDiagnosticMessage"/> and <see cref="IInternalDiagnosticMessage"/> to the <paramref name="log"/>.
	/// May return <c>null</c> if both <paramref name="showDiagnosticMessages"/> and <paramref name="showInternalDiagnosticMessages"/>
	/// are <c>false</c>.
	/// </summary>
	/// <param name="log">The MSBuild task logging helper</param>
	/// <param name="logLock">The lock object used to prevent multi-threaded code from overlapping out to the log</param>
	/// <param name="showDiagnosticMessages">A flag to indicate whether diagnostic messages should be shown</param>
	/// <param name="showInternalDiagnosticMessages">A flag to indicate whether internal diagnostic messages should be shown</param>
	/// <param name="assemblyDisplayName">The optional assembly display name to delineate the messages</param>
	public static MSBuildDiagnosticMessageSink? TryCreate(
		TaskLoggingHelper log,
		object logLock,
		bool showDiagnosticMessages = false,
		bool showInternalDiagnosticMessages = false,
		string? assemblyDisplayName = null)
	{
		if (!showDiagnosticMessages && !showInternalDiagnosticMessages)
			return null;

		var result = new MSBuildDiagnosticMessageSink();
		var prefix = assemblyDisplayName is null ? "" : string.Format(CultureInfo.CurrentCulture, "[{0}] ", assemblyDisplayName);
		var indent = new string(' ', prefix.Length);

		if (showDiagnosticMessages)
			result.Diagnostics.DiagnosticMessageEvent += args =>
			{
				lock (logLock)
				{
					var lines = args.Message.Message.Split('\n');
					for (var idx = 0; idx < lines.Length; ++idx)
						log.LogWarning("{0}{1}", idx == 0 ? prefix : indent, lines[idx].TrimEnd('\r'));
				}
			};

		if (showInternalDiagnosticMessages)
			result.Diagnostics.InternalDiagnosticMessageEvent += args =>
			{
				lock (logLock)
				{
					var lines = args.Message.Message.Split('\n');
					for (var idx = 0; idx < lines.Length; ++idx)
						log.LogMessage("{0}{1}", idx == 0 ? prefix : indent, lines[idx].TrimEnd('\r'));
				}
			};

		return result;
	}
}
