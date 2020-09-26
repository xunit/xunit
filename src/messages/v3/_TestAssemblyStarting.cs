using System;

#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary>
	/// This message indicates that the execution process is about to start for
	/// the requested assembly.
	/// </summary>
	public class _TestAssemblyStarting : _TestAssemblyMessage, _IAssemblyMetadata
	{
		string? assemblyName;
		string? testEnvironment;
		string? testFrameworkDisplayName;

		/// <inheritdoc/>
		public string AssemblyName
		{
			get => assemblyName ?? throw new InvalidOperationException($"Attempted to get {nameof(AssemblyName)} on an uninitialized '{GetType().FullName}' object");
			set => assemblyName = Guard.ArgumentNotNullOrEmpty(nameof(AssemblyName), value);
		}

		/// <inheritdoc/>
		public string? AssemblyPath { get; set; }

		/// <inheritdoc/>
		public string? ConfigFilePath { get; set; }

		/// <summary>
		/// Gets or sets the date and time when the test assembly execution began.
		/// </summary>
		public DateTimeOffset StartTime { get; set; }

		/// <summary>
		/// Gets or sets a display string that describes the test execution environment.
		/// Examples: "32-bit .NET Framework 4.8.4220.0", "64-bit .NET Core 4.6.29220.03"
		/// </summary>
		public string TestEnvironment
		{
			get => testEnvironment ?? throw new InvalidOperationException($"Attempted to get {nameof(TestEnvironment)} on an uninitialized '{GetType().FullName}' object");
			set => testEnvironment = Guard.ArgumentNotNullOrEmpty(nameof(TestEnvironment), value);
		}

		/// <summary>
		/// Gets or sets a display string which describes the test framework and version number.
		/// Examples: "xUnit.net v3 0.1.0-pre.15", "xUnit.net 2.4.1"
		/// </summary>
		public string TestFrameworkDisplayName
		{
			get => testFrameworkDisplayName ?? throw new InvalidOperationException($"Attempted to get {nameof(TestFrameworkDisplayName)} on an uninitialized '{GetType().FullName}' object");
			set => testFrameworkDisplayName = Guard.ArgumentNotNullOrEmpty(nameof(TestFrameworkDisplayName), value);
		}
	}
}
