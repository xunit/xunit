namespace Xunit.Sdk;

/// <summary>
/// Options representing how much parallelization to allow within the test execution pipeline.
/// </summary>
/// <remarks>
/// For xUnit.net v2 and xUnit.net v3 <br/>(&lt; 4.0.0) test projects only the following options are supported:<br/>
/// <em><see cref="None"/>, <see cref="Assemblies"/>, and <see cref="Collections"/>.</em>
/// </remarks>
[Flags]
public enum ParallelismOptions
{
	/// <summary>
	/// No test parallelization.
	/// </summary>
	/// <remarks>
	/// <para><em>Only supported for xUnit.net v2 and xUnit.net v3 test projects.</em></para>
	/// </remarks>
	None = 0,

	/// <summary>
	/// Run test assemblies in parallel.
	/// </summary>
	/// <remarks>
	/// <para><em>Only supported for xUnit.net v2 and xUnit.net v3 test projects.</em></para>
	/// </remarks>
	Assemblies = 1 << 0,

	/// <summary>
	/// Run test collections in an assembly in parallel.
	/// </summary>
	/// <remarks>
	/// <para><em>Only supported for xUnit.net v2 and xUnit.net v3 test projects.</em></para>
	/// </remarks>
	Collections = 1 << 1,

	/// <summary>
	/// Run the test classes of a test collection in parallel.
	/// </summary>
	/// <remarks>
	/// <para><em>Only supported for xUnit.net v3 (4.0.0+) test projects.</em></para>
	/// </remarks>
	Classes = 1 << 2,

	/// <summary>
	/// Run the test methods of a test class in parallel.
	/// </summary>
	/// <remarks>
	/// <para><em>Only supported for xUnit.net v3 (4.0.0+) test projects.</em></para>
	/// </remarks>
	Methods = 1 << 3,

	/// <summary>
	/// Run the test cases of a test method in parallel.
	/// </summary>
	/// <remarks>
	/// <para><em>Only supported for xUnit.net v3 (4.0.0+) test projects.</em></para>
	/// </remarks>
	TestCases = 1 << 4,

	/// <summary>
	/// Run the tests of a test case in parallel.
	/// </summary>
	/// <remarks>
	/// <para><em>Only supported for xUnit.net v3 (4.0.0+) test projects.</em></para>
	/// </remarks>
	Tests = 1 << 5,

	/// <summary>
	/// Enable maximum parallelization for the test execution pipeline.
	/// </summary>
	/// <remarks>
	/// <para><em>Only supported for xUnit.net v3 (4.0.0+) test projects.</em></para>
	/// </remarks>
	All = Assemblies | Collections | Classes | Methods | TestCases | Tests,
}

/// <summary>
/// Aliases for <see cref="ParallelismOptions"/> configurations.
/// </summary>
/// <remarks>
/// Used to avoid duplicate enum values which lead to undefined behavior when converting the enum to a string.
/// </remarks>
public static class ParallelismOptionsAliases
{
	/// <summary>
	/// The default parallelism options used for the test execution pipeline.
	/// </summary>
	public const ParallelismOptions Default = ParallelismOptions.Collections;
}

/// <summary>
/// Extension methods for <see cref="ParallelismOptions"/>
/// </summary>
public static class ParallelismOptionsExtensions
{
	extension(ParallelismOptions)
	{
		/// <summary>
		/// Gets the valid values for <see cref="ParallelismOptions"/>.
		/// </summary>
		public static ParallelismOptions[] ValidValues =>
		[
			ParallelismOptions.None, ParallelismOptions.Assemblies, ParallelismOptions.Collections,
			ParallelismOptions.Classes, ParallelismOptions.Methods, ParallelismOptions.TestCases,
			ParallelismOptions.Tests, ParallelismOptions.All
		];
	}
}
