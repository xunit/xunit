namespace Xunit.v3
{
	/// <summary>
	/// Represents a test class.
	/// </summary>
	public interface _ITestClass
	{
		/// <summary>
		/// Gets the class that this test case is attached to.
		/// </summary>
		_ITypeInfo Class { get; }

		/// <summary>
		/// Gets the test collection this test case belongs to.
		/// </summary>
		_ITestCollection TestCollection { get; }

		/// <summary>
		/// Gets the unique ID for this test class.
		/// </summary>
		string UniqueID { get; }
	}
}
