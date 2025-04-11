using System;
using System.Globalization;
using System.Threading;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Runner.v3;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// This class adapts <see cref="ITestProcessLauncher"/> into <see cref="ITestProcessDirectLauncher"/>, including static
/// versions that can be used be an implementation of <see cref="ITestProcessDirectLauncher"/> that must fall back
/// to using <see cref="ITestProcessLauncher"/> code due to version requirements.
/// </summary>
public class TestProcessLauncherAdapter(ITestProcessLauncher launcher) :
	ITestProcessDirectLauncher
{
	static readonly Version Version_0_3_0 = new(0, 3, 0);

	/// <inheritdoc/>
	public ITestProcessBase Find(
		XunitProjectAssembly projectAssembly,
		TestAssemblyInfo assemblyInfo,
		IMessageSink messageSink,
		IMessageSink? diagnosticMessageSink,
		ISourceInformationProvider? sourceInformationProvider) =>
			Find(launcher, projectAssembly, assemblyInfo, messageSink, diagnosticMessageSink, sourceInformationProvider);

	internal static ITestProcessBase Find(
		ITestProcessLauncher launcher,
		XunitProjectAssembly projectAssembly,
		TestAssemblyInfo assemblyInfo,
		IMessageSink messageSink,
		IMessageSink? diagnosticMessageSink,
		ISourceInformationProvider? sourceInformationProvider)
	{
		Guard.ArgumentNotNull(launcher);
		Guard.ArgumentNotNull(projectAssembly);
		Guard.ArgumentNotNull(assemblyInfo);
		Guard.ArgumentNotNull(messageSink);

		var arguments = Xunit3ArgumentFactory.ForFindInProcess(assemblyInfo.CoreFramework, projectAssembly, ListOption.Discovery);
		var process =
			launcher.Launch(projectAssembly, arguments)
				?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not launch test process. Test assembly '{0}', arguments: '{1}'", projectAssembly.AssemblyFileName, string.Join(" ", arguments)));

		var delegatingMessageSink = messageSink;
		if (projectAssembly.Configuration.IncludeSourceInformationOrDefault && sourceInformationProvider is not null)
			delegatingMessageSink = new SourceInformationMessageSink(delegatingMessageSink, sourceInformationProvider);

		var processCancellationSink = new ProcessCancellationMessageSink(delegatingMessageSink, process);
		var splitSink = new MessageSplitMessageSink(processCancellationSink, diagnosticMessageSink);
		var discoverySink = new DiscoveryStartingCompleteMessageSink(projectAssembly, splitSink);

		ThreadPool.QueueUserWorkItem(async _ =>
		{
			try
			{
				var internalDiagnosticMessages = projectAssembly.Configuration.InternalDiagnosticMessagesOrDefault;
				var synchronousMessageReporting = SynchronousMessageReporting(assemblyInfo, projectAssembly);

				while (true)
				{
					var @continue = true;
					var line = await process.StandardOutput.ReadLineAsync();
					if (line is null)
						break;

					try
					{
						var message = MessageSinkMessageDeserializer.Deserialize(line, discoverySink);
						if (message is null)
						{
							if (internalDiagnosticMessages)
								@continue = discoverySink.OnMessage(new InternalDiagnosticMessage("Received unparseable output from test process: " + line)) && @continue;
						}
						else
							@continue = discoverySink.OnMessage(message) && @continue;

						if (!@continue)
							process.Cancel(forceCancellation: false);
					}
					finally
					{
						if (synchronousMessageReporting)
							await process.StandardInput.WriteLineAsync();
					}
				}
			}
			finally
			{
				process.SafeDispose();
				discoverySink.Finish();
			}
		});

		return process;
	}

	/// <inheritdoc/>
	public TestAssemblyInfo GetAssemblyInfo(XunitProjectAssembly projectAssembly) =>
		GetAssemblyInfo(launcher, projectAssembly);

	internal static TestAssemblyInfo GetAssemblyInfo(
		ITestProcessLauncher launcher,
		XunitProjectAssembly projectAssembly)
	{
		Guard.ArgumentNotNull(launcher);
		Guard.ArgumentNotNull(projectAssembly);

		using var process = launcher.Launch(projectAssembly, ["-assemblyInfo"]) ?? throw new InvalidOperationException("Could not launch test process");
		if (!process.WaitForExit(60_000))
			throw new InvalidOperationException("Test process did not respond within 60 seconds");

		var output = process.StandardOutput.ReadToEnd().Trim(' ', '\r', '\n');
		try
		{
			return TestAssemblyInfo.FromJson(output);
		}
		catch (ArgumentException ex)
		{
			throw new InvalidOperationException("Test process did not return valid JSON (" + ex.Message + ")." + (output.Length > 0 ? (" Output:" + Environment.NewLine + output) : string.Empty));
		}
	}

	/// <inheritdoc/>
	public ITestProcessBase Run(
		XunitProjectAssembly projectAssembly,
		TestAssemblyInfo assemblyInfo,
		IMessageSink messageSink,
		IMessageSink? diagnosticMessageSink,
		ISourceInformationProvider? sourceInformationProvider) =>
			Run(launcher, projectAssembly, assemblyInfo, messageSink, diagnosticMessageSink, sourceInformationProvider);

	internal static ITestProcessBase Run(
		ITestProcessLauncher launcher,
		XunitProjectAssembly projectAssembly,
		TestAssemblyInfo assemblyInfo,
		IMessageSink messageSink,
		IMessageSink? diagnosticMessageSink,
		ISourceInformationProvider? sourceInformationProvider)
	{
		Guard.ArgumentNotNull(launcher);
		Guard.ArgumentNotNull(projectAssembly);
		Guard.ArgumentNotNull(assemblyInfo);
		Guard.ArgumentNotNull(messageSink);

		var arguments = Xunit3ArgumentFactory.ForRunInProcess(assemblyInfo.CoreFramework, projectAssembly);

		var process =
			launcher.Launch(projectAssembly, arguments)
				?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not launch test process. Test assembly '{0}', arguments: '{1}'", projectAssembly.AssemblyFileName, string.Join(" ", arguments)));

		ThreadPool.QueueUserWorkItem(async _ =>
		{
			var internalDiagnosticMessages = projectAssembly.Configuration.InternalDiagnosticMessagesOrDefault;
			var synchronousMessageReporting = SynchronousMessageReporting(assemblyInfo, projectAssembly);

			try
			{
				while (true)
				{
					var line = await process.StandardOutput.ReadLineAsync();
					if (line is null)
						break;

					try
					{
						var @continue = true;
						var message = MessageSinkMessageDeserializer.Deserialize(line, diagnosticMessageSink);

						if (message is null)
						{
							if (internalDiagnosticMessages && diagnosticMessageSink is not null)
								@continue = diagnosticMessageSink.OnMessage(new InternalDiagnosticMessage("Received unparseable output from test process: " + line)) && @continue;
						}
						else if (message is IDiagnosticMessage or IInternalDiagnosticMessage)
							@continue = (diagnosticMessageSink?.OnMessage(message) ?? true) && @continue;
						else
							@continue = messageSink.OnMessage(message) && @continue;

						if (message is ITestAssemblyFinished)
							break;
					}
					finally
					{
						if (synchronousMessageReporting)
							process.StandardInput.WriteLine();
					}
				}
			}
			finally
			{
				process.SafeDispose();
			}
		});

		return process;
	}

	static bool SynchronousMessageReporting(
		TestAssemblyInfo assemblyInfo,
		XunitProjectAssembly projectAssembly) =>
			assemblyInfo.CoreFramework >= Version_0_3_0 && projectAssembly.Configuration.SynchronousMessageReportingOrDefault;
}
