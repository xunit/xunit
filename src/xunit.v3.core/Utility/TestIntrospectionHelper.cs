namespace Xunit.v3;

public static partial class TestIntrospectionHelper
{
#if XUNIT_AOT
	/// <summary>
	/// Merges string-array traits (like from <see cref="DataAttribute.Traits"/>) into an existing traits dictionary.
	/// </summary>
	/// <param name="traits">The existing traits dictionary.</param>
	/// <param name="additionalTraits">The additional traits to merge.</param>
#else
	/// <summary>
	/// Merges string-array traits (like from <see cref="IDataAttribute.Traits"/>) into an existing traits dictionary.
	/// </summary>
	/// <param name="traits">The existing traits dictionary.</param>
	/// <param name="additionalTraits">The additional traits to merge.</param>
#endif
	public static void MergeTraitsInto(
		Dictionary<string, HashSet<string>> traits,
		string[]? additionalTraits)
	{
		if (additionalTraits is null)
			return;

		var idx = 0;

		while (idx < additionalTraits.Length - 1)
		{
			traits.AddOrGet(additionalTraits[idx]).Add(additionalTraits[idx + 1]);
			idx += 2;
		}
	}
}
