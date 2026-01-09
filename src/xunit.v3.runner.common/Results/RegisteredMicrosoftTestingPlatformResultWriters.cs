using System.Collections.Generic;
using System.Reflection;

namespace Xunit.Runner.Common;

/// <summary>
/// Utility class for enumerating the result writers registered for the given assembly
/// (for Microsoft Testing Platform mode).
/// </summary>
public static class RegisteredMicrosoftTestingPlatformResultWriters
{
	/// <summary>
	/// Gets the list of console result writers registered for the given assembly.
	/// </summary>
	/// <param name="assembly">The assembly</param>
	/// <param name="messages">Messages that were generated during discovery</param>
	/// <returns>List of available result writers</returns>
	public static IReadOnlyDictionary<string, IMicrosoftTestingPlatformResultWriter> Get(
		Assembly assembly,
		List<string>? messages = null) =>
			RegisteredResultWriters<IMicrosoftTestingPlatformResultWriter, IRegisterMicrosoftTestingPlatformResultWriterAttribute>.Get("Microsoft Testing Platform", assembly, messages);
}
