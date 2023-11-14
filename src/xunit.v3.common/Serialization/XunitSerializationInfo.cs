using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Represents serialization information for serializing a complex object. This is typically
/// used by objects which implement <see cref="IXunitSerializable"/>.
/// </summary>
public class XunitSerializationInfo : IXunitSerializationInfo
{
	static readonly char[] colonSeparator = new[] { ':' };
	readonly Dictionary<string, string> data = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitSerializationInfo"/> class
	/// for the purposes of serialization (starting empty).
	/// </summary>
	public XunitSerializationInfo()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitSerializationInfo"/> class
	/// for the purposes of serialization (starting populated by the given object).
	/// </summary>
	/// <param name="object">The data to copy into the serialization info</param>
	public XunitSerializationInfo(IXunitSerializable @object)
	{
		Guard.ArgumentNotNull(@object);

		@object.Serialize(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitSerializationInfo"/> class
	/// for the purposes of deserialization.
	/// </summary>
	/// <param name="serializedValue">The serialized value to copy into the serialization info</param>
	public XunitSerializationInfo(string serializedValue)
	{
		// Will end up with an empty string if the serialization type did not serialize any data
		if (string.IsNullOrWhiteSpace(serializedValue))
			return;

		var decodedValue = SerializationHelper.FromBase64(serializedValue);

		foreach (var element in decodedValue.Split('\n'))
		{
			var pieces = element.Split(colonSeparator, 2);
			if (pieces.Length != 2)
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Serialized piece '{0}' is malformed. Full serialization:{1}{2}", element, Environment.NewLine, decodedValue), nameof(serializedValue));

			data[pieces[0]] = pieces[1];
		}
	}

	/// <inheritdoc/>
	public void AddValue(
		string key,
		object? value,
		_ITypeInfo? valueTypeInfo = null)
	{
		if (valueTypeInfo is null)
			valueTypeInfo = Reflector.Wrap(value?.GetType()) ?? SerializationHelper.TypeInfo_Object;

		if (!SerializationHelper.IsSerializable(value, valueTypeInfo))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize a value of type '{0}': unsupported type for serialization", valueTypeInfo.Name), nameof(value));

		data.Add(key, SerializationHelper.Serialize(value, valueTypeInfo));
	}

	/// <inheritdoc/>
	public object? GetValue(string key)
	{
		if (data.TryGetValue(key, out var value))
			return SerializationHelper.Deserialize(value);

		return null;
	}

	/// <summary>
	/// Returns Base64 encoded string that represents the entirety of the data.
	/// </summary>
	public string ToSerializedString() =>
		SerializationHelper.ToBase64(
			string.Join("\n", data.Select(kvp => string.Format(CultureInfo.InvariantCulture, "{0}:{1}", kvp.Key, kvp.Value)))
		);
}
