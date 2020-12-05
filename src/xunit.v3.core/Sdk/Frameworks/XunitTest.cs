using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// An implementation of <see cref="_ITest"/> for xUnit v3.
	/// </summary>
	public class XunitTest : _ITest
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTest"/> class.
		/// </summary>
		/// <param name="testCase">The test case this test belongs to.</param>
		/// <param name="displayName">The display name for this test.</param>
		public XunitTest(IXunitTestCase testCase, string displayName)
		{
			TestCase = Guard.ArgumentNotNull(nameof(testCase), testCase);
			DisplayName = Guard.ArgumentNotNull(nameof(displayName), displayName);
		}

		/// <inheritdoc/>
		public string DisplayName { get; }

		/// <summary>
		/// Gets the xUnit v3 test case.
		/// </summary>
		public IXunitTestCase TestCase { get; }

		/// <inheritdoc/>
		ITestCase _ITest.TestCase => TestCase;
	}
}
