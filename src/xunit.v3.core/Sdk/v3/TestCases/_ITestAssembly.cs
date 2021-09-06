using System;

namespace Xunit.v3
{
	/// <summary>
	/// Represents a test assembly.
	/// </summary>
	public interface _ITestAssembly
	{
		/// <summary>
		/// Gets the assembly that this test assembly belongs to.
		/// </summary>
		_IAssemblyInfo Assembly { get; }

		/// <summary>
		/// Gets the full path of the configuration file name, if one is present.
		/// May be <c>null</c> if there is no configuration file.
		/// </summary>
		string? ConfigFileName { get; }

		/// <summary>
		/// Gets the unique ID for this test assembly.
		/// </summary>
		string UniqueID { get; }

		/// <summary>
		/// Gets the assembly version. If the version is not known, this may return 0.0.0.0.
		/// </summary>
		Version Version { get; }
	}
}
