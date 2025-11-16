using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="ITestClassOrderer"/>. Orders tests in
/// an unpredictable but stable order, so that repeated test runs of the
/// identical test assembly run tests in the same order.
/// </summary>
[method: Obsolete("Please use the singleton instance available via the Instance property")]
[method: EditorBrowsable(EditorBrowsableState.Never)]
public class DefaultTestClassOrderer() : ITestClassOrderer
{
	/// <summary>
	/// Gets the singleton instance of <see cref="DefaultTestClassOrderer"/>.
	/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
	public static DefaultTestClassOrderer Instance { get; } = new();
#pragma warning restore CS0618 // Type or member is obsolete

	/// <inheritdoc/>
	public IReadOnlyCollection<TTestClass?> OrderTestClasses<TTestClass>(IReadOnlyCollection<TTestClass?> testClasses)
		where TTestClass : notnull, ITestClass
	{
		var result = testClasses.ToList();

		try
		{
			result.Sort(Compare);
		}
		catch (Exception ex)
		{
			TestContext.Current.SendDiagnosticMessage("Exception thrown in DefaultTestClassOrderer.OrderTestClasses(); falling back to random order.{0}{1}", Environment.NewLine, ex);
			result = Randomize(result);
		}

		return result;
	}

#pragma warning disable CA5394 // Cryptograph randomness is not necessary here

	static List<TTestClass> Randomize<TTestClass>(List<TTestClass> testCases)
	{
		var result = new List<TTestClass>(testCases.Count);
		var randomizer = Randomizer.Current;

		while (testCases.Count > 0)
		{
			var next = randomizer.Next(testCases.Count);
			result.Add(testCases[next]);
			testCases.RemoveAt(next);
		}

		return result;
	}

#pragma warning restore CA5394

	static int Compare<TTestClass>(
		TTestClass? x,
		TTestClass? y)
			where TTestClass : notnull, ITestClass
	{
		if (x is null)
			return y is null ? 0 : -1;
		if (y is null)
			return 1;

		Guard.ArgumentNotNull(x.UniqueID);
		Guard.ArgumentNotNull(y.UniqueID);

		return string.CompareOrdinal(x.UniqueID, y.UniqueID);
	}
}
