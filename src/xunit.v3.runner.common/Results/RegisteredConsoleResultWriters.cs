using System.Collections.Generic;
using System.Reflection;

namespace Xunit.Runner.Common;

/// <summary>
/// Utility class for enumerating the result writers registered for the given assembly
/// (for console mode).
/// </summary>
public static class RegisteredConsoleResultWriters
{
	/// <summary>
	/// Gets the list of console result writers registered for the given assembly.
	/// </summary>
	/// <param name="assembly">The assembly</param>
	/// <param name="messages">Messages that were generated during discovery</param>
	/// <returns>List of available result writers</returns>
	public static IReadOnlyDictionary<string, IConsoleResultWriter> Get(
		Assembly assembly,
		List<string>? messages = null) =>
			RegisteredResultWriters<IConsoleResultWriter, IRegisterConsoleResultWriterAttribute>.Get("Console", assembly, messages);
}
