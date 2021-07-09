namespace Xunit.Runner.Common
{
	/// <summary>
	/// Indicates the kind of list a runner should generate, rather than running tests.
	/// </summary>
	public enum ListOption
	{
		/// <summary>
		/// Lists all the classes in the assembly which contain tests.
		/// </summary>
		Classes = 1,

		/// <summary>
		/// Lists full metadata about the test discovery.
		/// </summary>
		Full,

		/// <summary>
		/// Lists all the methods in the assembly which contain a test.
		/// </summary>
		Methods,

		/// <summary>
		/// Lists all the tests (as display name) in the assembly.
		/// </summary>
		Tests,

		/// <summary>
		/// Lists all the traits that are generated from the assembly.
		/// </summary>
		Traits,
	}
}
