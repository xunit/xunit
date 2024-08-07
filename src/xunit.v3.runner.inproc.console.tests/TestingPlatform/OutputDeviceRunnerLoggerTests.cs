using System;
using System.Collections.Generic;
using Microsoft.Testing.Platform.OutputDevice;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.InProc.SystemConsole.TestingPlatform;

public class OutputDeviceRunnerLoggerTests
{
	public static IEnumerable<TheoryDataRow<Action<IRunnerLogger>, string, string, ConsoleColor?>> LogData =
	[
		new(
			logger => logger.LogError(StackFrameInfo.None, "Error Message"),
			"Err",
			"Error Message",
			ConsoleColor.Red
		),
		new(
			logger => logger.LogImportantMessage(StackFrameInfo.None, "Important Message"),
			"Imp",
			"Important Message",
			null
		),
		new(
			logger => logger.LogMessage(StackFrameInfo.None, "Message"),
			"Inf",
			"Message",
			ConsoleColor.DarkGray
		),
		new(
			logger => logger.LogRaw("Raw Text"),
			"Raw",
			"Raw Text",
			null
		),
		new(
			logger => logger.LogWarning(StackFrameInfo.None, "Warning Message"),
			"Wrn",
			"Warning Message",
			ConsoleColor.Yellow
		),
	];

	[Theory(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(LogData))]
	public void ForwardsMessages(
		Action<IRunnerLogger> action,
		string level,
		string expectedLog,
		ConsoleColor? expectedColor)
	{
		var outputDevice = new SpyTestPlatformOutputDevice();
		var innerLogger = new SpyRunnerLogger();
		var runnerLogger = new OutputDeviceRunnerLogger(outputDevice, innerLogger, rawOnly: false);

		action(runnerLogger);

		var innerSinkMessage = Assert.Single(innerLogger.Messages);
		Assert.Equal($"[{level}] {expectedLog}", innerSinkMessage);
		var datum = Assert.Single(outputDevice.DisplayedData);
		if (expectedColor is null)
		{
			var textDatum = Assert.IsType<TextOutputDeviceData>(datum);
			Assert.Equal(expectedLog, textDatum.Text);
		}
		else
		{
			var formattedDatum = Assert.IsType<FormattedTextOutputDeviceData>(datum);
			var color = Assert.IsType<SystemConsoleColor>(formattedDatum.ForegroundColor);
			Assert.Equal(expectedColor, color.ConsoleColor);
			Assert.Equal(expectedLog, formattedDatum.Text);
		}
	}
}
