#pragma warning disable IDE0060  // Cannot remove unused parameters, this must match the signature of the .NET Framework version

#if NETCOREAPP

using System;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// This class provides assistance with assembly resolution for missing assemblies.
/// </summary>
public static class AssemblyHelper
{
	/// <summary>
	/// Subscribes to the appropriate assembly resolution event, to provide automatic assembly resolution for
	/// an assembly and any of its dependencies. Depending on the target platform, this may include the use
	/// of the .deps.json file generated during the build process.
	/// </summary>
	/// <returns>An object which, when disposed, un-subscribes.</returns>
	public static IDisposable? SubscribeResolveForAssembly(
		string assemblyFileName,
		IMessageSink? diagnosticMessageSink = null) =>
			null;

	/// <summary>
	/// Subscribes to the appropriate assembly resolution event, to provide automatic assembly resolution for
	/// an assembly and any of its dependencies. Depending on the target platform, this may include the use
	/// of the .deps.json file generated during the build process.
	/// </summary>
	/// <returns>An object which, when disposed, un-subscribes.</returns>
	public static IDisposable? SubscribeResolveForAssembly(
		Type typeInAssembly,
		IMessageSink? diagnosticMessageSink = null) =>
			null;
}

#endif
