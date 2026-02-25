namespace Xunit.Sdk;

/// <summary>
/// Utility methods related to <see cref="Type"/>.
/// </summary>
public static partial class TypeHelper
{
	/// <summary>
	/// Attempts to convert a value to type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The desired destination type</typeparam>
	/// <param name="arg">The value to try to convert</param>
	/// <param name="result">The resulting converted value, if the return value is <see langword="true"/>
	/// (or <see langword="default"/> if the return value is <see langword="false"/>).</param>
	/// <remarks>
	/// This method is typically used for argument coercion by source generators, especially for values that
	/// don't have appropriate support in <c>[InlineData]</c>. The supported type coercion includes:
	/// <list type="bullet">
	/// <item>String values to <see cref="DateTime"/></item>
	/// <item>String values to <see cref="DateTimeOffset"/></item>
	/// <item>String or integral values to <see cref="Enum"/></item>
	/// <item>String values to <see cref="Guid"/></item>
	/// </list>
	/// As a final step, it will call <see cref="Convert.ChangeType(object?, Type, IFormatProvider?)"/> to support
	/// any built-in system type coercion. Notably, this does not support implicit or explicit conversion operator
	/// methods, which are not available via reflection in Native AOT.
	/// </remarks>
	public static bool TryConvert<T>(
		object? arg,
		out T result)
	{
		if (arg is T valueAsT)
		{
			result = valueAsT;
			return true;
		}

		var type = typeof(T);

		if (arg is not null)
		{
			try
			{
				if (type.IsEnum)
				{
					result = (T)Enum.ToObject(type, arg);
					return true;
				}

#pragma warning disable CA1508 // Dear compiler, I need the "dead conditional code" to do the casting

				if (type == typeof(Guid) && arg.ToString() is string stringGuid && Guid.Parse(stringGuid) is T valueAsGuid)
				{
					result = valueAsGuid;
					return true;
				}

				if (type == typeof(DateTime) && arg.ToString() is string stringDateTime && DateTime.Parse(stringDateTime, CultureInfo.InvariantCulture) is T valueAsDateTime)
				{
					result = valueAsDateTime;
					return true;
				}

				if (type == typeof(DateTimeOffset) && arg.ToString() is string stringDateTimeOffset && DateTimeOffset.Parse(stringDateTimeOffset, CultureInfo.InvariantCulture) is T valueAsDateTimeOffset)
				{
					result = valueAsDateTimeOffset;
					return true;
				}

#pragma warning restore CA1508

				if (Convert.ChangeType(arg, type, CultureInfo.CurrentCulture) is T valueAsChangeType)
				{
					result = valueAsChangeType;
					return true;
				}
			}
			catch { }
		}

		result = default!;
		return false;
	}

	/// <summary>
	/// Attempts to convert a nullable value to type <c><typeparamref name="T"/>?</c>.
	/// </summary>
	/// <typeparam name="T">The desired destination type</typeparam>
	/// <param name="arg">The value to try to convert</param>
	/// <param name="result">The resulting converted value, if the return value is <see langword="true"/>
	/// (or <see langword="default"/> if the return value is <see langword="false"/>).</param>
	/// <remarks>
	/// This method is typically used for argument coercion by source generators, especially for values that
	/// don't have appropriate support in <c>[InlineData]</c>. The supported type coercion includes:
	/// <list type="bullet">
	/// <item>String values to <see cref="DateTime"/></item>
	/// <item>String values to <see cref="DateTimeOffset"/></item>
	/// <item>String or integral values to <see cref="Enum"/></item>
	/// <item>String values to <see cref="Guid"/></item>
	/// </list>
	/// As a final step, it will call <see cref="Convert.ChangeType(object?, Type, IFormatProvider?)"/> to support
	/// any built-in system type coercion. Notably, this does not support implicit or explicit conversion operator
	/// methods, which are not available via reflection in Native AOT.
	/// </remarks>
	public static bool TryConvertNullable<T>(
		object? arg,
		out T? result)
	{
		if (arg is null)
		{
			result = default;
			return true;
		}

		if (arg is T valueAsT)
		{
			result = valueAsT;
			return true;
		}

		var type = typeof(T);

		try
		{
			if (type.IsEnum)
			{
				result = (T)Enum.ToObject(type, arg);
				return true;
			}

#pragma warning disable CA1508 // Dear compiler, I need the "dead conditional code" to do the casting

			if (type == typeof(Guid) && arg.ToString() is string stringGuid && Guid.Parse(stringGuid) is T valueAsGuid)
			{
				result = valueAsGuid;
				return true;
			}

			if (type == typeof(DateTime) && arg.ToString() is string stringDateTime && DateTime.Parse(stringDateTime, CultureInfo.InvariantCulture) is T valueAsDateTime)
			{
				result = valueAsDateTime;
				return true;
			}

			if (type == typeof(DateTimeOffset) && arg.ToString() is string stringDateTimeOffset && DateTimeOffset.Parse(stringDateTimeOffset, CultureInfo.InvariantCulture) is T valueAsDateTimeOffset)
			{
				result = valueAsDateTimeOffset;
				return true;
			}

#pragma warning restore CA1508

			if (Convert.ChangeType(arg, type, CultureInfo.CurrentCulture) is T valueAsChangeType)
			{
				result = valueAsChangeType;
				return true;
			}
		}
		catch { }

		result = default;
		return false;
	}
}
