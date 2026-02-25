using System.Reflection;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// This class acts as a repository for registered configuration for xunit.v3.runner.common.
/// </summary>
public static class RegisteredRunnerConfig
{
	/// <summary>
	/// Gets the list of console result writers registered for the given assembly.
	/// </summary>
	/// <param name="assembly">The assembly</param>
	/// <param name="messages">Messages that were generated during discovery</param>
	/// <returns>List of available result writers</returns>
	public static IReadOnlyDictionary<string, IConsoleResultWriter> GetConsoleResultWriters(
		Assembly assembly,
		List<string>? messages = null) =>
			GetResultWriters<IConsoleResultWriter, IRegisterConsoleResultWriterAttribute>("Console", assembly, messages);

	/// <summary>
	/// Gets the list of console result writers registered for the given assembly.
	/// </summary>
	/// <param name="assembly">The assembly</param>
	/// <param name="messages">Messages that were generated during discovery</param>
	/// <returns>List of available result writers</returns>
	public static IReadOnlyDictionary<string, IMicrosoftTestingPlatformResultWriter> GetMicrosoftTestingPlatformResultWriters(
		Assembly assembly,
		List<string>? messages = null) =>
			GetResultWriters<IMicrosoftTestingPlatformResultWriter, IRegisterMicrosoftTestingPlatformResultWriterAttribute>("Microsoft Testing Platform", assembly, messages);

	static IReadOnlyDictionary<string, TWriter> GetResultWriters<TWriter, TWriterRegistration>(
		string writerTypeDescription,
		Assembly assembly,
		List<string>? messages = null)
			where TWriterRegistration : IRegisterResultWriterAttribute
	{
		Guard.ArgumentNotNull(writerTypeDescription);
		Guard.ArgumentNotNull(assembly);

		messages ??= [];

		var result = new Dictionary<string, TWriter>(StringComparer.OrdinalIgnoreCase);

		foreach (var attribute in assembly.GetMatchingCustomAttributes<TWriterRegistration>(messages))
		{
			var resultWriterType = attribute.ResultWriterType;
			if (resultWriterType is null)
			{
				messages?.Add(
					string.Format(
						CultureInfo.CurrentCulture,
						"{0} result writer type '{1}' returned null from {2}",
						writerTypeDescription,
						attribute.GetType().SafeName(),
						nameof(IRegisterResultWriterAttribute.ResultWriterType)
					)
				);
				continue;
			}

			try
			{
				if (Activator.CreateInstance(resultWriterType) is not TWriter resultWriter)
				{
					messages?.Add(
						string.Format(
							CultureInfo.CurrentCulture,
							"{0} result writer type '{1}' does not implement '{2}'",
							writerTypeDescription,
							resultWriterType.SafeName(),
							typeof(TWriter).SafeName()
						)
					);
					continue;
				}

				if (result.TryGetValue(attribute.ID, out var existingWriter))
				{
					messages?.Add(
						string.Format(
							CultureInfo.CurrentCulture,
							"{0} result writer type '{1}' conflicts with existing result writer type '{2}' with the same ID",
							writerTypeDescription,
							resultWriterType.SafeName(),
							existingWriter?.GetType().SafeName()
						)
					);
					continue;
				}

				result.Add(attribute.ID, resultWriter);
			}
			catch (Exception ex)
			{
				messages?.Add(
					string.Format(
						CultureInfo.CurrentCulture,
						"Exception creating {0} result writer type '{1}': {2}",
						writerTypeDescription,
						resultWriterType.SafeName(),
						ex.Unwrap()
					)
				);
			}
		}

		return result;
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
		messages = [];
		var result = new List<IRunnerReporter>();

		foreach (var attribute in assembly.GetMatchingCustomAttributes<IRegisterRunnerReporterAttribute>(messages))
		{
			var reporterType = attribute.RunnerReporterType;
			if (reporterType is null)
			{
				messages.Add(string.Format(CultureInfo.CurrentCulture, "Runner reporter type '{0}' returned null from {1}", attribute.GetType().SafeName(), nameof(IRegisterRunnerReporterAttribute.RunnerReporterType)));
				continue;
			}

			try
			{
				if (Activator.CreateInstance(reporterType) is not IRunnerReporter reporter)
				{
					messages.Add(string.Format(CultureInfo.CurrentCulture, "Runner reporter type '{0}' does not implement '{1}'", reporterType.SafeName(), typeof(IRunnerReporter).SafeName()));
					continue;
				}

				if (!string.IsNullOrWhiteSpace(reporter.RunnerSwitch))
				{
					var existingReporter = result.FirstOrDefault(r => reporter.RunnerSwitch.Equals(r.RunnerSwitch, StringComparison.OrdinalIgnoreCase));
					if (existingReporter is not null)
					{
						messages.Add(string.Format(CultureInfo.CurrentCulture, "Runner reporter type '{0}' conflicts with existing runner reporter type '{1}' with the same switch", reporterType.SafeName(), existingReporter.GetType().SafeName()));
						continue;
					}
				}

				result.Add(reporter);
			}
			catch (Exception ex)
			{
				messages.Add(string.Format(CultureInfo.CurrentCulture, "Exception creating runner reporter type '{0}': {1}", reporterType.SafeName(), ex.Unwrap()));
			}
		}

		return result;
	}
}
