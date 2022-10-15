using System;

namespace Xunit.Sdk;

/// <summary>
/// Wraps <see cref="Random"/> to provide access to the seed value, as well as
/// the ability to reset the current randomizer with a new seed value.
/// </summary>
public static class Randomizer
{
	static int seed;

	static Randomizer()
	{
		seed = Environment.TickCount;
		Current = new(seed);
	}

	/// <summary>
	/// Gets the current instance that returns random values based on the
	/// current <see cref="Seed"/> value.
	/// </summary>
	public static Random Current { get; private set; }

	/// <summary>
	/// Gets the seed used to create the randomizer.
	/// </summary>
	public static int Seed
	{
		get => seed;
		set
		{
			seed = value;
			Current = new(seed);
		}
	}
}
