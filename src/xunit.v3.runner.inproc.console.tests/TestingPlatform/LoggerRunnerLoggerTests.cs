using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.InProc.SystemConsole.TestingPlatform;

public class LoggerRunnerLoggerTests
{
	public static IEnumerable<TheoryDataRow<Action<IRunnerLogger>, string>> LogData =
	[
		new(
			logger => logger.LogError(StackFrameInfo.None, "Error Message"),
			"[Error] Error Message"
		),
		new(
			logger => logger.LogImportantMessage(StackFrameInfo.None, "Important Message"),
			"[Information] Important Message"
		),
		new(
			logger => logger.LogMessage(StackFrameInfo.None, "Message"),
			"[Information] Message"
		),
		new(
			logger => logger.LogRaw("Raw Text"),
			"[Information] Raw Text"
		),
		new(
			logger => logger.LogWarning(StackFrameInfo.None, "Warning Message"),
			"[Warning] Warning Message"
		)
	];

	[Theory(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(LogData))]
	public void ForwardsMessages(
		Action<IRunnerLogger> action,
		string expectedLog)
	{
		var logger = new SpyTestPlatformLogger();
		var runnerLogger = new LoggerRunnerLogger(logger);

		action(runnerLogger);

		var log = Assert.Single(logger.Messages);
		Assert.Equal(expectedLog, log);
	}
}
