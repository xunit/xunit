using System.Globalization;
using TestDriven.Framework;
using Xunit.Runner.Common;

namespace Xunit.Runner.TdNet;

public class TdNetDiagnosticMessageSink : TestMessageSink
{
	TdNetDiagnosticMessageSink()
	{ }

	public static TdNetDiagnosticMessageSink? TryCreate(
		ITestListener listener,
		object listenerLock,
		bool showDiagnosticMessages,
		bool showInternalDiagnosticMessages,
		string assemblyDisplayName)
	{
		if (!showDiagnosticMessages && !showInternalDiagnosticMessages)
			return null;

		var result = new TdNetDiagnosticMessageSink();
		var prefix = string.Format(CultureInfo.CurrentCulture, "[{0}]", assemblyDisplayName);
		var indent = new string(' ', prefix.Length);

		if (showDiagnosticMessages)
			result.Diagnostics.DiagnosticMessageEvent += args =>
			{
				lock (listenerLock)
				{
					var lines = args.Message.Message.Split('\n');
					for (var idx = 0; idx < lines.Length; ++idx)
						listener.WriteLine(string.Format(CultureInfo.CurrentCulture, "{0}{1}", idx == 0 ? prefix : indent, lines[idx].TrimEnd('\r')), Category.Warning);
				}
			};

		if (showInternalDiagnosticMessages)
			result.Diagnostics.InternalDiagnosticMessageEvent += args =>
			{
				lock (listenerLock)
				{
					var lines = args.Message.Message.Split('\n');
					for (var idx = 0; idx < lines.Length; ++idx)
						listener.WriteLine(string.Format(CultureInfo.CurrentCulture, "{0}{1}", idx == 0 ? prefix : indent, lines[idx].TrimEnd('\r')), Category.Debug);
				}
			};

		return result;
	}
}
