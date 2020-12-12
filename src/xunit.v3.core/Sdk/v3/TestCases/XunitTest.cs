using Xunit.Internal;

namespace Xunit.v3
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
		/// <param name="testIndex">The index of this test inside the test case. Used for computing <see cref="UniqueID"/>.</param>
		public XunitTest(
			IXunitTestCase testCase,
			string displayName,
			int testIndex)
		{
			TestCase = Guard.ArgumentNotNull(nameof(testCase), testCase);
			DisplayName = Guard.ArgumentNotNull(nameof(displayName), displayName);
			UniqueID = UniqueIDGenerator.ForTest(testCase.UniqueID, testIndex);
		}

		/// <inheritdoc/>
		public string DisplayName { get; }

		/// <summary>
		/// Gets the xUnit v3 test case.
		/// </summary>
		public IXunitTestCase TestCase { get; }

		/// <inheritdoc/>
		_ITestCase _ITest.TestCase => TestCase;

		/// <inheritdoc/>
		public string UniqueID { get; }
	}
}
