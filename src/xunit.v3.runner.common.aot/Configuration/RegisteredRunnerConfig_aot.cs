#pragma warning disable IDE0060 // Method contracts here must match the non-AOT version

using System.Collections.Concurrent;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// This class acts as a repository for registered configuration for xunit.v3.runner.common.
/// </summary>
public static class RegisteredRunnerConfig
{
	static readonly ConcurrentDictionary<string, IConsoleResultWriter> consoleResultWriters = new(StringComparer.OrdinalIgnoreCase);
	static readonly List<string> consoleResultWriterMessages = [];
	static readonly ConcurrentDictionary<string, IMicrosoftTestingPlatformResultWriter> mtpResultWriters = new(StringComparer.OrdinalIgnoreCase);
	static readonly List<string> mtpResultWriterMessages = [];
	static readonly ConcurrentBag<IRunnerReporter> runnerReporters = [];
	static readonly List<string> runnerReporterMessages = [];

	/// <summary>
	/// Gets the list of console result writers registered for the given assembly.
	/// </summary>
	/// <param name="assembly">The assembly</param>
	/// <param name="messages">Messages that were generated during discovery</param>
	/// <returns>List of available result writers</returns>
	public static IReadOnlyDictionary<string, IConsoleResultWriter> GetConsoleResultWriters(
		Assembly assembly,
		List<string>? messages = null)
	{
		messages?.AddRange(consoleResultWriterMessages);
		return consoleResultWriters;
	}

	/// <summary>
	/// Gets the list of console result writers registered for the given assembly.
	/// </summary>
	/// <param name="assembly">The assembly</param>
	/// <param name="messages">Messages that were generated during discovery</param>
	/// <returns>List of available result writers</returns>
	public static IReadOnlyDictionary<string, IMicrosoftTestingPlatformResultWriter> GetMicrosoftTestingPlatformResultWriters(
		Assembly assembly,
		List<string>? messages = null)
	{
		messages?.AddRange(mtpResultWriterMessages);
		return mtpResultWriters;
	}

	/// <summary>
	/// Gets the list of runner reporters registered for the given assembly.
	/// </summary>
	/// <param name="assembly">The assembly</param>
	/// <param name="messages">Messages that were generated during discovery</param>
	/// <returns>List of available reporters</returns>
	public static IReadOnlyList<IRunnerReporter> GetRunnerReporters(
		Assembly assembly,
		out List<string> messages)
	{
		messages = [.. runnerReporterMessages];
		return [.. runnerReporters];
	}

	/// <summary>
	/// Adds a console result writer registration.
	/// </summary>
	/// <param name="id">The ID of the result writer</param>
	/// <param name="resultWriter">The result writer</param>
	/// <remarks>
	/// The ID is used to construct the console command line option <c>"-result-{ID}"</c> and therefore
	/// must be unique.
	/// </remarks>
	public static void RegisterConsoleResultWriter(
		string id,
		IConsoleResultWriter resultWriter) =>
			RegisterResultWriter(id, resultWriter, "console", consoleResultWriters, consoleResultWriterMessages);

	/// <summary>
	/// Adds a Microsoft Testing Platform result writer registration.
	/// </summary>
	/// <param name="id">The ID of the result writer</param>
	/// <param name="resultWriter">The result writer</param>
	/// <remarks>
	/// The ID is used to construct the MTP command line options <c>"--xunit-result-{id}"</c> and
	/// <c>"--xunit-result-{id}-filename"</c>, and therefore must be unique.
	/// </remarks>
	public static void RegisterMicrosoftTestingPlatformResultWriter(
		string id,
		IMicrosoftTestingPlatformResultWriter resultWriter) =>
			RegisterResultWriter(id, resultWriter, "Microsoft Testing Platform", mtpResultWriters, mtpResultWriterMessages);

	/// <summary>
	/// Adds a runner reporter to the available list.
	/// </summary>
	/// <param name="runnerReporter">The runner reporter</param>
	public static void RegisterRunnerReporter(IRunnerReporter runnerReporter)
	{
		if (runnerReporter is null)
			runnerReporterMessages.Add("Cannot add a null runner reporter");
		else
			runnerReporters.Add(runnerReporter);
	}

	static void RegisterResultWriter<TWriter>(
		string id,
		TWriter resultWriter,
		string writerType,
		ConcurrentDictionary<string, TWriter> registrations,
		List<string> messages)
	{
		if (id is null)
		{
			messages.Add(
				string.Format(
					CultureInfo.CurrentCulture,
					"Cannot add {0} result writer type '{1}' with a null ID",
					writerType,
					resultWriter?.GetType().SafeName() ?? "<null>"
				)
			);

			return;
		}

		if (resultWriter is null)
		{
			consoleResultWriterMessages.Add(
				string.Format(
					CultureInfo.CurrentCulture,
					"Cannot add {0} result writer ID '{1}' with a null writer",
					writerType,
					id
				)
			);

			return;
		}

		if (!registrations.TryAdd(id, resultWriter))
		{
			registrations.TryGetValue(id, out var existingWriter);

			messages.Add(
				string.Format(
					CultureInfo.CurrentCulture,
					"The {0} result writer type '{1}' conflicts with existing result writer type '{2}' with the same ID",
					writerType,
					resultWriter.GetType().SafeName(),
					existingWriter?.GetType().SafeName()
				)
			);
		}
	}
}
