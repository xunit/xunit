using System;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents available parallelism options
/// </summary>
[Flags]
public enum ParallelismOption
{
	/// <summary>
	/// Do not parallelize any tests.
	/// </summary>
	none = 0,

	/// <summary>
	/// Run tests in different assemblies in parallel
	/// </summary>
	assemblies = 0x01,

	/// <summary>
	/// Run tests in different test collections in parallel
	/// </summary>
	collections = 0x02,

	/// <summary>
	/// Enable maximum parallelization
	/// </summary>
	all = assemblies | collections
}
