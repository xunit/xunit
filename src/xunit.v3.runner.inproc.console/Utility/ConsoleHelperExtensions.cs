using Xunit.Internal;
using Xunit.Runner.InProc.SystemConsole;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Extension methods for <see cref="ConsoleHelper"/>.
/// </summary>
public static class ConsoleHelperExtensions
{
	/// <summary>
	/// Writes a message to the console, and waits for acknowledge as appropriate.
	/// </summary>
	/// <param name="helper"></param>
	/// <param name="message">The message to write</param>
	/// <param name="automatedMode">The current automated mode we're running in</param>
	public static void WriteMessage(
		this ConsoleHelper helper,
		IMessageSinkMessage message,
		AutomatedMode automatedMode)
	{
		Guard.ArgumentNotNull(helper);
		Guard.ArgumentNotNull(message);

		helper.WriteLine(message.ToJson());

		if (automatedMode == AutomatedMode.Sync)
			helper.ReadLine();
	}
}
