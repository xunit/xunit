using System;

namespace Xunit.Internal;

internal readonly struct Maybe<T>(T? value) : IEquatable<Maybe<T>>
{
	public T? Value { get; } = value;

	public bool Equals(Maybe<T> other) =>
		Value is null ? other.Value is null : Value.Equals(other.Value);

	public override bool Equals(object? obj)
	{
		if (obj is null)
			return Value is null;

		if (obj is Maybe<T> other)
			return Equals(other);

		return false;
	}

	public override int GetHashCode() =>
		Value is null ? 0 : Value.GetHashCode();

	public static Maybe<T> Nothing() =>
		new();

	public static implicit operator T?(Maybe<T> maybe) =>
		maybe.Value;

	public static implicit operator Maybe<T>(T? value) =>
		new(value);
}
