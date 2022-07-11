using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="_ITest"/> for xUnit v3.
/// </summary>
public class XunitTest : _ITest
{
	readonly bool? @explicit;

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTest"/> class.
	/// </summary>
	/// <param name="testCase">The test case this test belongs to.</param>
	/// <param name="explicit">A flag to indicate the test was marked as explicit; if not set, will fall back to the test case</param>
	/// <param name="displayName">The display name for this test.</param>
	/// <param name="testIndex">The index of this test inside the test case. Used for computing <see cref="UniqueID"/>.</param>
	public XunitTest(
		IXunitTestCase testCase,
		bool? @explicit,
		string displayName,
		int testIndex)
	{
		TestCase = Guard.ArgumentNotNull(testCase);
		this.@explicit = @explicit;
		DisplayName = Guard.ArgumentNotNull(displayName);
		UniqueID = UniqueIDGenerator.ForTest(testCase.UniqueID, testIndex);
	}

	/// <summary>
	/// This constructor is for testing purposes only. Do not use in production code.
	/// </summary>
	public XunitTest(
		IXunitTestCase testCase,
		bool? @explicit,
		string displayName,
		string uniqueID)
	{
		TestCase = Guard.ArgumentNotNull(testCase);
		this.@explicit = @explicit;
		DisplayName = Guard.ArgumentNotNull(displayName);
		UniqueID = Guard.ArgumentNotNull(uniqueID);
	}

	/// <inheritdoc/>
	public string DisplayName { get; }

	/// <inheritdoc/>
	public bool Explicit => @explicit ?? TestCase.Explicit;

	/// <summary>
	/// Gets the xUnit v3 test case.
	/// </summary>
	public IXunitTestCase TestCase { get; }

	/// <inheritdoc/>
	_ITestCase _ITest.TestCase => TestCase;

	/// <inheritdoc/>
	public string UniqueID { get; }
}
