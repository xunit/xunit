using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Represents serialization information for serializing a complex object. This is typically
/// used by objects which implement <see cref="IXunitSerializable"/>.
/// </summary>
public class XunitSerializationInfo : IXunitSerializationInfo
{
	static readonly char[] colonSeparator = [':'];
	readonly Dictionary<string, string> data = [];
	readonly SerializationHelper serializationHelper;

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitSerializationInfo"/> class
	/// for the purposes of serialization (starting empty).
	/// </summary>
	/// <param name="serializationHelper">The serialization helper</param>
	public XunitSerializationInfo(SerializationHelper serializationHelper) =>
		this.serializationHelper = Guard.ArgumentNotNull(serializationHelper);

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitSerializationInfo"/> class
	/// for the purposes of serialization (starting populated by the given object).
	/// </summary>
	/// <param name="object">The data to copy into the serialization info</param>
	/// <param name="serializationHelper">The serialization helper</param>
	public XunitSerializationInfo(
		SerializationHelper serializationHelper,
		IXunitSerializable @object)
	{
		Guard.ArgumentNotNull(@object);

		this.serializationHelper = Guard.ArgumentNotNull(serializationHelper);

		@object.Serialize(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitSerializationInfo"/> class
	/// for the purposes of deserialization.
	/// </summary>
	/// <param name="serializationHelper">The serialization helper</param>
	/// <param name="serializedValue">The serialized value to copy into the serialization info</param>
	public XunitSerializationInfo(
		SerializationHelper serializationHelper,
		string serializedValue)
	{
		this.serializationHelper = Guard.ArgumentNotNull(serializationHelper);

		// Will end up with an empty string if the serialization type did not serialize any data
		if (string.IsNullOrWhiteSpace(serializedValue))
			return;

		foreach (var element in serializedValue.Split('\n'))
		{
			var pieces = element.Split(colonSeparator, 2);
			if (pieces.Length != 2)
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Serialized piece '{0}' is malformed. Full serialization:{1}{2}", element, Environment.NewLine, serializedValue), nameof(serializedValue));

			data[pieces[0]] = pieces[1];
		}
	}

	/// <inheritdoc/>
	public void AddValue(
		string key,
		object? value,
		Type? valueType = null) =>
			data.Add(key, serializationHelper.Serialize(value, valueType ?? value?.GetType() ?? typeof(object)));

	/// <inheritdoc/>
	public object? GetValue(string key) =>
		data.TryGetValue(key, out var value)
			? serializationHelper.Deserialize(value)
			: null;

	/// <summary>
	/// Returns a string that represents the entirety of the data.
	/// </summary>
	public string ToSerializedString() =>
		string.Join("\n", data.Select(kvp => string.Format(CultureInfo.InvariantCulture, "{0}:{1}", kvp.Key, kvp.Value)));
}
