namespace Xunit.SimpleRunner;

/// <summary>
/// A base class which contains information about a test.
/// </summary>
public abstract class TestInfo
{
	/// <summary>
	/// Gets the name of the method that contains the test.
	/// </summary>
	/// <remarks>
	/// For typical xUnit.net tests, this will not be <see langword="null"/>. However, some custom test
	/// frameworks may generate tests from sources other than methods, in which case this
	/// value may be <see langword="null"/>.
	/// </remarks>
	public required string? MethodName { get; set; }

	/// <summary>
	/// Gets the traits associated with the test.
	/// </summary>
	public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }

	/// <summary>
	/// Gets the display name of the test collection the test belongs to.
	/// </summary>
	public required string TestCollectionDisplayName { get; set; }

	/// <summary>
	/// Gets the display name for the test.
	/// </summary>
	public required string TestDisplayName { get; set; }

	/// <summary>
	/// Gets the fully qualified type name of the class that contains the test.
	/// </summary>
	/// <remarks>
	/// For typical xUnit.net tests, this will not be <see langword="null"/>. However, some custom test
	/// frameworks may generate tests from sources other than classes, in which case this
	/// value may be <see langword="null"/>.
	/// </remarks>
	public required string? TypeName { get; set; }
}
