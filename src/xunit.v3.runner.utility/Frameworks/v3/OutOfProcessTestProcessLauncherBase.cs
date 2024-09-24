using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Xunit.Internal;
using Xunit.Runner.Common;

namespace Xunit.v3;

/// <summary>
/// Base implementation of <see cref="ITestProcessLauncher"/> designed to launch an xUnit.net v3
/// test project out of process (the default behavior). The base class abstracts away the actual
/// launch and manipulation, so that replacement launchers to perform things like launching under
/// a debugger.
/// </summary>
public abstract class OutOfProcessTestProcessLauncherBase : ITestProcessLauncher
{
	/// <summary>
	/// Return <c>true</c> if running under Windows; return <c>false</c> if running elsewhere (and
	/// Mono is required for .NET Framework support). By default uses <see cref="RuntimeInformation"/>.
	/// </summary>
	public virtual bool IsWindows =>
		RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	/// <inheritdoc/>
	public ITestProcess? Launch(
		XunitProjectAssembly projectAssembly,
		IReadOnlyList<string> arguments)
	{
		Guard.ArgumentNotNull(projectAssembly);
		Guard.ArgumentNotNull(arguments);

		if (projectAssembly?.AssemblyFileName is null)
			return default;
		if (projectAssembly.AssemblyMetadata is null || projectAssembly.AssemblyMetadata.TargetFrameworkIdentifier == TargetFrameworkIdentifier.UnknownTargetFramework)
			return default;

		string? responseFile = default;
		string executable;
		var executableArguments = string.Empty;

		if (projectAssembly.AssemblyMetadata.TargetFrameworkIdentifier == TargetFrameworkIdentifier.DotNetCore)
		{
			// We want to run the executable stub rather than 'dotnet exec' because it will seek out the appropriate
			// bitness of the .NET SDK when appropriate.
			executable = projectAssembly.AssemblyFileName;

			// We always expect them to pass .dll here, because that's the actual test assembly. The executable stub won't
			// have appropriate metadata, but we'll just be safe here anyways.
			if (executable.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				executable = executable.Substring(0, projectAssembly.AssemblyFileName.Length - 4);
			if (IsWindows)
				executable += ".exe";

			if (!File.Exists(executable))
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not find app host executable '{0}'. Make sure you did not disable the app host when building the test project.", executable));
		}
		else if (!IsWindows)
		{
			executable = "mono";
			executableArguments = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", projectAssembly.AssemblyFileName);
		}
		else
			executable = projectAssembly.AssemblyFileName;

		if (arguments.Count != 0)
		{
			responseFile = Path.GetTempFileName();
			File.WriteAllLines(responseFile, arguments);

			if (executableArguments.Length != 0)
				executableArguments += " ";

			executableArguments += string.Format(CultureInfo.InvariantCulture, "@@ \"{0}\"", responseFile);
		}

		return StartTestProcess(executable, executableArguments, responseFile);
	}

	/// <summary>
	/// Starts the test process.
	/// </summary>
	/// <param name="executable">The executable to be launched (note that this may not be a fully qualified path name, as it
	/// may be depending on the system path to locate the executable)</param>
	/// <param name="executableArguments">The arguments to pass to the executable</param>
	/// <param name="responseFile">The response file that's being used, if present</param>
	/// <remarks>
	/// The response file will be part of the <paramref name="executableArguments"/>, but the actual path to
	/// the response file is provided here in the even that it needs to be modified or copied elsewhere (at
	/// which point the developer is responsible for updating <paramref name="executableArguments"/> to point
	/// to the new response file location). Additionally, the developer is responsible for deleting the
	/// response file from the disk when the execution is complete.
	/// </remarks>
	protected abstract ITestProcess? StartTestProcess(
		string executable,
		string executableArguments,
		string? responseFile);
}
