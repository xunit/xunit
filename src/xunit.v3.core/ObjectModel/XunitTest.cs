using System.Collections.Generic;
using System.Diagnostics;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="_ITest"/> for xUnit v3.
/// </summary>
[DebuggerDisplay("class = {TestCase.TestClassName}, method = {TestCase.TestMethodName}")]
public class XunitTest : IXunitTest
{
	static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> EmptyDictionary = new Dictionary<string, IReadOnlyList<string>>();

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTest"/> class.
	/// </summary>
	/// <param name="testCase">The test case this test belongs to.</param>
	/// <param name="testMethod">The test method to be run; may differ from the test method embedded into the test case</param>
	/// <param name="explicit">A flag to indicate the test was marked as explicit; if not set, will fall back to the test case</param>
	/// <param name="testDisplayName">The display name for this test.</param>
	/// <param name="testIndex">The index of this test inside the test case. Used for computing <see cref="UniqueID"/>.</param>
	/// <param name="traits">The traits for the given test.</param>
	/// <param name="timeout">The timeout for the test; if not set, will fall back to the test case</param>
	public XunitTest(
		IXunitTestCase testCase,
		IXunitTestMethod testMethod,
		bool? @explicit,
		string testDisplayName,
		int testIndex,
		IReadOnlyDictionary<string, IReadOnlyList<string>> traits,
		int? timeout)
	{
		TestCase = Guard.ArgumentNotNull(testCase);
		TestMethod = Guard.ArgumentNotNull(testMethod);
		Explicit = @explicit ?? TestCase.Explicit;
		TestDisplayName = Guard.ArgumentNotNull(testDisplayName);
		UniqueID = UniqueIDGenerator.ForTest(testCase.UniqueID, testIndex);
		Timeout = timeout ?? TestCase.Timeout;

		Guard.ArgumentNotNull(traits);

		var result = new Dictionary<string, IReadOnlyList<string>>(traits.Count);
		foreach (var kvp in traits)
			result.Add(kvp.Key, kvp.Value.CastOrToReadOnlyList());
		Traits = result;
	}

	/// <summary>
	/// This constructor is for testing purposes only. Do not use in production code.
	/// </summary>
	public XunitTest(
		IXunitTestCase testCase,
		IXunitTestMethod testMethod,
		bool? @explicit,
		string testDisplayName,
		string uniqueID,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		int? timeout = null)
	{
		TestCase = Guard.ArgumentNotNull(testCase);
		TestMethod = Guard.ArgumentNotNull(testMethod);
		Explicit = @explicit ?? TestCase.Explicit;
		TestDisplayName = Guard.ArgumentNotNull(testDisplayName);
		UniqueID = Guard.ArgumentNotNull(uniqueID);
		Timeout = timeout ?? TestCase.Timeout;

		if (traits is null)
			Traits = EmptyDictionary;
		else
		{
			var result = new Dictionary<string, IReadOnlyList<string>>(traits.Count);
			foreach (var kvp in traits)
				result.Add(kvp.Key, kvp.Value.CastOrToReadOnlyList());
			Traits = result;
		}
	}

	/// <inheritdoc/>
	public bool Explicit { get; }

	/// <summary>
	/// Gets the xUnit v3 test case.
	/// </summary>
	public IXunitTestCase TestCase { get; }

	/// <inheritdoc/>
	_ITestCase _ITest.TestCase => TestCase;

	/// <inheritdoc/>
	public string TestDisplayName { get; }

	/// <inheritdoc/>
	public IXunitTestMethod TestMethod { get; }

	/// <inheritdoc/>
	public int Timeout { get; }

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits { get; }

	/// <inheritdoc/>
	public string UniqueID { get; }
}
