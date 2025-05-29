using System;
using System.Globalization;
using Microsoft.Testing.Platform.Configurations;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

/// <summary>
/// Consumer of <see cref="IConfiguration"/> for xUnit.net v3.
/// </summary>
/// <remarks>
/// This class is an implementation detail for Microsoft.Testing.Platform that is public for testing purposes.
/// Use this class at your own risk, as breaking changes may occur as needed.
/// </remarks>
public static class TestConfig
{
	/// <summary>
	/// Parses configuration from <see cref="IConfiguration"/> and places the values into
	/// an instance of <see cref="XunitProjectAssembly"/>.
	/// </summary>
	/// <param name="configuration">The configuration</param>
	/// <param name="projectAssembly">The project assembly to put config values into</param>
	public static void Parse(
		IConfiguration configuration,
		XunitProjectAssembly projectAssembly)
	{
		Guard.ArgumentNotNull(configuration);
		Guard.ArgumentNotNull(projectAssembly);

		var assertEquivalentMaxDepthString = configuration[Keys.AssertEquivalentMaxDepth];
		if (int.TryParse(assertEquivalentMaxDepthString, out var assertEquivalentMaxDepth) && assertEquivalentMaxDepth >= 1)
			projectAssembly.Configuration.AssertEquivalentMaxDepth = assertEquivalentMaxDepth;

		var cultureString = configuration[Keys.Culture];
		if (cultureString is not null)
			projectAssembly.Configuration.Culture = cultureString.ToUpperInvariant() switch
			{
				"DEFAULT" => null,
				"INVARIANT" => "",
				_ => cultureString,
			};

		var diagnosticMessagesString = configuration[Keys.DiagnosticMessages];
		if (TryParseBool(diagnosticMessagesString, out var diagnosticMessages))
			projectAssembly.Configuration.DiagnosticMessages = diagnosticMessages;

		var explicitString = configuration[Keys.Explicit];
		if (Enum.TryParse<ExplicitOption>(explicitString, ignoreCase: true, out var @explicit))
			projectAssembly.Configuration.ExplicitOption = @explicit;

		var failSkipsString = configuration[Keys.FailSkips];
		if (TryParseBool(failSkipsString, out var failSkips))
			projectAssembly.Configuration.FailSkips = failSkips;

		var failWarnsString = configuration[Keys.FailWarns];
		if (TryParseBool(failWarnsString, out var failWarns))
			projectAssembly.Configuration.FailTestsWithWarnings = failWarns;

		var internalDiagnosticMessagesString = configuration[Keys.InternalDiagnosticMessages];
		if (TryParseBool(internalDiagnosticMessagesString, out var internalDiagnosticMessages))
			projectAssembly.Configuration.InternalDiagnosticMessages = internalDiagnosticMessages;

		var longRunningTestSecondsString = configuration[Keys.LongRunningTestSeconds];
		if (int.TryParse(longRunningTestSecondsString, out var longRunningTestSeconds) && longRunningTestSeconds >= 1)
			projectAssembly.Configuration.LongRunningTestSeconds = longRunningTestSeconds;

		var maxParallelThreadsString = configuration[Keys.MaxParallelThreads];
		if (maxParallelThreadsString is not null)
			switch (maxParallelThreadsString.ToUpperInvariant())
			{
				case "DEFAULT":
				case "0":
					projectAssembly.Configuration.MaxParallelThreads = null;
					break;

				case "UNLIMITED":
				case "-1":
					projectAssembly.Configuration.MaxParallelThreads = -1;
					break;

				default:
					var match = ConfigUtility.MultiplierStyleMaxParallelThreadsRegex.Match(maxParallelThreadsString);
					// Use invariant format and convert ',' to '.' so we can always support both formats, regardless of locale
					// If we stick to locale-only parsing, we could break people when moving from one locale to another (for example,
					// from people running tests on their desktop in a comma locale vs. running them in CI with a decimal locale).
					if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var maxThreadMultiplier))
						projectAssembly.Configuration.MaxParallelThreads = (int)(maxThreadMultiplier * Environment.ProcessorCount);
					else if (int.TryParse(maxParallelThreadsString, out var threadValue) && threadValue >= -1)
						projectAssembly.Configuration.MaxParallelThreads = threadValue;
					break;
			}

		var methodDisplayString = configuration[Keys.MethodDisplay];
		if (Enum.TryParse<TestMethodDisplay>(methodDisplayString, ignoreCase: true, out var methodDisplay))
			projectAssembly.Configuration.MethodDisplay = methodDisplay;

		var methodDisplayOptionsString = configuration[Keys.MethodDisplayOptions];
		if (Enum.TryParse<TestMethodDisplayOptions>(methodDisplayOptionsString, ignoreCase: true, out var methodDisplayOptions))
			projectAssembly.Configuration.MethodDisplayOptions = methodDisplayOptions;

		var parallelAlgorithmString = configuration[Keys.ParallelAlgorithm];
		if (Enum.TryParse<ParallelAlgorithm>(parallelAlgorithmString, ignoreCase: true, out var parallelAlgorithm))
			projectAssembly.Configuration.ParallelAlgorithm = parallelAlgorithm;

		var parallelizeTestCollectionsString = configuration[Keys.ParallelizeTestCollections];
		if (TryParseBool(parallelizeTestCollectionsString, out var parallelizeTestCollections))
			projectAssembly.Configuration.ParallelizeTestCollections = parallelizeTestCollections;

		var preEnumerateTheoriesString = configuration[Keys.PreEnumerateTheories];
		if (TryParseBool(preEnumerateTheoriesString, out var preEnumerateTheories))
			projectAssembly.Configuration.PreEnumerateTheories = preEnumerateTheories;

		var printMaxEnumerableLengthString = configuration[Keys.PrintMaxEnumerableLength];
		if (int.TryParse(printMaxEnumerableLengthString, out var printMaxEnumerableLength) && printMaxEnumerableLength >= 0)
			projectAssembly.Configuration.PrintMaxEnumerableLength = printMaxEnumerableLength;

		var printMaxObjectDepthString = configuration[Keys.PrintMaxObjectDepth];
		if (int.TryParse(printMaxObjectDepthString, out var printMaxObjectDepth) && printMaxObjectDepth >= 0)
			projectAssembly.Configuration.PrintMaxObjectDepth = printMaxObjectDepth;

		var printMaxObjectMemberCountString = configuration[Keys.PrintMaxObjectMemberCount];
		if (int.TryParse(printMaxObjectMemberCountString, out var printMaxObjectMemberCount) && printMaxObjectMemberCount >= 0)
			projectAssembly.Configuration.PrintMaxObjectMemberCount = printMaxObjectMemberCount;

		var printMaxStringLengthString = configuration[Keys.PrintMaxStringLength];
		if (int.TryParse(printMaxStringLengthString, out var printMaxStringLength) && printMaxStringLength >= 0)
			projectAssembly.Configuration.PrintMaxStringLength = printMaxStringLength;

		var seedString = configuration[Keys.Seed];
		if (int.TryParse(seedString, NumberStyles.None, NumberFormatInfo.CurrentInfo, out var seed))
			projectAssembly.Configuration.Seed = seed;

		var showLiveOutputString = configuration[Keys.ShowLiveOutput];
		if (TryParseBool(showLiveOutputString, out var showLiveOutput))
			projectAssembly.Configuration.ShowLiveOutput = showLiveOutput;

		var stopOnFailString = configuration[Keys.StopOnFail];
		if (TryParseBool(stopOnFailString, out var stopOnFail))
			projectAssembly.Configuration.StopOnFail = stopOnFail;
	}

	static bool TryParseBool(string? value, out bool result)
	{
		bool? output = value?.ToUpperInvariant() switch
		{
			"ON" or "TRUE" or "1" => true,
			"OFF" or "FALSE" or "0" => false,
			_ => null,
		};

		result = output ?? false;
		return output is not null;
	}

	/// <summary>
	/// The values of the configuration keys
	/// </summary>
	public static class Keys
	{
		/// <summary/>
		public const string AssertEquivalentMaxDepth = "xUnit:assertEquivalentMaxDepth";
		/// <summary/>
		public const string Culture = "xUnit:culture";
		/// <summary/>
		public const string DiagnosticMessages = "xUnit:diagnosticMessages";
		/// <summary/>
		public const string Explicit = "xUnit:explicit";
		/// <summary/>
		public const string FailSkips = "xUnit:failSkips";
		/// <summary/>
		public const string FailWarns = "xUnit:failWarns";
		/// <summary/>
		public const string InternalDiagnosticMessages = "xUnit:internalDiagnosticMessages";
		/// <summary/>
		public const string LongRunningTestSeconds = "xUnit:longRunningTestSeconds";
		/// <summary/>
		public const string MaxParallelThreads = "xUnit:maxParallelThreads";
		/// <summary/>
		public const string MethodDisplay = "xUnit:methodDisplay";
		/// <summary/>
		public const string MethodDisplayOptions = "xUnit:methodDisplayOptions";
		/// <summary/>
		public const string ParallelAlgorithm = "xUnit:parallelAlgorithm";
		/// <summary/>
		public const string ParallelizeTestCollections = "xUnit:parallelizeTestCollections";
		/// <summary/>
		public const string PreEnumerateTheories = "xUnit:preEnumerateTheories";
		/// <summary/>
		public const string PrintMaxEnumerableLength = "xUnit:printMaxEnumerableLength";
		/// <summary/>
		public const string PrintMaxObjectDepth = "xUnit:printMaxObjectDepth";
		/// <summary/>
		public const string PrintMaxObjectMemberCount = "xUnit:printMaxObjectMemberCount";
		/// <summary/>
		public const string PrintMaxStringLength = "xUnit:printMaxStringLength";
		/// <summary/>
		public const string Seed = "xUnit:seed";
		/// <summary/>
		public const string ShowLiveOutput = "xUnit:showLiveOutput";
		/// <summary/>
		public const string StopOnFail = "xUnit:stopOnFail";
	}
}
