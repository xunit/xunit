using System;
using System.Collections.Generic;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents the configuration settings for a test runner which are independent of the test
/// assembly. Are usually passed via command line or some other equivalent mechanism. Accessed
/// via <see cref="XunitProject"/>.<see cref="XunitProject.Configuration"/>.
/// </summary>
public class TestProjectConfiguration
{
	/// <summary>
	/// Gets or sets a flag which indicates whether the runner should return assembly
	/// information rather than listing or executing tests.
	/// </summary>
	public bool? AssemblyInfo { get; set; }

	/// <summary>
	/// Gets a flag which indicates whether the runner should return assembly information
	/// rather than listing or executing tests. If the flag is not specified, returns the
	/// default value (<c>false</c>).
	/// </summary>
	public bool AssemblyInfoOrDefault => AssemblyInfo ?? false;

	/// <summary>
	/// Gets the environment variable name used to test for the user requesting no color output.
	/// </summary>
	public const string EnvNameNoColor = "NO_COLOR";

	/// <summary>
	/// Gets or sets a flag which indicates whether the runner should attempt to attach the debugger
	/// before running any tests.
	/// </summary>
	public bool? Debug { get; set; }

	/// <summary>
	/// Gets a flag which indicates whether the runner should attempt to attach the debugger
	/// before running any tests. If the flag is not specified, returns the default value
	/// (<c>false</c>).
	/// </summary>
	public bool DebugOrDefault => Debug ?? false;

	/// <summary>
	/// Gets or sets a flag which indicates whether the runner should ignore test failures.
	/// </summary>
	public bool? IgnoreFailures { get; set; }

	/// <summary>
	/// Gets a flag which indicates whether the runner should ignore test failures. If the
	/// flag is not specified, returns the default value (<c>false</c>).
	/// </summary>
	public bool IgnoreFailuresOrDefault => IgnoreFailures ?? false;

	/// <summary>
	/// Gets or sets a flag to indicate that we should list things instead of run them
	/// (and what we're listing, and in what format).
	/// </summary>
	public (ListOption Option, ListFormat Format)? List { get; set; }

	/// <summary>
	/// The output files that should be generated from the test run. The key is
	/// the output type, and the value is the output filename. The output type matches
	/// the <see cref="Transform.ID"/> property on the transforms available in
	/// <see cref="TransformFactory"/>.<see cref="TransformFactory.AvailableTransforms"/>.
	/// </summary>
	public Dictionary<string, string> Output { get; } = [];

	/// <summary>
	/// Gets or sets a flag indicating that the test runner should not attempt to use any
	/// automatically (aka environmentally) enabled reporters.
	/// </summary>
	public bool? NoAutoReporters { get; set; }

	/// <summary>
	/// Gets a flag indicating that the test runner should not attempt to use any automatically
	/// (aka environmentally) enabled reporters. If the flag is not set, returns the default
	/// value (<c>false</c>).
	/// </summary>
	public bool NoAutoReportersOrDefault => NoAutoReporters ?? false;

	/// <summary>
	/// Gets or sets a flag indicating that the test runner should not output any color codes
	/// when writing text to the console.
	/// </summary>
	public bool? NoColor { get; set; }

	/// <summary>
	/// Gets a flag indicating that the test runner should not output any color codes when writing
	/// text to the console. If the flag is not set, returns <c>true</c> if the user has defined
	/// the NO_COLOR environment variable, or <c>false</c> otherwise.
	/// </summary>
	public bool NoColorOrDefault => NoColor ?? Environment.GetEnvironmentVariable(EnvNameNoColor) is not null;

	/// <summary>
	/// Gets or sets a flag indicating that the test runner should not output the copyright
	/// information.
	/// </summary>
	public bool? NoLogo { get; set; }

	/// <summary>
	/// Gets a flag indicating that the test runner hsould not output the copyright information.
	/// If the flag is not set, returns the default value (<c>false</c>).
	/// </summary>
	public bool NoLogoOrDefault => NoLogo ?? false;

	/// <summary>
	/// Gets or sets a flag indicating that the test runner should pause before running any
	/// tests.
	/// </summary>
	public bool? Pause { get; set; }

	/// <summary>
	/// Gets a flag indicating that the test runner should pause before running any tests.
	/// If the flag is not set, returns the default value (<c>false</c>).
	/// </summary>
	public bool PauseOrDefault => Pause ?? false;

	/// <summary>
	/// Gets or sets a flag indicating that ANSI color usage should be forced on Windows.
	/// ANSI color is always used for non-Windows.
	/// </summary>
	public bool? UseAnsiColor { get; set; }

	/// <summary>
	/// Gets a flag indicating that ANSI color usage should be forced on Windows. ANSI color is
	/// always used for non-Windows. If the flag is not set, returns the default value (<c>false</c>).
	/// </summary>
	public bool UseAnsiColorOrDefault => UseAnsiColor ?? false;

	/// <summary>
	/// Gets or sets a flag indicating that the test runner should pause after all tests
	/// have run.
	/// </summary>
	public bool? Wait { get; set; }

	/// <summary>
	/// Gets a flag indicating that the test runner should pause after all tests have run.
	/// If the flag is not set, returns the default value (<c>false</c>).
	/// </summary>
	public bool WaitOrDefault => Wait ?? false;

	/// <summary>
	/// Gets or sets a flag indicating that the test runner should wait for a debugger to be
	/// attached before performing any actions.
	/// </summary>
	public bool? WaitForDebugger { get; set; }

	/// <summary>
	/// Gets a flag indicating that the test runner should wait for a debugger to be attached
	/// before performing any actions. If the flag is not set, returns the default value (<c>false</c>).
	/// </summary>
	public bool WaitForDebuggerOrDefault => WaitForDebugger ?? false;
}
