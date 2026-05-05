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
	/// This method is typically used for argument coercion by source generators. The steps it takes include:
	/// <list type="bullet">
	/// <item>If <paramref name="arg"/> is already the correct type, returns <see langword="true"/> with <paramref name="result"/>
	/// set to <paramref name="arg"/></item>
	/// <item>If <typeparamref name="T"/> is an enum, calls <see cref="Enum.ToObject(Type, object)"/> and returns
	/// <see langword="true"/> with <paramref name="result"/> set to the converted value</item>
	/// <item>If <paramref name="arg"/> is <see cref="string"/> and <typeparamref name="T"/> is one of <see cref="Guid"/>,
	/// <see cref="DateTime"/>, or <see cref="DateTimeOffset"/>, tries to parse the string value, and if successful,
	/// returns <see langword="true"/> with <paramref name="result"/> set to the converted value</item>
	/// <item>If <see cref="Convert.ChangeType(object?, Type, IFormatProvider?)"/> can convert the item to the target type,
	/// returns <see langword="true"/> with <paramref name="result"/> set to the converted value</item>
	/// <item>Returns <see langword="false"/> with <paramref name="result"/> set to <see langword="default"/></item>
	/// </list>
	/// Notably, this does not support implicit or explicit conversion operator methods, which are not available via reflection
	/// in Native AOT.
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
	/// Attempts to convert a nullable reference type value to <c><typeparamref name="T"/>?</c>.
	/// </summary>
	/// <typeparam name="T">The desired destination type</typeparam>
	/// <param name="arg">The value to try to convert</param>
	/// <param name="result">The resulting converted value, if the return value is <see langword="true"/>
	/// (or <see langword="null"/> if the return value is <see langword="false"/>).</param>
	/// <remarks>
	/// This method is typically used for argument coercion by source generators. The steps it takes include:
	/// <list type="bullet">
	/// <item>If <paramref name="arg"/> is <see langword="null"/>, returns <see langword="true"/> with <paramref name="result"/>
	/// set to <see langword="null"/></item>
	/// <item>If <paramref name="arg"/> is already the correct type, returns <see langword="true"/> with <paramref name="result"/>
	/// set to <paramref name="arg"/></item>
	/// <item>If <see cref="Convert.ChangeType(object?, Type, IFormatProvider?)"/> can convert the item to the target type,
	/// returns <see langword="true"/> with <paramref name="result"/> set to the converted value</item>
	/// <item>Returns <see langword="false"/> with <paramref name="result"/> set to <see langword="null"/></item>
	/// </list>
	/// Notably, this does not support implicit or explicit conversion operator methods, which are not available via reflection
	/// in Native AOT.
	/// </remarks>
	public static bool TryConvertNullable<T>(
		object? arg,
		out T? result)
			where T : class
	{
		if (arg is null)
		{
			result = null;
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
			if (Convert.ChangeType(arg, type, CultureInfo.CurrentCulture) is T valueAsChangeType)
			{
				result = valueAsChangeType;
				return true;
			}
		}
		catch { }

		result = null;
		return false;
	}


	/// <summary>
	/// Attempts to convert a nullable value type value to <see cref="Nullable{T}"/>.
	/// </summary>
	/// <typeparam name="T">The desired destination type</typeparam>
	/// <param name="arg">The value to try to convert</param>
	/// <param name="result">The resulting converted value, if the return value is <see langword="true"/>
	/// (or <see langword="null"/> if the return value is <see langword="false"/>).</param>
	/// <remarks>
	/// This method is typically used for argument coercion by source generators. The steps it takes include:
	/// <list type="bullet">
	/// <item>If <paramref name="arg"/> is <see langword="null"/>, returns <see langword="true"/> with <paramref name="result"/>
	/// set to <see langword="null"/></item>
	/// <item>If <paramref name="arg"/> is already the correct type, returns <see langword="true"/> with <paramref name="result"/>
	/// set to <paramref name="arg"/></item>
	/// <item>If <typeparamref name="T"/> is an enum, calls <see cref="Enum.ToObject(Type, object)"/> and returns
	/// <see langword="true"/> with <paramref name="result"/> set to the converted value</item>
	/// <item>If <paramref name="arg"/> is <see cref="string"/> and <typeparamref name="T"/> is one of <see cref="Guid"/>,
	/// <see cref="DateTime"/>, or <see cref="DateTimeOffset"/>, tries to parse the string value, and if successful,
	/// returns <see langword="true"/> with <paramref name="result"/> set to the converted value</item>
	/// <item>If <see cref="Convert.ChangeType(object?, Type, IFormatProvider?)"/> can convert the item to the target type,
	/// returns <see langword="true"/> with <paramref name="result"/> set to the converted value</item>
	/// <item>Returns <see langword="false"/> with <paramref name="result"/> set to <see langword="null"/></item>
	/// </list>
	/// Notably, this does not support implicit or explicit conversion operator methods, which are not available via reflection
	/// in Native AOT.
	/// </remarks>
	public static bool TryConvertNullable<T>(
		object? arg,
		out T? result)
			where T : struct
	{
		if (arg is null)
		{
			result = null;
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

		result = null;
		return false;
	}
}
