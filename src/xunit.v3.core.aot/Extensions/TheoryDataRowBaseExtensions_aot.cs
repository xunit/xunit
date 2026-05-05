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
	/// Try to get a nullable reference type value from an array in a type-safe manner.
	/// </summary>
	public static (bool Success, T? Result, object? RawValue) TryGetNullableClass<T>(
		this object?[] data,
		int idx)
			where T : class
	{
		if (data is null || data.Length <= idx)
			return (false, null, "<missing value>");

		var rawValue = data[idx];
		if (rawValue is null)
			return (true, null, null);

		if (TypeHelper.TryConvertNullable<T>(rawValue, out var valueAsT))
			return (true, valueAsT, rawValue);

		return (false, null, rawValue);
	}

	/// <summary>
	/// Try to get a nullable value type value from an array in a type-safe manner.
	/// </summary>
	public static (bool Success, T? Result, object? RawValue) TryGetNullableStruct<T>(
		this object?[] data,
		int idx)
			where T : struct
	{
		if (data is null || data.Length <= idx)
			return (false, null, "<missing value>");

		var rawValue = data[idx];
		if (rawValue is null)
			return (true, null, null);

		if (TypeHelper.TryConvertNullable<T>(rawValue, out var valueAsT))
			return (true, valueAsT, rawValue);

		return (false, null, rawValue);
	}
}
