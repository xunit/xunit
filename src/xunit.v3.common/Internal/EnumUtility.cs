namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class EnumUtility
{
	/// <summary/>
	public static bool ContainsValidFlags(
		int value,
		int[] validFlags)
	{
		Guard.ArgumentNotNull(validFlags);

		foreach (var flag in validFlags)
			value &= ~flag;

		return value == 0;
	}
}
