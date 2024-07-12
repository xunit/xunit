using System;
using System.IO;
using Xunit;
using Xunit.Runner.Common;

[Collection("ConsoleHelper statics")]
public class ConsoleRunnerLoggerTests
{
	[Fact]
	public void WriteLine_ColorsEnabled_PlainText()
	{
		var writer = new StringWriter();
		var message = "foo bar";
		var sut = new ConsoleRunnerLogger(true, false, writer);

		sut.WriteLine(message);

		Assert.Equal(message + Environment.NewLine, writer.ToString());
	}

	[Fact]
	public void WriteLine_ColorsEnabled_AnsiText()
	{
		var writer = new StringWriter();
		var message = "\x1b[3m\x1b[36mhello world\u001b[0m || \x1b[94;103mbright blue on bright yellow\x1b[m";
		var sut = new ConsoleRunnerLogger(true, false, writer);

		sut.WriteLine(message);

		Assert.Equal(message + Environment.NewLine, writer.ToString());
	}

	[Fact]
	public void WriteLine_ColorsDisabled_PlainText()
	{
		var writer = new StringWriter();
		var message = "foo bar";
		var sut = new ConsoleRunnerLogger(false, false, writer);

		sut.WriteLine(message);

		Assert.Equal(message + Environment.NewLine, writer.ToString());
	}

	[Fact]
	public void WriteLine_ColorsDisabled_AnsiText()
	{
		var writer = new StringWriter();
		var sut = new ConsoleRunnerLogger(false, false, writer);

		sut.WriteLine("\x1b[3m\x1b[36mhello world\u001b[0m || \x1b[94;103mbright blue on bright yellow\x1b[m");

		Assert.Equal("hello world || bright blue on bright yellow" + Environment.NewLine, writer.ToString());
	}

	[Fact]
	public void WriteLine_ColorsEnabled_AnsiColorsEnabled()
	{
		var oldWriter = ConsoleHelper.ConsoleWriter;

		try
		{
			var writer = new StringWriter();
			var sut = new ConsoleRunnerLogger(true, true, writer);
			ConsoleHelper.ConsoleWriter = writer;

			sut.LogError("This is an error message");

			Assert.Equal("\u001b[91mThis is an error message" + Environment.NewLine + "\u001b[0m", writer.ToString());
		}
		finally
		{
			ConsoleHelper.ConsoleWriter = oldWriter;
		}
	}
}
