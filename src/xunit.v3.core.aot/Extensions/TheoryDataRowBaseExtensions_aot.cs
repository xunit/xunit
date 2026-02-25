using Xunit.Sdk;

namespace Xunit;

partial class TheoryDataRowBaseExtensions
{
	/// <summary>
	/// Try to get a value from an array in a type-safe manner.
	/// </summary>
	public static (bool Success, T Result, object? RawValue) TryGet<T>(
		this object?[] data,
		int idx)
	{
		if (data is null || data.Length <= idx)
			return (false, default!, "<missing value>");

		var rawValue = data[idx];
		if (TypeHelper.TryConvert<T>(rawValue, out var valueAsT))
			return (true, valueAsT, rawValue);

		return (false, default!, rawValue);
	}

	/// <summary>
	/// Try to get a value from an array in a type-safe manner.
	/// </summary>
	public static (bool Success, T? Result, object? RawValue) TryGetNullable<T>(
		this object?[] data,
		int idx)
	{
		if (data is null || data.Length <= idx)
			return (false, default, "<missing value>");

		var rawValue = data[idx];
		if (TypeHelper.TryConvert<T>(rawValue, out var valueAsT))
			return (true, valueAsT, rawValue);

		if (rawValue is null)
			return (true, default, null);

		return (false, default, rawValue);
	}
}
