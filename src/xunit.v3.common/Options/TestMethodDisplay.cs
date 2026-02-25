namespace Xunit.Sdk;

/// <summary>
/// Indicates the default display name format for test methods.
/// </summary>
public enum TestMethodDisplay
{
	/// <summary>
	/// Use a fully qualified name (namespace + class + method)
	/// </summary>
	ClassAndMethod = 1,

	/// <summary>
	/// Use just the method name (without class)
	/// </summary>
	Method = 2
}

/// <summary>
/// Extension methods for <see cref="TestMethodDisplay"/>
/// </summary>
public static class TestMethodDisplayExtensions
{
	extension(TestMethodDisplay)
	{
		/// <summary>
		/// Gets the valid values for <see cref="TestMethodDisplay"/>.
		/// </summary>
		public static HashSet<TestMethodDisplay> ValidValues =>
		[
			TestMethodDisplay.ClassAndMethod,
			TestMethodDisplay.Method,
		];
	}

	/// <summary>
	/// Determines if the value is a valid enum value.
	/// </summary>
	public static bool IsValid(this TestMethodDisplay value) =>
		TestMethodDisplay.ValidValues.Contains(value);
}
