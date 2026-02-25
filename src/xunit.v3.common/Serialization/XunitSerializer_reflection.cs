namespace Xunit.Sdk;

/// <summary>
/// A helper class that makes implementing <see cref="IXunitSerializer"/> more type-safe.
/// </summary>
/// <typeparam name="T">The type of the object being serialized</typeparam>
/// <remarks>
/// This class is only suitable for implementing a serializer bound to a single type, which
/// must not be nullable (i.e., must be a struct or a non-nullable reference type).
/// </remarks>
public abstract class XunitSerializer<T> : IXunitSerializer
	where T : notnull
{
	/// <summary>
	/// Deserializes a value that was obtained from <see cref="Serialize"/>.
	/// </summary>
	/// <param name="type">The type of the original value</param>
	/// <param name="serializedValue">The serialized value</param>
	/// <returns>The deserialized value</returns>
	public abstract T Deserialize(
		Type type,
		string serializedValue);

	object IXunitSerializer.Deserialize(
		Type type,
		string serializedValue)
	{
		Guard.ArgumentNotNull(type);
		Guard.ArgumentNotNullOrEmpty(serializedValue);

		if (!typeof(T).IsAssignableFrom(type))
			throw new ArgumentException(TypeIncompatibleDeserialization(type), nameof(type));

		return Deserialize(type, serializedValue);
	}

	/// <summary>
	/// Determines if a specific value of data is serializable.
	/// </summary>
	/// <param name="value">The value to test</param>
	/// <param name="failureReason">Returns a failure reason when the value isn't serializable</param>
	/// <returns>Return <see langword="true"/> if the value is serializable; <see langword="false"/>, otherwise</returns>
	/// <remarks>
	/// This will be called by <see cref="SerializationHelper.IsSerializable(object?)"/>,
	/// <see cref="SerializationHelper.IsSerializable(object?, Type?)"/>, and
	/// <see cref="SerializationHelper.Serialize"/>. The failure reason is used when
	/// called from <c>Serialize</c> to format an error exception, but is otherwise ignored
	/// from the calls from <c>IsSerializable</c>.<br />
	/// <br />
	/// When <paramref name="value"/> is an array, this method will be called once for each element
	/// in the array. This differs in behavior from <see cref="IXunitSerializer.IsSerializable"/>, which
	/// is only called once with the array passed in <paramref name="value"/>.<br />
	/// <br />
	/// The default implementation of this method just returns <see langword="true"/>; you only need to override
	/// this if there are situations values might not be serializable (for example, if the custom type
	/// contains potentially non-serializable data).
	/// </remarks>
	public virtual bool IsSerializable(
		T value,
		[NotNullWhen(false)] out string? failureReason)
	{
		failureReason = null;
		return true;
	}

	bool IXunitSerializer.IsSerializable(
		Type type,
		object? value,
		[NotNullWhen(false)] out string? failureReason)
	{
		Guard.ArgumentNotNull(type);

		return IsSerializableImpl(testForArray: true, value, out failureReason);

		bool IsSerializableImpl(
			bool testForArray,
			object? value,
			[NotNullWhen(false)] out string? failureReason)
		{
			if (value is null)
			{
				failureReason = null;
				return true;
			}

			if (testForArray && value is Array arrayValue)
			{
				foreach (var innerValue in arrayValue)
					if (!IsSerializableImpl(testForArray: false, innerValue, out failureReason))
						return false;

				failureReason = null;
				return true;
			}

			if (value is not T typedValue)
			{
				failureReason = TypeIncompatibleSerialization(value.GetType());
				return false;
			}

			return IsSerializable(typedValue, out failureReason);
		}
	}

	/// <summary>
	/// Serializes a value into a string to be later deserialized with <see cref="Deserialize"/>.
	/// </summary>
	/// <param name="value">The value to be serialized</param>
	/// <returns>The serialized value</returns>
	/// <remarks>
	/// This method will never be called with <see langword="null"/> values, because those are already
	/// special cased by the serialization system. You may assume that <see cref="IsSerializable"/>
	/// is called before this, so any validation done there need not be repeated here.
	/// </remarks>
	public abstract string Serialize(T value);

	string IXunitSerializer.Serialize(object value)
	{
		Guard.ArgumentNotNull(value);

		if (value is not T typedValue)
			throw new ArgumentException(TypeIncompatibleSerialization(value.GetType()), nameof(value));

		return Serialize(typedValue);
	}

	/// <summary>
	/// Gets an error message indicating type incompatbility, usable for <see cref="Deserialize"/>.
	/// </summary>
	/// <param name="type">The incompatible type</param>
	protected static string TypeIncompatibleDeserialization(Type type) =>
		string.Format(
			CultureInfo.CurrentCulture,
			"Cannot deserialize type '{0}' because it is not type compatible with '{1}'.",
			type.SafeName(),
			typeof(T).SafeName()
		);

	/// <summary>
	/// Gets an error message indicating type incompatbility, usable for <see cref="IsSerializable"/>
	/// and <see cref="Serialize"/>.
	/// </summary>
	/// <param name="type">The incompatible type</param>
	protected static string TypeIncompatibleSerialization(Type type) =>
		string.Format(
			CultureInfo.CurrentCulture,
			"Cannot serialize value of type '{0}' because it is not type compatible with '{1}'.",
			type.SafeName(),
			typeof(T).SafeName()
		);
}
