using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="_ITest"/> for xUnit v3.
/// </summary>
public class XunitTest : _ITest
{
	static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> EmptyDictionary = new Dictionary<string, IReadOnlyList<string>>();

	readonly bool? @explicit;

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTest"/> class.
	/// </summary>
	/// <param name="testCase">The test case this test belongs to.</param>
	/// <param name="explicit">A flag to indicate the test was marked as explicit; if not set, will fall back to the test case</param>
	/// <param name="testDisplayName">The display name for this test.</param>
	/// <param name="testIndex">The index of this test inside the test case. Used for computing <see cref="UniqueID"/>.</param>
	/// <param name="traits">The traits for the given test.</param>
	/// <param name="timeout">The timeout for the test.</param>
	public XunitTest(
		IXunitTestCase testCase,
		bool? @explicit,
		string testDisplayName,
		int testIndex,
		IReadOnlyDictionary<string, IReadOnlyList<string>> traits,
		int timeout)
	{
		TestCase = Guard.ArgumentNotNull(testCase);
		this.@explicit = @explicit;
		TestDisplayName = Guard.ArgumentNotNull(testDisplayName);
		UniqueID = UniqueIDGenerator.ForTest(testCase.UniqueID, testIndex);
		Timeout = timeout;

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
		bool? @explicit,
		string testDisplayName,
		string uniqueID,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		int timeout = 0)
	{
		TestCase = Guard.ArgumentNotNull(testCase);
		this.@explicit = @explicit;
		TestDisplayName = Guard.ArgumentNotNull(testDisplayName);
		UniqueID = Guard.ArgumentNotNull(uniqueID);
		Timeout = timeout;

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
	public bool Explicit => @explicit ?? TestCase.Explicit;

	/// <summary>
	/// Gets the xUnit v3 test case.
	/// </summary>
	public IXunitTestCase TestCase { get; }

	/// <inheritdoc/>
	_ITestCase _ITest.TestCase => TestCase;

	/// <inheritdoc/>
	public string TestDisplayName { get; }

	/// <inheritdoc/>
	public int Timeout { get; }

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits { get; }

	/// <inheritdoc/>
	public string UniqueID { get; }
}
