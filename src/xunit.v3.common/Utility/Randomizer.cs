#pragma warning disable CA5394  // Cryptographically secure randomness is not needed here

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
		Current = new ThreadSafeRandom(seed);
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
			Current = new ThreadSafeRandom(seed);
		}
	}

	sealed class ThreadSafeRandom(int seed) :
		Random(seed)
	{
		readonly object lockObject = new();

		public override int Next()
		{
			lock (lockObject)
				return base.Next();
		}

		public override int Next(int maxValue)
		{
			lock (lockObject)
				return base.Next(maxValue);
		}

		public override int Next(int minValue, int maxValue)
		{
			lock (lockObject)
				return base.Next(minValue, maxValue);
		}

		public override void NextBytes(byte[] buffer)
		{
			lock (lockObject)
				base.NextBytes(buffer);
		}

		public override double NextDouble()
		{
			lock (lockObject)
				return base.NextDouble();
		}

		protected override double Sample()
		{
			lock (lockObject)
				return base.Sample();
		}
	}
}
