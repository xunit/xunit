using System;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.OutputDevice;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

/// <summary>
/// Abstract base class that implements <see cref="IOutputDeviceDataProducer"/>.
/// </summary>
/// <param name="componentName">The component name</param>
/// <param name="uid">The component UID</param>
/// <remarks>
/// This class is an implementation detail for Microsoft.Testing.Platform that is public for testing purposes.
/// Use this class at your own risk, as breaking changes may occur as needed.
/// </remarks>
public abstract class OutputDeviceDataProducerBase(
	string componentName,
	string uid) :
		ExtensionBase(componentName, uid), IOutputDeviceDataProducer
{
	/// <summary>
	/// Creates an instance of <see cref="IOutputDeviceData"/> with the given message, to
	/// be printed in the given color.
	/// </summary>
	/// <param name="message">The message</param>
	/// <param name="color">The message color</param>
	protected static IOutputDeviceData ToMessageWithColor(
		string message,
		ConsoleColor color) =>
			new FormattedTextOutputDeviceData(message) { ForegroundColor = new SystemConsoleColor { ConsoleColor = color } };
}
