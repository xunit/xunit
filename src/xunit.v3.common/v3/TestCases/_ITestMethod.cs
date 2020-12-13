using Xunit.Abstractions;

namespace Xunit.v3
{
	/// <summary>
	/// Represents a test method.
	/// </summary>
	public interface _ITestMethod
	{
		/// <summary>
		/// Gets the method associated with this test method.
		/// </summary>
		IMethodInfo Method { get; }

		/// <summary>
		/// Gets the test class that this test method belongs to.
		/// </summary>
		_ITestClass TestClass { get; }
	}
}
