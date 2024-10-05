using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Utility class for enumerating the runner reporters registered for the given assembly.
/// </summary>
public static class RegisteredRunnerReporters
{
	/// <summary>
	/// Gets the list of rrunner reporters registered for the given assembly. 
	/// </summary>
	/// <param name="assembly">The assembly</param>
	/// <param name="messages">Messages that were generated during discovery</param>
	/// <returns>List of available reporters</returns>
	public static List<IRunnerReporter> Get(
		Assembly assembly,
		out List<string> messages)
	{
		messages = [];
		var result = new List<IRunnerReporter>();

		foreach (var attribute in assembly.GetCustomAttributes().OfType<IRegisterRunnerReporterAttribute>())
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
